using System;
using System.Linq;

namespace TinyInterceptor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class InterceptAttribute : Attribute
    {
        public Type TargetType { get; set; }

        public InterceptAttribute(Type targetType)
        {
            if (!targetType.GetInterfaces().Contains(typeof(IInterceptSupport)))
                throw new InvalidOperationException("TargetType must implement IInterceptSupport interface");

            TargetType = targetType;
        }
    }
}
