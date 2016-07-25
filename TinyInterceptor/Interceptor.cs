using System;
using System.Collections.Generic;

namespace TinyInterceptor
{
    public class Interceptor
    {
        private readonly List<Type> _registeredTypes = new List<Type>();

        public void Register<T>() where T : class, new()
        {
            _registeredTypes.Add(typeof(T));
        }

        public T Resolve<T>() where T : class, new()
        {
            if (!_registeredTypes.Contains(typeof(T)))
                throw new InvalidOperationException("Type was not registered.");

            return Build<T>();
        }

        private T Build<T>() where T : class, new()
        {
            var instance = Activator.CreateInstance<T>();
            return InternalProxy<T>.Create(instance);
        }
    }
}
