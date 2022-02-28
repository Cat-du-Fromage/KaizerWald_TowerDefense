using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
namespace TowerDefense
{
    public class NeighborFlock : MonoBehaviour
    {
        [SerializeField] private PathfindingGrid grid;

        private void Awake()
        {
            grid ??= FindObjectOfType<PathfindingGrid>();
        }
    }
}
#endif