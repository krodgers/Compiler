using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public class FileReader {
        private StreamReader input;

        /**
        Opens the file for parsing
        */
        public FileReader(String fileName) {
            try {
                input = new StreamReader(fileName);
            }
            catch(Exception e) {
                Error("Failed to open source file.\n" + e.Message);
            }
        }
        /**
        Returns current symbol and advances to the next character on input
         */
        public char GetSym();

        public void Error(String errMsg) {
            input.Close();
            Console.Error.Write(errMsg);

        }
        

    }


    public class Scanner {
        // current character on input
        private char inputSym;
        private List<String> identifiers;
        private FileReader reader;
		public int number; // the last nmber encountered
        public int id; // last identifier encountered
        

        // Returns current symbol and advances to next token
         public int GetSym();
        
        // Opens file and scans first letter
        public Scanner(String fileName) {
            reader = new FileReader(fileName);
            inputSym = reader.GetSym();
            identifiers = new List<String>();
        }
        private void Next() {
            inputSym = reader.GetSym();
        }

        public void Error(String errMsg) {
            reader.Error(errMsg);
        }

        public String Id2String(int id) {
            if (id < identifiers.Count)
                return identifiers[id];

            return String.Empty;

        }
       
    }

}
