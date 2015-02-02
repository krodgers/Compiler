using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class Program {
        static void Main(string[] args) {
            Parser p = new Parser("test001.txt");
            p.StartFirstPass();

            Console.ReadLine();
        }
    }

}
