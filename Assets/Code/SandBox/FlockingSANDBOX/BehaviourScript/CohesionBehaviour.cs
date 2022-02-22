using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;

namespace TowerDefense
{
    [CreateAssetMenu(menuName = "Flock/Behaviour/Cohesion")]
    public class CohesionBehaviour : FlockBehaviour
    {
        public override Vector3 CalculateMove(FlockAgent agent, List<Transform> neighborAgents, Flock flockManager)
        {
            //if nothing to adjust with
            if(neighborAgents.Count == 0) return Vector3.zero;
            
            Vector3 cohesionMove = Vector3.zero;
            //Get an average of all nearby objects position
            for (int i = 0; i < neighborAgents.Count; i++)
            {
                cohesionMove += neighborAgents[i].position;
            }
            cohesionMove /= neighborAgents.Count;
            cohesionMove -= agent.transform.position;
            
            return cohesionMove;
        }
    }
}
