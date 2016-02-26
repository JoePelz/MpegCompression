using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tester {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello, world!");
            float test = 1.6f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = 1.4f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = -0.4f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = -0.6f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = -1.4f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = -1.6f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = -255f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = -256f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);
            test = -257f;
            Console.WriteLine("{0:0.00} as a byte: " + (byte)(test), test);

            Console.Read();
        }
    }
}
