using System;

namespace NetworkFramework.Attributes
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method)]

    // ReSharper disable once InconsistentNaming
    public class RPCMethod : Attribute
    {
    }
}