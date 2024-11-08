using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public static class EventListenerManager
{
    private static readonly Dictionary<(Type, EventName), Delegate> eventDictionary = new();

    public static void Register((Type, EventName) eventKey, Delegate callback)
    {
        if (eventDictionary.TryGetValue(eventKey, out var existingDelegate))
        {
            if (existingDelegate.GetType() == callback.GetType())
                eventDictionary[eventKey] = Delegate.Combine(existingDelegate, callback);
            else
                Debug.LogError($"Event '{eventKey.Item2}' is already registered with a different delegate type.");
        }
        else
        {
            eventDictionary[eventKey] = callback;
        }
    }

    public static T Trigger<T>((Type, EventName) eventKey, params object[] parameters)
    {
        return (T)Trigger(eventKey, parameters);
    }

    public static object Trigger((Type, EventName) eventKey, params object[] parameters)
    {
        if (eventDictionary.TryGetValue(eventKey, out var callback))
        {
            try
            {
                return callback.DynamicInvoke(parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking event '{eventKey.Item2}': {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Event '{eventKey.Item2}' not found in the event dictionary.");
        }

        return null;
    }

    public static void RegisterListener(object target)
    {
        var methods = target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<EventListenerAttribute>();
            if (attribute != null)
            {
                var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var returnType = method.ReturnType;

                Type[] delegateSignature = returnType == typeof(void)
                    ? parameters
                    : parameters.Concat(new[] { returnType }).ToArray();

                var delegateType = Expression.GetDelegateType(delegateSignature);

                try
                {
                    var listener = method.CreateDelegate(delegateType, target);
                    Register((attribute.SourceType, attribute.EventName), listener);
                }
                catch (ArgumentException ex)
                {
                    Debug.LogError(
                        $"Could not create delegate for method '{method.Name}' with signature '{method}'. Ensure it matches the expected signature. Exception: {ex.Message}");
                }
            }
        }
    }
}
