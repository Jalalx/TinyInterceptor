using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace TinyInterceptor
{
    internal class InternalProxy<T> : RealProxy where T : class, new()
    {
        private readonly T _instance = null;

        private InternalProxy(T instance) : base(typeof(T))
        {
            _instance = instance;
        }

        public static T Create(T instance)
        {
            var proxy = new InternalProxy<T>(instance);
            var obj = proxy.GetTransparentProxy();

            return obj as T;
        }

        public override IMessage Invoke(IMessage message)
        {
            var methodCallMessage = message as IMethodCallMessage;
            var method = methodCallMessage.MethodBase;

            var interceptAttribute = method.GetCustomAttributes(false/* do not search in parent members. */)
                .SingleOrDefault(a => a.GetType() == typeof(InterceptAttribute)) as InterceptAttribute;

            object result = null;
            if (interceptAttribute != null)
            {
                // create an instance of intercept support class which introduced to InterceptAttribute.
                var interceptorSupport = Activator.CreateInstance(interceptAttribute.TargetType) as IInterceptSupport;

                // call IInterceptSupport.BeforeExecute
                interceptorSupport.BeforeExecute(_instance);

                try
                {
                    // call method normally.
                    result = method.Invoke(_instance, methodCallMessage.InArgs);

                    // call IInterceptSupport.AfterExecute
                    interceptorSupport.AfterExecute(_instance);
                }
                catch (Exception ex)
                {
                    // call IInterceptSupport.OnError
                    interceptorSupport.OnError(_instance, ex);
                }
            }
            else
            {
                // call method normally.
                result = method.Invoke(_instance, methodCallMessage.InArgs);
            }

            // return result of interception
            return new ReturnMessage(result, null, 0, methodCallMessage.LogicalCallContext, methodCallMessage);
        }
    }
}
