using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

    public class Symbol {

        public Token type; // what this symbol is, VAR, FUNC, PROC, ARR ...
        public int currLineNumber {  set; get; } // the last line number this variable seen on
        public int identID { private set; get; }
        private int[] arrDims;// for if it's an array

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
        // Returns true if this symbol is a global symbol
        // i.e. is declared right under main
        public bool IsGlobal() {
            return validScope.Contains(1);
        }
        
        // If it's an array, returns the dimensions
        // returns an empty array otherwise
        public int[] GetArrayDimensions() {
            if (type == Token.ARR) {
                return arrDims;
            } else {
                return new int[0];
            }

        }



        // Utilities
        // Add a valid scope to this symbol
        public void AddScope(int scope) {
            validScope.Add(scope);
        }


    }
}
