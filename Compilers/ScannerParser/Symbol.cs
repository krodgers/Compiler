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

        private Dictionary<int, Result> validScopes; // scope is key, value is value of the symbol in the scope
      

        // Constructor 
        public Symbol(Token whatAmI, int ID, int lineNum, int scope) {
            type = whatAmI;
            currLineNumber = lineNum;
            identID = ID;
            arrDims = null;
            validScopes = new Dictionary<int, Result>();
            validScopes.Add(scope, null);

        }

        public Symbol(Token whatAmI, int ID, int lineNum, int[] arrayDimensions, int scope) {
            type = whatAmI;
            currLineNumber = lineNum;
            identID = ID;
            arrDims = arrayDimensions;
            validScopes = new Dictionary<int, Result>();
            validScopes.Add(scope, null);
        }

        // Accessors
        // Returns true if this identifier is valid in scope
        public bool IsInScope(int scope) {
            // 1 is global scope
            return validScopes.ContainsKey(scope) || validScopes.ContainsKey(1);
        }

        // Returns true if this symbol is a global symbol
        // i.e. is declared right under main
        public bool IsGlobal() {
            return validScopes.ContainsKey(1);
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

        // Returns the last stored value of this symbol in the given scope
        // returns null if the scope isn't valid or symbol hasn't been given a value
        public Result GetCurrentValue(int whichScope) {
            if (IsInScope(whichScope)) {
                return validScopes[whichScope];
                
            } else {
                return null;
            }


        }


        // Utilities
        // Add a valid scope to this symbol
        public void AddScope(int scope) {
            validScopes.Add(scope, null);
        }

        // assign a value to the symbol in given scope
        // returns if it successfully added i.e. is in a correct scope
        public bool AssignValue(int scope, Result value) {
            if (IsInScope(scope)) {
                validScopes[scope] = value;
                return true;
            } else {
                return false;
            }
        }


    }
}
