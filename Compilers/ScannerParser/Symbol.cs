using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

    public class Symbol {

        public Token type{private set; get;} // what this symbol is, VAR, FUNC, PROC, ARR ...
        public int currLineNumber {  set; get; } // the last line number this variable seen on
        public int identID { private set; get; }
       
        private int[] arrDims;// for if it's an array
        private Dictionary<int, Result> validScopes; // scope is key, value is value of the symbol in the scope
        private int functionOffset; // if the symbol is a function argument, its offset from FP (-4, -8...)

        // Constructor 
        public Symbol(Token whatAmI, int ID, int lineNum, int scope) {
            type = whatAmI;
            currLineNumber = lineNum;
            identID = ID;
            arrDims = null;
            validScopes = new Dictionary<int, Result>();
            validScopes.Add(scope, null);
            functionOffset = 0;

        }

        public Symbol(Token whatAmI, int ID, int lineNum, int[] arrayDimensions, int scope) {
            type = whatAmI;
            currLineNumber = lineNum;
            identID = ID;
            arrDims = arrayDimensions;
            validScopes = new Dictionary<int, Result>();
            validScopes.Add(scope, null);
            functionOffset = 0;
        }

        // Accessors
        // Returns true if this identifier is valid in this particular scope
        // Call IsGlobal() to check for global scopes
        public bool IsInScope(int scope) {
            return validScopes.ContainsKey(scope);
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
            if (IsInScope(whichScope) && validScopes.ContainsKey(whichScope)) {
                return validScopes[whichScope];
                
            } else {
                return null;
            }

        }

// Returns 0 if it doesn't have an offset
        public int GetFunctionArgumentOffset() {
            if (type == Token.VAR) {
                return functionOffset;
            } else {
                Console.WriteLine("WARNING: {0} shouldn't have an argument offset", type);
                return 0;
            }
        }


        // Utilities
        // Add a valid scope to this symbol
        public void AddScope(int scope) {
            validScopes.Add(scope, null);
        }

        // assign a value to the symbol in given scope
        // returns if it successfully added i.e. is in a correct scope
        public bool SetValue(int scope, Result value) {
            if (IsInScope(scope)) {
                validScopes[scope] = value;
                return true;
            } else {
                return false;
            }
        }
        
        public void SetArgumentOffset(int offset) {
            if (offset > 0) {
                Console.WriteLine("WARNING: Setting positive argument offset");
            }
            if (type != Token.VAR) {
                Console.WriteLine("WARNING: Setting an offset for {0}", type);
            }
            functionOffset = offset;
        }

      


    }
}
