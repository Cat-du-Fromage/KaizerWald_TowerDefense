using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense
{
    public class UIBuilding : MonoBehaviour
    {
        [SerializeField] private GameObject[] TurretsBlueprint;
        
        [SerializeField] private Button BasicTurretBuild;
        [SerializeField] private Button ArtilleryTurretBuild;
        [SerializeField] private Button VolleyTurretBuild;
        [SerializeField] private Button HowitzerTurretBuild;
        
        [SerializeField] private BuildManager GridManager;

        private void Awake()
        {
            GridManager = GridManager == null ? FindObjectOfType<BuildManager>() : GridManager;
            BasicTurretBuild = BasicTurretBuild == null ? gameObject.GetComponentInChildrenFrom<TagBasicTurret, Button>() : BasicTurretBuild;
        }

        private void Start()
        {
            DisableAllBlueprint();
            BasicTurretBuild.onClick.AddListener(OnBasicTurretClick);
        }

        private void OnDestroy()
        {
            BasicTurretBuild.onClick.RemoveListener(OnBasicTurretClick);
        }

        private void OnBasicTurretClick() => GridManager.ToggleBlueprint(TurretsBlueprint[0]);

        private void DisableAllBlueprint()
        {
            for (int i = 0; i < TurretsBlueprint.Length; i++)
            {
                TurretsBlueprint[i].SetActive(false);
            }
        }
    }
}
