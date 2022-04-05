using System;
using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

using static Unity.Jobs.LowLevel.Unsafe.JobsUtility;
using static KWUtils.NativeCollectionExt;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;


namespace TowerDefense
{
    public class EnemyManager : MonoBehaviour
    {
        //References to Grids
        [SerializeField] private FlowFieldGrid flowFieldGrid;

        //Prefabs
        [SerializeField] private GameObject EnemyPrefab;
        
        private Dictionary<int, Transform> enemiesTransforms;
        private HashSet<int> enemiesToRemove;
        
        //JOB SYSTEM
        private NativeArray<float3> nativeFlowField;
        private NativeArray<float3> nativeEnemiesDirection;
        private NativeArray<float3> nativeEnemiesPosition;

        private TransformAccessArray transformAccessArray;

        private JobHandle directionJobHandle;
        private JobHandle moveEnemiesJobHandle;

        //Round Management
        private bool EnemiesMoved;
        private int spawnChunkIndex;
        private Vector3[] spawnPoints;

        private void DisposeAll()
        {
            if (nativeFlowField.IsCreated)         nativeFlowField.Dispose();
            if (nativeEnemiesDirection.IsCreated)  nativeEnemiesDirection.Dispose();
            if (nativeEnemiesPosition.IsCreated)   nativeEnemiesPosition.Dispose();
        }

        private void Awake()
        {
            flowFieldGrid = flowFieldGrid.FindCheckNullComponent();
            enemiesTransforms = new Dictionary<int, Transform>(16);
            enemiesToRemove = new HashSet<int>(16);

            transformAccessArray = new TransformAccessArray(16);
        }

        private void Start()
        {
            spawnChunkIndex = flowFieldGrid.GetChunkSpawn();
            spawnPoints = flowFieldGrid.Grid.GetChunkCellsCenter(spawnChunkIndex);
        }

        private void OnDestroy()
        {
            DisposeAll();
            if(transformAccessArray.isCreated) transformAccessArray.Dispose();
        }
        
        private void Update()
        {
            if (flowFieldGrid.startCellIndex is -1 or 0) return;
            if (flowFieldGrid.Grid == null) return;
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                CreateWave(64);
            }
            if (enemiesTransforms.Count == 0) return;
            MoveAllEnemies();
        }

        private void LateUpdate()
        {
            if (moveEnemiesJobHandle.IsCompleted && EnemiesMoved)
            {
                moveEnemiesJobHandle.Complete();
                DisposeAll();
                EnemiesMoved = false;
            }
            if (enemiesToRemove.Count == 0) return;
            ClearEnemiesGone();
        }

        public void MoveAllEnemies()
        {
            int numEnemies = enemiesTransforms.Count;
            
            nativeEnemiesPosition = AllocNtvAry<float3>(numEnemies);
            nativeFlowField = flowFieldGrid.Grid.GridArray.ToNativeArray().Reinterpret<float3>();
            nativeEnemiesDirection = AllocNtvAry<float3>(numEnemies);

            JobHandle positionsJobHandle = new JGetPositions(nativeEnemiesPosition)
                .ScheduleReadOnly(transformAccessArray, JobWorkerCount - 1);
            
            JEntityFlowFieldDirection directionsJob = new JEntityFlowFieldDirection
            {
                GridSize = flowFieldGrid.Grid.GridData.NumCellXY,
                EntitiesPosition = nativeEnemiesPosition,
                FlowField = nativeFlowField,
                EntityFlowFieldDirection = nativeEnemiesDirection,
            };
            directionJobHandle = directionsJob.ScheduleParallel(numEnemies, JobWorkerCount - 1,positionsJobHandle);
            
            JMoveEnemies moveJob = new (Time.deltaTime, nativeEnemiesDirection, nativeEnemiesPosition);
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
                enemy.name = $"Agent_{i}";

                RegisterEnemy(enemy);
                transformAccessArray.Add(enemy.transform);
            }
        }
        
        private void ClearEnemiesGone()
        {
            foreach (int enemyIndex in enemiesToRemove)
            {
                Destroy(enemiesTransforms[enemyIndex].gameObject);
                UnregisterEnemy(enemyIndex);
            }
            transformAccessArray.SetTransforms(enemiesTransforms.GetValuesArray());
            enemiesToRemove.Clear();
        }

        private void RegisterEnemy(EnemyComponent enemy) => enemiesTransforms.Add(enemy.UniqueID, enemy.transform);
        private void UnregisterEnemy(int uniqueId) => enemiesTransforms.Remove(uniqueId);

        //==============================================================================================================
        //EXTERNAL NOTIFICATION
        //==============================================================================================================
        public void EnemyKilled(int enemy) => enemiesToRemove.Add(enemy);
    }
    
    [BurstCompile(CompileSynchronously = true)]
    public struct JGetPositions : IJobParallelForTransform
    {
        [NativeDisableParallelForRestriction]
        [WriteOnly]private NativeArray<float3> EntitiesPosition;

        public JGetPositions(NativeArray<float3> entitiesPosition)
        {
            EntitiesPosition = entitiesPosition;
        }
        
        public void Execute(int index, TransformAccess transform)
        {
            EntitiesPosition[index] = transform.position;
        }
    }
    

    [BurstCompile(CompileSynchronously = true)]
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

    [BurstCompile(CompileSynchronously = true)]
    public struct JMoveEnemies : IJobParallelForTransform
    {
        [ReadOnly] private readonly float DeltaTime;

        [NativeDisableParallelForRestriction]
        [ReadOnly] private NativeArray<float3> EntityFlowFieldDirection;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly] private NativeArray<float3> EntitiesPosition;

        public JMoveEnemies(float delta, NativeArray<float3> direction, NativeArray<float3> positions)
        {
            DeltaTime = delta;
            EntityFlowFieldDirection = direction;
            EntitiesPosition = positions;
        }
        
        public void Execute(int index, TransformAccess transform)
        {
            //Rotation
            Quaternion newRotation = Quaternion.LookRotation(EntityFlowFieldDirection[index], Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, DeltaTime * 10);
            
            //Position
            float2 direction = EntityFlowFieldDirection[index].xz;
            Vector3 newPosition = transform.position + (DeltaTime * 5) * new Vector3(direction.x, 0.0f, direction.y);

            //Apply Changes to GameObject's Transform
            transform.position = newPosition;
            EntitiesPosition[index] = newPosition;
        }
    }
}
