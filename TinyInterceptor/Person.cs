using System;

namespace TinyInterceptor
{
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
            // if you want to throw an exception, Run Without Debugging to see results.
            //throw new InvalidOperationException("sample exception");
        }
    }
}
