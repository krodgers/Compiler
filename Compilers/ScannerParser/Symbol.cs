using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

    public class Symbol {

        public Token type{protected set; get;} // what this symbol is, VAR, FUNC, PROC, ARR ...
        public int currLineNumber {  set; get; } // the last line number this variable seen on
        public int identID { protected set; get; }
        protected Dictionary<int, Result> validScopes; // scope is key, value is value of the symbol in the scope

        // Constructor 
        public Symbol(Token whatAmI, int ID, int lineNum, int scope) {
            type = whatAmI;
            currLineNumber = lineNum;
            identID = ID;
            validScopes = new Dictionary<int, Result>();
            validScopes.Add(scope, null);
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
        
       

        // Returns the last stored value of this symbol in the given scope
        // returns null if the scope isn't valid or symbol hasn't been given a value
        public Result GetCurrentValue(int whichScope) {
            if (IsInScope(whichScope) && validScopes.ContainsKey(whichScope)) {
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
        public bool SetValue(int scope, Result value) {
            if (IsInScope(scope)) {
                validScopes[scope] = value;
                return true;
            } else {
                return false;
            }
        }
        

      


    }
}
