using System;

namespace SexyReact
{
    /// <summary>
    /// Decorate on your properties in your rx objects.  You implement them with auto-properties, but
    /// under the hood they get implemented by efficient calls to Get/Set (no reflection).
    /// 
    /// Decorate on your classes to implement more efficient storage/property-observer strategies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class RxAttribute : Attribute
    {
    }
}