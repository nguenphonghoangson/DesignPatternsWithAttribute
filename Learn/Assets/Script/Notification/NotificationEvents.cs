using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public static class NotificationEvents
{
    public static event Action<string, bool> OnStateChanged;
    
    public static void RaiseStateChanged(string nodeId, bool active)
    {
        OnStateChanged?.Invoke(nodeId, active);
    }
}
