using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public abstract class FlockBehaviour : ScriptableObject
    {
        /// <summary>
        /// Method use to calculate Cohesion, alignment, separation
        /// </summary>
        /// <param name="agent">agent concerned</param>
        /// <param name="neighborAgents">other agent OR obstacles</param>
        /// <param name="flockManager">flock Manager</param>
        /// <returns>Calculate Direction + Velocity/Speed of the move</returns>
        public abstract Vector3 CalculateMove(FlockAgent agent, List<Transform> neighborAgents, Flock flockManager);
    }
}
