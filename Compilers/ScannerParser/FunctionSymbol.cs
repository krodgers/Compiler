using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

// Function OR Procedure
    class FunctionSymbol : Symbol {

        public int numberOfFormalParamters { set; get; } // number of arguments it takes


          // Constructor 
        public FunctionSymbol(Token whatAmI, int ID, int lineNum, int scope) 
            :base(whatAmI, ID, lineNum, scope) {

        }


    }
}
