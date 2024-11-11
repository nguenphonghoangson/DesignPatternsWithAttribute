using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
public static class NotificationManager
{
    private static Dictionary<NodeType, List<NotificationNode>> notification =
        new Dictionary<NodeType, List<NotificationNode>>();

    public static void RegisterNotification(NotificationNode target)
    {
        var nodeAttr = target.GetType().GetCustomAttribute<NodeAttribute>();
        
        if (nodeAttr != null)
        {
            var parentNode = target.ParentType;
            if (!notification.ContainsKey(parentNode))
            {
                notification[parentNode] = new List<NotificationNode>();
            }

            notification[parentNode].Add(target);
        }
    }
}
public enum NodeType
{
    
}   