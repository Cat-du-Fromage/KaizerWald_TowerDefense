using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace TowerDefense
{
    public class EnemyManager : MonoBehaviour
    {
        //References to Grids
        [SerializeField] private SandBox_SpatialPartition spatialPartition;
        [SerializeField] private PathfindingGrid PathfindingGrid;

        //Prefabs
        [SerializeField] private Transform EndPoint;
        [SerializeField] private GameObject EnemyPrefab;

        private Vector3 spawn;

        //private Dictionary<int, EnemyComponent> enemiesData;
        private Dictionary<int, Transform> enemiesTransforms;
        private HashSet<int> enemiesToRemove;
        
        private List<float3> enemiesPositions;

        //JOB SYSTEM
        private NativeArray<int> nativeEnemiesID;
        private NativeArray<float3> nativeFlowField;
        private NativeArray<float3> nativeEnemiesDirection;
        private NativeArray<float3> nativeEnemiesPosition;
        private NativeMultiHashMap<int, int> nativeEnemiesInGridCell;
        
        private TransformAccessArray transformAccessArray;

        private JobHandle directionJobHandle;
        private JobHandle moveEnemiesJobHandle;

        private bool EnemiesMoved;

        private void DisposeAll()
        {
            if (nativeEnemiesID.IsCreated) nativeEnemiesID.Dispose();
            if (nativeFlowField.IsCreated) nativeFlowField.Dispose();
            if (nativeEnemiesDirection.IsCreated) nativeEnemiesDirection.Dispose();
            if (nativeEnemiesPosition.IsCreated) nativeEnemiesPosition.Dispose();
            if (nativeEnemiesInGridCell.IsCreated) nativeEnemiesInGridCell.Dispose();
        }

        private void Awake()
        {
            PathfindingGrid ??= FindObjectOfType<PathfindingGrid>();

            //enemiesData = new Dictionary<int, EnemyComponent>(16);
            enemiesTransforms = new Dictionary<int, Transform>(16);
            
            enemiesToRemove = new HashSet<int>(16);
            enemiesPositions = new List<float3>(16);
            
            transformAccessArray = new TransformAccessArray(16);
        }

        private void OnDestroy()
        {
            DisposeAll();
            if(transformAccessArray.isCreated) transformAccessArray.Dispose();
        }

        // Update is called once per frame
        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                CreateWave(1);
            }
            if (enemiesTransforms.Count == 0) return;
            MoveAllEnemies();
        }

        private void LateUpdate()
        {
            if (moveEnemiesJobHandle.IsCompleted && EnemiesMoved)
            {
                moveEnemiesJobHandle.Complete();
                enemiesPositions.Clear();
                enemiesPositions.AddRange(nativeEnemiesPosition.ToArray());
                /*
                using var test1 = nativeEnemiesInGridCell.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < test1.Length; i++)
                {
                    int numValue = nativeEnemiesInGridCell.CountValuesForKey(test1[i]);
                    Debug.Log($"Get value count for key:{test1[i]}; Numvalue : {numValue}");
                }
*/
                DisposeAll();

                EnemiesMoved = false;
            }
            
            if (enemiesToRemove.Count == 0) return;
            ClearEnemiesGone();
        }

        private void MoveAllEnemies()
        {
            int numEnemies = enemiesPositions.Count;
            
            //CAREFULL Need Manual Dispose
            nativeEnemiesPosition = enemiesPositions.ToNativeArray(Allocator.TempJob);
            
            nativeFlowField = PathfindingGrid.DirectionsGrid.ToNativeArray().Reinterpret<float3>();
            nativeEnemiesDirection = AllocNtvAry<float3>(numEnemies);
            //Gather Datas
            nativeEnemiesID = enemiesTransforms.GetKeysArray().ToNativeArray();
            nativeEnemiesInGridCell = new NativeMultiHashMap<int, int>(numEnemies, Allocator.TempJob);
            
            JEntityFlowFieldDirection directionsJob = new JEntityFlowFieldDirection
            {
                EntitiesID = nativeEnemiesID,
                GridSize = PathfindingGrid.GridSize,
                EntitiesPosition = nativeEnemiesPosition,
                FlowField = nativeFlowField,
                Directions = nativeEnemiesDirection,
                EnemiesInGridCell = nativeEnemiesInGridCell.AsParallelWriter()
            };
            directionJobHandle = directionsJob.ScheduleParallel(numEnemies, JobWorkerCount - 1,default);

            JMoveEnemies moveJob = new JMoveEnemies
            {
                DeltaTime = Time.deltaTime,
                EntityFlowFieldDirection = nativeEnemiesDirection,
                EntitiesPosition = nativeEnemiesPosition
            };
            
            moveEnemiesJobHandle = moveJob.Schedule(transformAccessArray, directionJobHandle);
            JobHandle.ScheduleBatchedJobs();
            EnemiesMoved = true;
        }

        public void CreateWave(int numToSpawn) //temporary public
        {
            Vector3[] spawnPoints = PathfindingGrid.GetSpawnPointsForEntities(numToSpawn, 2);
            for (int i = 0; i < numToSpawn; i++)
            {
                GameObject go = Instantiate(EnemyPrefab, spawnPoints[i], Quaternion.identity);
                go.name = $"Agent_{i}";
                EnemyComponent enemy = go.GetComponent<EnemyComponent>();

                enemiesTransforms.Add(enemy.UniqueID, enemy.transform);
                transformAccessArray.Add(enemy.transform);
                enemiesPositions.Add(enemy.transform.position);
            }
        }
        
        
        
        //ITS A MESS NEED A SERIOUS CONCEPTION NOW!

        private void ClearEnemiesGone()
        {
            foreach (int enemyIndex in enemiesToRemove)
            {
                Destroy(enemiesTransforms[enemyIndex].gameObject);
                enemiesTransforms.Remove(enemyIndex);
            }
            transformAccessArray.Dispose();
            transformAccessArray = new TransformAccessArray(enemiesTransforms.GetValuesArray());
            enemiesToRemove.Clear();
        }
        
        //==============================================================================================================
        //EXTERNAL NOTIFICATION
        //==============================================================================================================
        public void EnemyKilled(int enemy) => enemiesToRemove.Add(enemy);
    }

    [BurstCompile]
    public struct JEntityFlowFieldDirection : IJobFor
    {
        [ReadOnly] public int2 GridSize;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<int> EntitiesID;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<float3> EntitiesPosition;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<float3> FlowField;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> Directions;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeMultiHashMap<int, int>.ParallelWriter EnemiesInGridCell;
        public void Execute(int index)
        {
            int cellIndex = EntitiesPosition[index].xz.GetIndexFromPosition(GridSize, 1);
            Directions[index] = FlowField[cellIndex];
            EnemiesInGridCell.Add(cellIndex, EntitiesID[index]);
        }
    }
    
    [BurstCompile]
    public struct JMoveEnemies : IJobParallelForTransform
    {
        [ReadOnly] public float DeltaTime;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<float3> EntityFlowFieldDirection;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> EntitiesPosition;
        public void Execute(int index, TransformAccess transform)
        {
            //Rotation
            Quaternion newRotation = Quaternion.LookRotation(EntityFlowFieldDirection[index], Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, DeltaTime * 10);
            
            //Position
            float2 direction = EntityFlowFieldDirection[index].xz;
            Vector3 newPosition = transform.position + new Vector3(direction.x, 0.0f, direction.y) * DeltaTime * 5;
            
            //Apply Changes to GameObject's Transform
            transform.position = newPosition;
            EntitiesPosition[index] = newPosition;
        }
    }
}
