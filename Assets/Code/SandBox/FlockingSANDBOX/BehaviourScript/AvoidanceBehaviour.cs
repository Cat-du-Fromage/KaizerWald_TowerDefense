using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;

namespace TowerDefense
{
    [CreateAssetMenu(menuName = "Flock/Behaviour/Avoidance")]
    public class AvoidanceBehaviour : FlockBehaviour
    {
        public override Vector3 CalculateMove(FlockAgent agent, List<Transform> neighborAgents, Flock flockManager)
        {
            //if nothing to adjust with
            if (neighborAgents.Count == 0)
            {
                return Vector3.zero;
            }
            
            Vector3 agentPosition = agent.transform.position;
            Vector3 avoidanceMove = Vector3.zero;

            int numObjectTooClose = 0;
            //Get an average of all nearby objects position
            for (int i = 0; i < neighborAgents.Count; i++)
            {
                if (Vector3.SqrMagnitude(neighborAgents[i].position - agentPosition) <
                    flockManager.SquareAvoidanceRadius)
                {
                    numObjectTooClose++;
                    avoidanceMove += (agentPosition - neighborAgents[i].position);
                }
            }

            if (numObjectTooClose > 0)
            {
                avoidanceMove /= numObjectTooClose;
            }
            
            avoidanceMove -= agentPosition;
            
            return avoidanceMove;
        }
    }
}
