using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[Node]
public class NotificationNode:MonoBehaviour
{
    public NodeType NodeType => nodeType;
    public NodeType ParentType => parentType;
    public Image NotificationImage => notificationImage;
    public string NodeId { get; set; }
    [SerializeField] private NodeType nodeType;
    private string nodeId;
    [SerializeField] private NodeType parentType;
    [SerializeField] private Image notificationImage;

    private void Awake()
    {
        NotificationManager.RegisterNotification(this);
    }
}


