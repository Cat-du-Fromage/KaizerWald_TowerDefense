using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class ButtonTurretReference : MonoBehaviour
    {
        [SerializeField] private GameObject TurretPrefab;
        
        public GameObject GetBlueprint() => TurretPrefab;
    }
}
