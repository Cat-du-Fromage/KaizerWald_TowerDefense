using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class EndGateComponent : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out EnemyComponent enemyPassing))
            {
                this.Notify(enemyPassing);
            }
        }
    }
}
