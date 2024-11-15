using System;

[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : Attribute
{
    public bool Persistent { get; set; } = true;
}