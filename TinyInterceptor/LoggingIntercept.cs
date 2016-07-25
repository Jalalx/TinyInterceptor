using System;

namespace TinyInterceptor
{
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
}
