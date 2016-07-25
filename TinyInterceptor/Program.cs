using System;

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
}
