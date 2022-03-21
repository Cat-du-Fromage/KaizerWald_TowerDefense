//#define EnableBurst
using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using KWUtils.KWGenericGrid;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using quaternion = Unity.Mathematics.quaternion;


namespace TowerDefense
{
    public class EnemyManager : MonoBehaviour
    {
        //References to Grids
        [SerializeField] private FlowfieldGrid FlowfieldGrid;

        //Prefabs
        [SerializeField] private GameObject EnemyPrefab;
        
        private Dictionary<int, Transform> enemiesTransforms;
        private HashSet<int> enemiesToRemove;

        private Dictionary<int, float3> enemiesPosition;
        //JOB SYSTEM
        private NativeArray<float3> nativeFlowField;
        private NativeArray<float3> nativeEnemiesDirection;
        private NativeArray<float3> nativeEnemiesPosition;
        
        //Use later for FlockSystem
        //===========================================================
        private NativeArray<int> nativeEnemiesID;
        //===========================================================
        
        private TransformAccessArray transformAccessArray;

        private JobHandle directionJobHandle;
        private JobHandle moveEnemiesJobHandle;

        private bool EnemiesMoved;
        private Vector3[] spawnPoints;

        private void DisposeAll()
        {
            if (nativeEnemiesID.IsCreated)         nativeEnemiesID.Dispose();
            if (nativeFlowField.IsCreated)         nativeFlowField.Dispose();
            if (nativeEnemiesDirection.IsCreated)  nativeEnemiesDirection.Dispose();
            if (nativeEnemiesPosition.IsCreated)   nativeEnemiesPosition.Dispose();
        }

        private void Awake()
        {
            FlowfieldGrid = FlowfieldGrid.GetCheckNullComponent();
            enemiesTransforms = new Dictionary<int, Transform>(16);
            
            enemiesToRemove = new HashSet<int>(16);

            //enemiesPositions = new List<float3>(16);
            
            transformAccessArray = new TransformAccessArray(16);
            
            //TEST
            enemiesPosition = new Dictionary<int, float3>(16);
        }

        private void Start()
        {
            spawnPoints = FlowfieldGrid.GetSpawnPointsForEntities();
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
                CreateWave(8);
            }
            if (enemiesTransforms.Count == 0) return;
            MoveAllEnemies();
        }

        private void LateUpdate()
        {
            if (moveEnemiesJobHandle.IsCompleted && EnemiesMoved)
            {
                moveEnemiesJobHandle.Complete();
                ResetEnemyPositionCache();
                DisposeAll();

                EnemiesMoved = false;
            }
            
            if (enemiesToRemove.Count == 0) return;
            ClearEnemiesGone();
        }

        private void ResetEnemyPositionCache()
        {
            foreach ((int id, Transform position) in enemiesTransforms)
            {
                enemiesPosition[id] = position.position;
            }
        }

        private void MoveAllEnemies()
        {
            int numEnemies = enemiesPosition.Count;
            
            nativeEnemiesPosition = enemiesPosition.GetValuesArray().ToNativeArray();
            //nativeFlowField = new NativeArray<Vector3>(FlowfieldGrid.Grid.GridArray, Allocator.TempJob).Reinterpret<float3>();
            nativeFlowField = FlowfieldGrid.Grid.GridArray.ToNativeArray().Reinterpret<float3>();
            nativeEnemiesDirection = AllocNtvAry<float3>(numEnemies);
            
            JEntityFlowFieldDirection directionsJob = new JEntityFlowFieldDirection
            {
                GridSize = FlowfieldGrid.Grid.GridData.NumCellXY,
                EntitiesPosition = nativeEnemiesPosition,
                FlowField = nativeFlowField,
                EntityFlowFieldDirection = nativeEnemiesDirection,
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
            int batchToSpawn = min(numToSpawn, spawnPoints.Length);
            for (int i = 0; i < batchToSpawn; i++)
            {
                EnemyComponent enemy = Instantiate(EnemyPrefab, spawnPoints[i], Quaternion.identity).GetComponent<EnemyComponent>();
                enemy.name = ($"Agent_{i}");

                RegisterEnemy(enemy);
                transformAccessArray.Add(enemy.transform);
            }
        }
        
        //ITS A MESS NEED A SERIOUS CONCEPTION NOW!

        private void ClearEnemiesGone()
        {
            foreach (int enemyIndex in enemiesToRemove)
            {
                Destroy(enemiesTransforms[enemyIndex].gameObject);
                UnregisterEnemy(enemyIndex);
            }
            transformAccessArray.Dispose();
            transformAccessArray = new TransformAccessArray(enemiesTransforms.GetValuesArray());
            enemiesToRemove.Clear();
        }

        private void RegisterEnemy(EnemyComponent enemy)
        {
            enemiesTransforms.Add(enemy.UniqueID, enemy.transform);
            enemiesPosition.Add(enemy.UniqueID,enemy.transform.position);
        }

        private void UnregisterEnemy(int uniqueId)
        {
            enemiesTransforms.Remove(uniqueId);
            enemiesPosition.Remove(uniqueId);
        }
        
        //==============================================================================================================
        //EXTERNAL NOTIFICATION
        //==============================================================================================================
        public void EnemyKilled(int enemy) => enemiesToRemove.Add(enemy);

        private void OnDrawGizmos()
        {
            if (spawnPoints.IsNullOrEmpty()) return;
            Gizmos.color = Color.cyan;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Gizmos.DrawWireCube(spawnPoints[i], Vector3.one);
            }
        }
    }
#if EnableBurst
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct JEntityFlowFieldDirection : IJobFor
    {
        [ReadOnly] public int2 GridSize;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<float3> EntitiesPosition;
        
        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<float3> FlowField;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] public NativeArray<float3> EntityFlowFieldDirection;
        public void Execute(int index)
        {
            int cellIndex = EntitiesPosition[index].xz.GetIndexFromPosition(GridSize, 1);
            EntityFlowFieldDirection[index] = FlowField[cellIndex];
        }
    }
#if EnableBurst
    [BurstCompile(CompileSynchronously = true)]
#endif
    public struct JMoveEnemies : IJobParallelForTransform
    {
        [ReadOnly] public float DeltaTime;

        [NativeDisableParallelForRestriction]
        [ReadOnly] public NativeArray<float3> EntityFlowFieldDirection;
        
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> EntitiesPosition;
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
/*
        private float3 GetDirection(int index)
        {
            //Get AllEnemies
            float distanceCheck = 2f * 2f;
            NativeList<int> distances = new NativeList<int>(EntitiesPosition.Length, Allocator.Temp);
            for (int i = 0; i < EntitiesPosition.Length; i++)
            {
                if (i == index) continue;
                if (dot(EntitiesPosition[index].xz, EntitiesPosition[i].xz) < 0) continue;
                if (distancesq(EntitiesPosition[index].xz, EntitiesPosition[i].xz) <= distanceCheck)
                {
                    distances.AddNoResize(i);
                }
            }
            Debug.Log($"num neigh = {distances.Length}");
            float3 directionSeparation = float3.zero;
            if (distances.Length > 0)
            {
                for (int i = 0; i < distances.Length; i++)
                {
                    directionSeparation.xz += EntitiesPosition[index].xz - EntitiesPosition[distances[i]].xz;
                }
            }
            return normalizesafe(directionSeparation);
        }
        */