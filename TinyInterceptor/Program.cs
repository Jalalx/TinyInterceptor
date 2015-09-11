using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace TinyInterceptor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var interceptor = new Interceptor();

            interceptor.Register<Person>();

            var person = interceptor.Resolve<Person>();
            //var person = new Person();

            person.FirstName = "Bill";
            person.LastName = "Gates";

            person.Introduce();


            Console.ReadKey();
        }
    }

    public class Person : MarshalByRefObject
    {
        public Person()
        {
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Intercept(typeof(LoggingIntercept))]
        public void Introduce()
        {
            Console.WriteLine("My name is {0} {1}.", FirstName, LastName);
            // if you want to throw an exception, Run Without Debuggin to see results.
            //throw new InvalidOperationException("wtf?");
        }
    }

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

    public interface IInterceptSupport
    {
        void BeforeExecute(object instance);

        void AfterExecute(object instance);

        void OnError(object instance, Exception ex);
    }

    public class LoggingIntercept : IInterceptSupport
    {
        public void AfterExecute(object instance)
        {
            Console.WriteLine("After member execute");
        }

        public void BeforeExecute(object instance)
        {
            Console.WriteLine("Before member execute");
        }

        public void OnError(object instance, Exception ex)
        {
            Console.WriteLine("Execution faild.", ex.Message);
        }
    }

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
