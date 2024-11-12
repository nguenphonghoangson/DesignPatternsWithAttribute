using System.Collections.Generic;
using System.Linq;

public class NotificationTree
{
    private readonly Dictionary<string, NotificationConfig> _nodes = new();
    private readonly Dictionary<string, HashSet<string>> _childrenMap = new();
    private readonly Dictionary<string, string> _parentMap = new();
    private readonly HashSet<string> _activeNodes = new();

    public void AddNode(NotificationConfig config) 
    {                                                                                                                                                          
        _nodes[config.Id] = config;
        
        if (!string.IsNullOrEmpty(config.ParentId))
        {
            _parentMap[config.Id] = config.ParentId;
            if (!_childrenMap.ContainsKey(config.ParentId))
            {
                _childrenMap[config.ParentId] = new HashSet<string>();
            }
            _childrenMap[config.ParentId].Add(config.Id);
        }
    }

    public void RemoveNode(string nodeId)
    {
        if (_nodes.ContainsKey(nodeId))
        {
            if (_parentMap.ContainsKey(nodeId))
            {
                var parentId = _parentMap[nodeId];
                _childrenMap[parentId].Remove(nodeId);
                _parentMap.Remove(nodeId);
            }

            if (_childrenMap.ContainsKey(nodeId))
            {
                foreach (var childId in _childrenMap[nodeId])
                {
                    _parentMap.Remove(childId);
                }
                _childrenMap.Remove(nodeId);
            }

            _nodes.Remove(nodeId);
            _activeNodes.Remove(nodeId);
        }
    }

    public void ActivateNode(string nodeId)
    {
        if (!_nodes.ContainsKey(nodeId)) return;

        _activeNodes.Add(nodeId);
        NotificationEvents.RaiseStateChanged(nodeId, true);

        // Activate parent chain
        var currentId = nodeId;
        while (_parentMap.TryGetValue(currentId, out var parentId))
        {
            _activeNodes.Add(parentId);
            NotificationEvents.RaiseStateChanged(parentId, true);
            currentId = parentId;
        }
    }

    public void DeactivateNode(string nodeId)
    {
        if (!_nodes.ContainsKey(nodeId)) return;

        _activeNodes.Remove(nodeId);
        NotificationEvents.RaiseStateChanged(nodeId, false);
        var currentId = nodeId;
        while (_parentMap.TryGetValue(currentId, out var parentId))
        {
            if (ShouldDeactivateParent(parentId))
            {
                _activeNodes.Remove(parentId);
                NotificationEvents.RaiseStateChanged(parentId, false);
            }
            else
            {
                break;
            }
            currentId = parentId;
        }
    }

    private bool ShouldDeactivateParent(string parentId)
    {
        if (!_childrenMap.ContainsKey(parentId)) return true;
        return !_childrenMap[parentId].Any(childId => _activeNodes.Contains(childId));
    }
}