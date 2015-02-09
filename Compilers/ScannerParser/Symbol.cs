using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

    class Symbol {

        public Token type; // what this symbol is, VAR, FUNC, PROC, ARR ...
        public int currLineNumber {  set; get; } // the last line number this variable seen on
        public int identID { private set; get; }
        public int[] arrDims { private set; get; } // for if it's an array

        private List<int> validScope;


        // Constructor 
        public Symbol(Token whatAmI, int ID, int lineNum, int scope) {
            type = whatAmI;
            currLineNumber = lineNum;
            identID = ID;
            arrDims = null;
            validScope = new List<int>();
            validScope.Add(scope);

        }

        public Symbol(Token whatAmI, int ID, int lineNum, int[] arrayDimensions, int scope) {
            type = whatAmI;
            currLineNumber = lineNum;
            identID = ID;
            arrDims = arrayDimensions;
            validScope = new List<int>();
            validScope.Add(scope);
        }

        // Accessors
        // Returns true if this identifier is valid in scope
        public bool IsInScope(int scope) {
            // 1 is global scope
            return validScope.Contains(scope) || validScope.Contains(1);
        }

        public int GetMyScope() {
            return validScope.Last<int>();
        }



        // Utilities
        // Add a valid scope to this symbol
        public void AddScope(int scope) {
            validScope.Add(scope);
        }


    }
}
