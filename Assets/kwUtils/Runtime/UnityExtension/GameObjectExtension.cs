using System.Collections.Generic;
using UnityEngine;

namespace KWUtils
{
    public static class GameObjectExtension
    {
        public static Transform GetTransformWithComponent<T>(this Transform transf) 
        where T : Component
        {
            if (transf == null) transf = Object.FindObjectOfType<T>().transform;
            return transf;
        }
    
        public static T FindCheckNullComponent<T>(this T component) 
        where T : Component
        {
            if (component == null) component = Object.FindObjectOfType<T>();
            return component;
        }
        
        public static T GetCheckNullComponent<T>(this MonoBehaviour script, T component) 
        where T : Component
        {
            if (component == null) component = script.GetComponent<T>();
            return component;
        }
        
        public static T? GetComponentNullable<T>(this GameObject gameObject) 
        where T : Component
        {
            T comp = gameObject.GetComponent<T>();
            return comp == null ? null : comp;
        }

        
        public static I GetInterfaceComponent<I>(this GameObject gameObject) 
        where I : class
        {
            return gameObject.GetComponent(typeof(I)) as I;
        }
        
        public static I GetInterfaceComponent<I>(this Component component) 
        where I : class
        {
            return component.gameObject.GetComponent(typeof(I)) as I;
        }

        public static List<I> FindObjectsOfInterface<I>() 
        where I : class
        {
            MonoBehaviour[] monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
            List<I> list = new List<I>();
            foreach(MonoBehaviour behaviour in monoBehaviours)
            {
                I[] components = behaviour.GetComponents<I>();
                if (components.IsNullOrEmpty()) continue;
                list.AddRange(components);
            }
            return list;
        }

        public static T2 GetComponentInChildrenFrom<T1,T2>(this GameObject gameObject) 
        where T1 : Component
        where T2 : Component
        {
            return gameObject.GetComponentInChildren<T1>().gameObject.GetComponent<T2>();
        }
        
        public static T2 GetComponentWithTag<T1,T2>(this GameObject gameObject) 
        where T1 : Component
        where T2 : Component
        {
            return gameObject.GetComponentInChildren<T1>().gameObject.GetComponent<T2>();
        }
        
        public static T2 GetWithTagComponentInChildren<T1,T2>(this GameObject gameObject) 
        where T1 : Component
        where T2 : Component
        {
            return gameObject.GetComponentInChildren<T1>().gameObject.GetComponentInChildren<T2>();
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject)
        where T : Component
        {
            return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
        }
    }
}
