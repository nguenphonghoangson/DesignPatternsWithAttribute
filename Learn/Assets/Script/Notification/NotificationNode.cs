using System;
using UnityEngine;
using UnityEngine.UI;

public class NotificationNode : MonoBehaviour
{
    [SerializeField] private NotificationConfig config;
    [SerializeField] private Image notificationImage;

    public string Id => config.Id;
    public NotificationConfig Config => config;

    private void Start()
    {
        RegisterNode();
        SetNodeState(true);
    }

    private void OnDestroy()
    {
        NotificationSystem.Instance.Unregister(Id);
    }

    private void RegisterNode()
    {
        NotificationSystem.Instance.Register(this);
    }

    public void SetVisualState(bool isActive)
    {
        notificationImage.gameObject.SetActive(isActive);
    }

    [ContextMenu("Deactivate")]
    public void DeactivateNode()
    {
        SetNodeState(false);
    }

    [ContextMenu("Activate")]
    public void ActivateNode()
    {
        SetNodeState(true);
    }

    private void SetNodeState(bool isActive)
    {
        NotificationSystem.Instance.SetState(Id, isActive);
    }
}

[Serializable]
public class NotificationConfig
{
    [SerializeField] private string id;
    [SerializeField] private string parentId;

    public string Id
    {
        get => id;
        set => id = value;
    }

    public string ParentId
    {
        get => parentId;
        set => parentId = value;
    }
}