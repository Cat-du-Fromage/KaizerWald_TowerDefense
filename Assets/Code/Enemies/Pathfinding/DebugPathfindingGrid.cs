using System.Collections;
using System.Collections.Generic;
using KWUtils;
using Unity.Mathematics;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using KWUtils.Debug;
#endif

namespace TowerDefense
{
#if UNITY_EDITOR
    public partial class PathfindingGrid : MonoBehaviour
    {
        [SerializeField] private MainFlock MainFlockDebug;
        [SerializeField] private NeighborFlock[] NeighborFlockDebug;
        private Transform MainFlockDebugTsm;
        private Transform[] NeighborFlockDebugTsm;
        
        public bool EnableDebugger;
        int totalChunk => numChunkXY.x * numChunkXY.y;
        
        private Dictionary<int, int[]> grid;//DEBUG ONLY
        private Dictionary<int, byte[]> costGrid; //DEBUG ONLY
        
        private GridData debugGridData;
        
        private byte[] CostField; 
        
        

        //SPAWNING POINT
        [SerializeField] private GameObject SpawnArea;
        public bool ShowSpawnChunk;
        
        [HideInInspector]
        [SerializeField]private Vector3[] chunksPosition;

        private void OnValidate()
        {
            InitializeFields();
            if (chunksPosition.Length == 0 || chunksPosition.Length != totalChunk)
            {
                chunksPosition = new Vector3[totalChunk];
            }
            InitializeChunkGrid();
            SpawningChunkIndex = Mathf.Clamp(SpawningChunkIndex, 0, totalChunk - 1);
            MoveSpawnArea();
            ShowHideSpawnArea(ShowSpawnChunk);
        }

        

        /// <summary>
        /// Move the spawning area (Green Square)
        /// </summary>
        private void MoveSpawnArea() => SpawnArea.transform.position = GetPosition(SpawningChunkIndex.GetXY2(numChunkXY.x));
        private void ShowHideSpawnArea(bool state) => SpawnArea.GetComponent<MeshRenderer>().enabled = state;



        [ExecuteInEditMode]
        private void OnDrawGizmos()
        {
            if (!EnableDebugger) return;
            debugGridData = new GridData(ChunkSize, gridSize);
            if (chunksPosition.Length == 0)
            {
                chunksPosition = new Vector3[totalChunk];
                InitializeChunkGrid();
            }
            
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            //DisplayChunkGrid(style);

            //CostDebug(style, true);
            //BestCostDebug(style);
            FlowFieldDebug(true, true);
            //DebugBoidsBehaviour();

            //DisplayDestination();
        }

        private void DebugBoidsBehaviour()
        {
            if(MainFlockDebug == null || NeighborFlockDebug.Length == 0) return;
           
            //Vector3 flowfieldDirectionMain = 
            InitFlockDebug();
            
            int cellIndex = new float3(MainFlockDebugTsm.position).xz.GetIndexFromPosition(GridSize, 1);

            //Vector3 directionFlow = GetFlowDirection();
            

            //Display where the main flock IS
            GetCellMainFlockIsIn();
            
            //GetNeighborToMainDirection(directionFlow);
            

            void GetNeighborToMainDirection(Vector3 flowdir)
            {
                Vector3 AddAllNeighborDirection = Vector3.zero;
                
                
                
                for (int i = 0; i < NeighborFlockDebug.Length; i++)
                {
                    Vector3 debugDirection = Vector3.zero;
                    debugDirection = MainFlockDebugTsm.position - NeighborFlockDebugTsm[i].position;
                    if (CheckIfBehind(NeighborFlockDebugTsm[i])) continue;
                    Gizmos.color = Color.magenta;
                    DrawArrow.ForGizmo(NeighborFlockDebugTsm[i].position, debugDirection * 2);

                    AddAllNeighborDirection += debugDirection.normalized;
                }
                //Without FlowField
                AddAllNeighborDirection /= Mathf.Max(1,NeighborFlockDebug.Length);
                Gizmos.color = Color.blue;
                DrawArrow.ForGizmo(MainFlockDebugTsm.position, AddAllNeighborDirection);

                //With FlowField
                AddAllNeighborDirection += flowdir;
                
                if (AddAllNeighborDirection == Vector3.zero) return;
                Gizmos.color = Color.red;
                DrawArrow.ForGizmo(MainFlockDebugTsm.position,AddAllNeighborDirection * 2.5f);

            }

            //Check if Main Boids behind the others
            bool CheckIfBehind(Transform neighbor)
            {
                //CAREFUL we need vector to be "opposite" so forward + direction(origin:main, to : other) this : <- + -> NOT : -> + <- OR <- + <-
                Vector3 dotDirection = neighbor.position - MainFlockDebugTsm.position;
                return Vector3.Dot(MainFlockDebugTsm.forward, dotDirection) < 0;
            }
            
            //Check for null Value
            void InitFlockDebug()
            {
                if (NeighborFlockDebugTsm == null)
                {
                    NeighborFlockDebugTsm = new Transform[NeighborFlockDebug.Length];
                    for (int i = 0; i < NeighborFlockDebugTsm.Length; i++)
                    {
                        NeighborFlockDebugTsm[i] = NeighborFlockDebug[i].transform;
                    }
                }
                if (MainFlockDebugTsm == null)
                {
                    MainFlockDebugTsm = MainFlockDebug.transform;
                }
            }

            void GetCellMainFlockIsIn()
            {
                //DrawCell where the Main Flock IS
                Gizmos.color = Color.green;
                int2 coord = cellIndex.GetXY2(gridSize.x);
                Gizmos.DrawWireCube(new Vector3(coord.x+0.5f,0,coord.y+0.5f), Vector3.one);
            }
/*
            //GetFlowField Direction + display yellow Arrow
            Vector3 GetFlowDirection()
            {
                if (directionsGrid != null && directionsGrid.Length > 0)
                {
                    Gizmos.color = Color.yellow;
                    DrawArrow.ForGizmo(MainFlockDebugTsm.position, directionsGrid[cellIndex]);
                    return directionsGrid[cellIndex];
                }
                return Vector3.zero;
            }
            */
        }
        
        
        private void DisplayChunkGrid(GUIStyle style)
        {
            Vector3 cubeBounds = new Vector3(ChunkSize, 0, ChunkSize);
            for (int i = 0; i < totalChunk; i++)
            {
                Handles.DrawWireCube(chunksPosition[i], cubeBounds);
                Handles.Label(chunksPosition[i] + new Vector3(-(ChunkSize/4f),0,ChunkSize), i.GetXY2(numChunkXY.x).ToString(), style);
            }
        }

