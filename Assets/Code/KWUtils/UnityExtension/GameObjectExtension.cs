using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public static class GameObjectExtension
    {
        public static I GetInterfaceComponent<I>(this GameObject gameObject) 
        where I : class
        {
            return gameObject.GetComponent(typeof(I)) as I;
        }
        
        public static List<I> FindObjectsOfInterface<I>() 
        where I : class
        {
            MonoBehaviour[] monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
            List<I> list = new List<I>();
 
            foreach(MonoBehaviour behaviour in monoBehaviours)
            {
                I component = behaviour.GetComponent(typeof(I)) as I;
                if (component is null) continue;
                list.Add(component);
            }
            return list;
        }
    }
}
