using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public static class EventListenerManager
{
    // Từ điển lưu trữ event và delegate
    private static readonly Dictionary<(Type, EventName, string), Delegate> eventDictionary = new();

    // Hàm Register để đăng ký event và delegate
    public static void Register((Type, EventName) eventKey, Delegate callback)
    {
        // Lấy chữ ký của phương thức từ Delegate
        string methodSignature = GetMethodSignature(callback);

        // Tạo một key bao gồm Type, EventName và MethodSignature
        var eventWithMethodKey = (eventKey.Item1, eventKey.Item2, methodSignature);

        if (eventDictionary.TryGetValue(eventWithMethodKey, out var existingDelegate))
        {
            // Nếu đã tồn tại delegate, gộp chúng lại
            if (existingDelegate.GetType() == callback.GetType())
                eventDictionary[eventWithMethodKey] = Delegate.Combine(existingDelegate, callback);
            else
                Debug.LogError($"Event '{eventKey.Item2}' with signature '{methodSignature}' is already registered with a different delegate type.");
        }
        else
        {
            // Nếu chưa có, thêm vào dictionary
            eventDictionary[eventWithMethodKey] = callback;
        }
    }
    // Phương thức Trigger để gọi event kiểu dữ liệu cụ thể
    public static T Trigger<T>((Type, EventName) eventKey, params object[] parameters)
    {
        return (T)Trigger(eventKey, parameters);
    }
    // Phương thức Trigger để gọi event trả về object
    public static object Trigger((Type, EventName) eventKey, params object[] parameters)
    {
        // Tạo chữ ký của phương thức từ tham số
        string methodSignature = string.Join(",", parameters.Select(p => p.GetType().Name).ToArray());

        // Kiểm tra xem event có trong dictionary không
        if (eventDictionary.TryGetValue((eventKey.Item1, eventKey.Item2, methodSignature), out var callback))
        {
            try
            {
                // Gọi delegate và trả về kết quả
                return callback.DynamicInvoke(parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking event '{eventKey.Item2}': {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Event '{eventKey.Item2}' with signature '{methodSignature}' not found.");
        }

        return null;
    }

    // Phương thức hỗ trợ lấy chữ ký của phương thức từ Delegate
    private static string GetMethodSignature(Delegate callback)
    {
        var methodInfo = callback.Method;
        var parameters = methodInfo.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType.Name).ToArray();
        return string.Join(",", parameterTypes);  // Tạo chuỗi chứa các kiểu tham số
    }
    public static void RegisterListener(object target)
    {
        var methods = GetMethodsWithEventListenerAttribute(target);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<EventListenerAttribute>();
            if (attribute != null)
            {
                var delegateType = GetDelegateTypeForMethod(method);
                if (delegateType != null)
                {
                    TryCreateAndRegisterDelegate(target, method, delegateType, attribute);
                }
            }
        }
    }

    private static IEnumerable<MethodInfo> GetMethodsWithEventListenerAttribute(object target)
    {
        return target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.GetCustomAttribute<EventListenerAttribute>() != null);
    }

    private static Type GetDelegateTypeForMethod(MethodInfo method)
    {
        var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
    
        if (method.ReturnType == typeof(void))
        {
            return Expression.GetActionType(parameterTypes.ToArray());
        }
        else
        {
            parameterTypes.Add(method.ReturnType);
            return Expression.GetFuncType(parameterTypes.ToArray());
        }
    }

    private static void TryCreateAndRegisterDelegate(object target, MethodInfo method, Type delegateType, EventListenerAttribute attribute)
    {
        try
        {
            var listener = Delegate.CreateDelegate(delegateType, target, method.Name);
            Register((attribute.SourceType, attribute.EventName), listener);
        }
        catch (ArgumentException ex)
        {
            Debug.LogError($"Failed to create delegate for method '{method.Name}' with signature '{method}'. Exception: {ex.Message}");
        }
    }

}
