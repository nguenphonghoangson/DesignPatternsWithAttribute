using System;

[AttributeUsage(AttributeTargets.Method)]
public class ObserverAttribute : Attribute
{
    public string PropertyName { get; }

    public ObserverAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}