        private void DisplayDestination()
        {
            if (destinationGridCell == -1) return;
            int2 xy = destinationGridCell.GetXY2(gridSize.x);
            Vector3 destPos = new Vector3(xy.x + 0.5f, 0, xy.y + 0.5f);
            Handles.DrawWireCube(destPos, Vector3.one);
        }
        
        private void InitializeChunkGrid()
        {
            int halfChunk = ChunkSize/2;
            
            for (int i = 0; i < totalChunk; i++)
            {
                int2 chunkCoord = i.GetXY2(numChunkXY.x);
                
                float chunkX = (chunkCoord.x * ChunkSize) + halfChunk;
                float chunkY = (chunkCoord.y * ChunkSize) + halfChunk;

                chunksPosition[i] = new Vector3(chunkX, 0, chunkY);
            }
        }

        private void BestCostDebug(GUIStyle style, bool walkableOnly)
        {
            if (grid == null) return;
            for (int i = 0; i < 2; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < grid[chunkIndex].Length; j++)
                {
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    Handles.Label(cellPos, grid[chunkIndex][j].ToString(), style);
                }
            }
        }
        
        private void CostDebug(GUIStyle style, bool walkableOnly)
        {
            if (costGrid == null) return;
            for (int i = 4; i < 9; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < costGrid[chunkIndex].Length; j++)
                {
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    Handles.Label(cellPos, costGrid[chunkIndex][j].ToString(), style);
                }
            }
        }

        private void FlowFieldDebug(bool walkableOnly, bool drawCube = false)
        {
            /*
            if (directionChunkGrid == null) return;
            
            Vector3 cubeBounds = new Vector3(1f,0.2f,1f);
            Vector3 ArrowOffset = new Vector3(0, -0.1f, 0);
            for (int i = 0; i < 4; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < directionChunkGrid[chunkIndex].Length; j++)
                {
                    Gizmos.color = Color.yellow;
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    DrawArrow.ForGizmo(cellPos-ArrowOffset, directionChunkGrid[chunkIndex][j]/2f);
                    if (drawCube)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(cellPos, cubeBounds);
                    }
                }
            }
            */
            if (directionGrid == null) return;
            
            Vector3 cubeBounds = new Vector3(1f,0.2f,1f);
            Vector3 ArrowOffset = new Vector3(0, -0.1f, 0);
            for (int i = 0; i < 4; i++)
            {
                int chunkIndex = walkableOnly ? walkableChunk[i] : i;
                for (int j = 0; j < directionGrid[chunkIndex].Length; j++)
                {
                    Gizmos.color = Color.yellow;
                    int realIndex = chunkIndex.GetGridCellIndexFromChunkCellIndex(debugGridData, j);
                        
                    int2 coord = realIndex.GetXY2(gridSize.x);
                    Vector3 cellPos = new Vector3(coord.x + 0.5f, 0, coord.y + 0.5f);
                    DrawArrow.ForGizmo(cellPos-ArrowOffset, directionGrid[chunkIndex][j]/2f);
                    if (drawCube)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(cellPos, cubeBounds);
                    }
                }
            }
        }
    }
#endif
}
