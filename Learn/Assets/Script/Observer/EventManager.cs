using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Delegate = System.Delegate;

[Singleton]
public class EventManager : MonoBehaviour
{
    private static readonly Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

    public static void Register<T>(string eventName, Action<T> callback)
    {
        if (eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            // Check if the existing delegate type matches the new callback type
            if (existingDelegate is Action<T> existingCallback)
            {
                eventDictionary[eventName] = Delegate.Combine(existingCallback, callback);
            }
            else
            {
                Debug.LogError($"Event '{eventName}' is already registered with a different delegate type.");
            }
        }
        else
        {
            eventDictionary[eventName] = callback;
        }
    }

    public static void Unregister<T>(string eventName, System.Action<T> callback)
    {
        if (eventDictionary.TryGetValue(eventName, out var existingDelegate))
        {
            if (existingDelegate is System.Action<T> existingCallback)
            {
                var currentDelegate = Delegate.Remove(existingCallback, callback);

                // Remove the event if no delegates are left
                if (currentDelegate == null)
                {
                    eventDictionary.Remove(eventName);
                }
                else
                {
                    eventDictionary[eventName] = currentDelegate;
                }
            }
            else
            {
                Debug.LogError($"Attempt to unregister delegate of a different type for event '{eventName}'.");
            }
        }
    }

    public void Trigger<T>(string eventName, T parameter)
    {
        if (eventDictionary.TryGetValue(eventName, out var callback))
        {
            if (callback is Action<T> typedCallback)
            {
                typedCallback.Invoke(parameter);
            }
            else
            {
                Debug.LogError($"Event '{eventName}' is registered with a different parameter type. Expected {callback.GetType()}, but got {typeof(Action<T>)}.");
            }
        }
        else
        {
            Debug.LogWarning($"Event '{eventName}' not found in the event dictionary.");
        }
    }
}

public static class EventManagerExtensions
{
    public static void RegisterEvent(this object target)
    {
        var methods = target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<EventAttribute>();
            if (attribute != null)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object))
                {
                    Action<object> listener = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), target, method);
                    EventManager.Register(attribute.EventName, listener);
                }
                else
                {
                    Debug.LogError($"Method {method.Name} does not match the required signature for event listener. It should have a single parameter of type 'object'.");
                }
            }
        }
    }
}
