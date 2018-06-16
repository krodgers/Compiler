using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class MemoryBasedSymbol : Symbol{


        public int stackOffset { private set; get; } // offset from 

        // Constructor 
        public MemoryBasedSymbol(Token whatAmI, int ID, int lineNum, int scope, int offset)
            : base(whatAmI, ID, lineNum, scope) {
                if (offset < 0)
                    stackOffset = -offset;
                else
                    stackOffset = offset;
        }

        // Returns 0 if it doesn't have an offset
        public int GetFunctionArgumentOffset() {
            if (type == Token.VAR) {
                return stackOffset;
            } else {
                Console.WriteLine("WARNING: {0} shouldn't have an argument offset", type);
                return 0;
            }
        }

        public void SetArgumentOffset(int offset) {
            if (offset > 0) {
                Console.WriteLine("WARNING: Setting positive argument offset");
            }
            if (type != Token.VAR && type != Token.ARR) {
                Console.WriteLine("WARNING: Setting an offset for {0}", type);
            }
            stackOffset = offset;
        }


    }
}
