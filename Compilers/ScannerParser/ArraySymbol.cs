using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class ArraySymbol: MemoryBasedSymbol {
        private int[] arrDims; // the dimensions of the array
        private int arrSize;
        //public ArraySymbol(Token whatAmI, int ID, int lineNum, int[] arrayDimensions, int scope)
        //    : base(whatAmI, ID, lineNum, scope, -1)
        //{
        //    arrDims = arrayDimensions;
        //    arrSize = 1;
        //    foreach (int d in arrayDimensions)
        //        arrSize *= d;
        //}

        // for stack arrays
        public ArraySymbol(Token whatAmI, int ID, int lineNum, int[] arrayDimensions, int scope, int offset)
            : base(whatAmI, ID, lineNum, scope, offset)
        {
            arrDims = arrayDimensions;
            arrSize = 1;
            foreach (int d in arrayDimensions)
                arrSize *= d;
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
        public int GetArraySize() {
            return arrSize;
        }

    }
}
