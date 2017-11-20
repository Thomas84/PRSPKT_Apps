using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stepik_org
{
    class Program
    {
        public static void Main()
        {
            var a = new A();
            a.Number = 1;
            Console.ReadKey();
        }
    }

    class A
    {

        //private int number;
        public int Number
        {
            set { Console.WriteLine("Hello, world!"); }
        }

    }
}
