using System.Diagnostics;

namespace App.Base.Services.Implementations
{
    public class FooService: IFooService
    {
        public FooService() { 
        }

        public void Do()
        {
            Console.Write("Hello?");
        }
    }
}
