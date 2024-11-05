using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class SingletonManager
{
    private static readonly Dictionary<Type, MonoBehaviour> instances = new Dictionary<Type, MonoBehaviour>();

    public static T GetInstance<T>() where T : MonoBehaviour
    {
        var type = typeof(T);
        
        if (Attribute.IsDefined(type, typeof(SingletonAttribute)))
        {
            if (!instances.ContainsKey(type))
            {
                var singleton = new GameObject($"{type.Name}_Singleton").AddComponent<T>();
                var attribute = type.GetCustomAttribute<SingletonAttribute>();

                if (attribute != null && attribute.Persistent)
                {
                    UnityEngine.Object.DontDestroyOnLoad(singleton.gameObject);
                }

                instances[type] = singleton;
            }
            
            return instances[type] as T;
        }
        
        throw new InvalidOperationException($"Class {type.Name} does not have SingletonAttribute.");
    }
}