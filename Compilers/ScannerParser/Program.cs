using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class Program {
        static void Main(string[] args) {

            Console.WriteLine("{0}", (char)255);
Console.ReadLine();
            Scanner s = new Scanner("");
            foreach (int i in Enum.GetValues(typeof(Token))) {
                Console.WriteLine("Token {0} # {1}", Enum.GetName(typeof(Token),i), i);
            }
            Console.ReadLine();
        }
    }
}
