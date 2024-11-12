using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationSystem : MonoBehaviour
{
    private static NotificationSystem _instance;
    public static NotificationSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("NotificationSystem");
                _instance = go.AddComponent<NotificationSystem>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private readonly NotificationTree _notificationTree = new();
    private readonly Dictionary<string, NotificationNode> _registeredNodes = new();

    public void Register(NotificationNode node)
    {
        if (!_registeredNodes.ContainsKey(node.Id))
        {
            _registeredNodes[node.Id] = node;
            _notificationTree.AddNode(node.Config);
        }
    }

    public void Unregister(string nodeId)
    {
        if (_registeredNodes.ContainsKey(nodeId))
        {
            _registeredNodes.Remove(nodeId);
            _notificationTree.RemoveNode(nodeId);
        }
    }

    public void SetState(string nodeId, bool active)
    {
        if (_registeredNodes.TryGetValue(nodeId, out var node))
        {
            if (active) _notificationTree.ActivateNode(nodeId);
            else _notificationTree.DeactivateNode(nodeId);
        }
    }

    private void OnEnable()
    {
        NotificationEvents.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        NotificationEvents.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(string nodeId, bool active)
    {
        if (_registeredNodes.TryGetValue(nodeId, out var node))
        {
            node.SetVisualState(active);
        }
    }
}