using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense
{
    public class FlockAgent : MonoBehaviour
    {
        private Transform agentTransform;
        private Collider agentCollider;
        public Collider AgentCollider => agentCollider;

        private void Awake()
        {
            agentTransform = transform;
            agentCollider = GetComponent<Collider>();
        }

        //Turn agent => face the new direction
        public void Move(Vector3 velocity)
        {
            agentTransform.forward = velocity;
            //Debug.DrawRay(agentTransform.position, agentTransform.forward, Color.red, 2f);
            //Debug.Log(velocity * Time.deltaTime);
            //agentTransform.Translate(velocity * Time.deltaTime);
            agentTransform.position += velocity * Time.deltaTime;
        }
    }
}
