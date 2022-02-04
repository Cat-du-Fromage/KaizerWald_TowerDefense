using System;
using UnityEngine;

namespace TowerDefense
{
    public class SelectionSubSystem : MonoBehaviour,IInteractionSubSystem
    {
        public IBaseSystem MainSystem { get; set; }
        
    }
}