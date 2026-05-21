namespace StorageApp.Attributes;
using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public class ContextTabAttribute : Attribute
{
    public string TabName { get; }
    
    public ContextTabAttribute(string tabName)
    {
        TabName = tabName;
    }
}