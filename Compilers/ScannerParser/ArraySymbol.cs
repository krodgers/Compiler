using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class ArraySymbol: Symbol {
        private int[] arrDims; // the dimensions of the array
        public ArraySymbol(Token whatAmI, int ID, int lineNum, int[] arrayDimensions, int scope)
            : base(whatAmI, ID, lineNum, scope)
        {
            arrDims = arrayDimensions;
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

    }
}
