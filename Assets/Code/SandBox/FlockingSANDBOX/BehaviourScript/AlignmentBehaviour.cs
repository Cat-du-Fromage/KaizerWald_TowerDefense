using System.Collections;
using System.Collections.Generic;
using KWUtils;
using UnityEngine;

namespace TowerDefense
{
    [CreateAssetMenu(menuName = "Flock/Behaviour/Alignment")]
    public class AlignmentBehaviour : FlockBehaviour
    {
        public override Vector3 CalculateMove(FlockAgent agent, List<Transform> neighborAgents, Flock flockManager)
        {
            //if nothing to adjust with
            if(neighborAgents.Count == 0) return Vector3.zero;
            
            Vector3 alignmentMove = Vector3.zero;
            //Get an average of all nearby objects position
            for (int i = 0; i < neighborAgents.Count; i++)
            {
                alignmentMove += neighborAgents[i].transform.forward;
            }
            alignmentMove /= neighborAgents.Count;

            return alignmentMove;
        }
    }
}
