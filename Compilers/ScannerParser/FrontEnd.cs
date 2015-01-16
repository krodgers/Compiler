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

    public class Parser {
        private int scannerSym; // current token on input

        public enum Token{
        ERROR = 0, TIMES = 1, DIV= 2,
            PLUS = 11, MINUS = 12, EQL = 20, 
            NEQ, LSS, GEQ, LEQ, GTR,
            PERIOD = 30, COMMA, OPENBRACKET, CLOSEBRACKET, CLOSEPAREN,
            BECOMES = 40, THEN, DO, 
            OPENPAREN = 50, NUMBER = 60, IDENT, 
            SEMI = 70, END = 80, OD, FI,
            ELSE = 90, LET = 100, CALL, IF, WHILE, RETURN,
            VAR = 110, ARR, FUNC, PROC,
            BEGIN = 150, MAIN = 200, EOF = 255
};
        private void Next();
    }
}
