using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

    // Class for random yet useful functions
    class Utilities {


// Checks the output in filename against the expected string tokens in expected
// Do not put line numbers in expected
// example : string[] expected  = {"mul", "i", "j"} when you want "1: mul i j"
// filename - the name of the file to use for comparing to expected
        public static void CheckFile(string filename, string[] expected) {
            StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            string result = sr.ReadToEnd();
            string[] delims = { " ", "\n", "\r\n", "1:", "2:", "3:", "4:", "5:", "6:", "7:", "8:", "9:", "0:" };
            string[] splitted = result.Split(delims, StringSplitOptions.RemoveEmptyEntries);


            string[] splittedString = { " ", " ", " " };
            string[] expectedString = { " ", " ", " " };

            for (int s = 0; s < splitted.Length; s++) {
                // Clear out arrays so only one instruction is in them
                if (s % 3 == 0) {
                    splittedString[0] = splittedString[1] = splittedString[2] = " ";
                    expectedString[0] = expectedString[1] = expectedString[2] = " ";
                }

                splittedString[s % 3] = splitted[s];
                expectedString[s % 3] = expected[s];
                if (!splitted[s].Equals(expected[s])) {
                    Console.WriteLine("Failed");

                    Console.WriteLine("Got: {0} {1} {2} Wanted: {3} {4} {5}", splittedString[0], splittedString[1], splittedString[2], expectedString[0], expectedString[1], expectedString[2]);
                    sr.Dispose();
                    return;
                }

            }
            Console.WriteLine("Passed");
        }

    }
}
