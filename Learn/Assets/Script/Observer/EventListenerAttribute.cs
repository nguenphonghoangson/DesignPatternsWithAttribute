using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class EventListenerAttribute : Attribute
{
    public Type SourceType { get; }
    public EventName EventName { get; }

    public EventListenerAttribute(Type sourceType, EventName eventName)
    {
        SourceType = sourceType;
        EventName = eventName;
    }
}