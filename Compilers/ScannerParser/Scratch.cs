using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScannerParser {
    class Scratch {

        public static void Main() {

            double d = 3.14;

            string d1 = d.ToString("F");
            d = 1.321;
            string d2 = d.ToString("F");
            d = 2.678;
            string d3 = d.ToString("F");
            Console.WriteLine("{0} {1} {2}", d1, d2, d3);
            Console.ReadLine();
        }

    }
}


