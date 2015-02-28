using System;
using System.Collections.Generic;
using System.IO;

namespace ScannerParser {
    // This class contains methods to write out insstructions in SSA form
    class SSAWriter {
        public static StreamWriter sw;
        //////////////////////////////////////////////
        // Basic Instructions
        //////////////////////////////////////////////
        public static Result PutArithmeticRegInstruction(string opCode, Result a, Result b, int lineNumber) {

            PutInstruction(opCode, a.GetValue(), b.GetValue(), lineNumber);
            Result res = new Result(Kind.REG, String.Format("({0})", lineNumber));
            
            return res;
        }
        public static Result PutArithmeticImmInstruction(string opCode, Result a, Result b, int lineNumber) {

            PutInstruction(opCode, a.GetValue(), b.GetValue(), lineNumber);
            Result res = new Result(Kind.REG, String.Format("({0})", lineNumber));

            return res;
        }

        // This function, along with the other puts output assembly in the following format:
        // Line#: opcode (argReg1) (argReg2)
        public static void PutInstruction(string opString, string a, string b, int lineNumber) {
            sw.WriteLine("{0}: {1} {2} {3}", lineNumber, opString, a, b);
            Console.WriteLine("{0}: {1} {2} {3}", lineNumber, opString, a, b);
        }

        public static void  PutInstruction(string op, string a, double b, int lineNumber) {
            sw.WriteLine("{0}: {1} {2} {3}", lineNumber, op, b ,a);
            Console.WriteLine("{0}: {1} {2} {3}", lineNumber, op, b ,a);
        }

        //////////////////////////////////////////////
        // Function Instructions
        //////////////////////////////////////////////

        public static void FunctionEntry(Result functionName, int lineNumber) {
            sw.WriteLine("{0}: bra {1}", lineNumber, functionName.GetValue().ToUpper());
            Console.WriteLine("{0}: bra {1}", lineNumber, functionName.GetValue().ToUpper());

        }
        // puts argument on stack
        // lineNumber: the current line of assembly
        public static void StoreFunctionArgument(Result argument, int lineNumber) {
            
            sw.WriteLine("{0}: sub #4 $SP", lineNumber);
            Console.WriteLine("{0}: sub #4 $SP", lineNumber);
            StoreVariable(argument, new Result(Kind.CONST, ConstantType.ADDR, lineNumber + 1), lineNumber);
        }
        // gets argument from stack
        // Need to increment Assembly line by 2
        public static Result LoadFunctionArgument(int argumentOffset, Result arg, int lineNumber) {
            sw.WriteLine("{0}: sub #{1} $FP", lineNumber, argumentOffset);
            Console.WriteLine("{0}: sub #{1} $FP", lineNumber, argumentOffset);
            sw.WriteLine("{0}: move ({1}) {2}", lineNumber + 1, lineNumber, arg.GetValue());
            Console.WriteLine("{0}: move ({1}) {2}", lineNumber + 1, lineNumber, arg.GetValue());

            return new Result(Kind.REG, String.Format("({0})", lineNumber + 1));
        }
        public static void ReturnFromFunction(Result returnValue, int lineNumber) {
            sw.WriteLine("{1}: ret {0}", returnValue.GetValue(), lineNumber);
            Console.WriteLine("{1}: ret {0}", returnValue.GetValue(), lineNumber);

        }
        public static void ReturnFromProcedure(int lineNumber) {
            sw.WriteLine("{0}: ret", lineNumber);
            Console.WriteLine("{0}: ret", lineNumber);

        }
        // branches to ret add
        public static void LeaveFunction(int lineNumber) {
            sw.WriteLine("{0}: bra $RA", lineNumber);
            Console.WriteLine("{0}: bra $RA", lineNumber);
        }


        //////////////////////////////////////////////
        // Memory Instructions
        //////////////////////////////////////////////
        // Loads Variable from Memory
        public static Result LoadVariable(Result thingToLoad, int lineNumber) {
            sw.WriteLine("{1}: load {0}", thingToLoad.GetValue(), lineNumber);
            Console.WriteLine("{1}: load {0}", thingToLoad.GetValue(), lineNumber);
            Result res;
            if (thingToLoad.type == Kind.REG) {
                res = new Result(Kind.REG, thingToLoad.GetValue());

            } else {
                res = new Result(Kind.REG, String.Format("({0})", lineNumber));
            }
            return res;
        }
        // Stores Variable to Memory
        public static void StoreVariable(Result thingToStore, Result whereToStore, int lineNumber) {
            if (whereToStore.type == Kind.CONST && whereToStore.constantType == ConstantType.ADDR) {
                sw.WriteLine("{2}: store R{1} {0}", thingToStore.GetValue(), whereToStore.GetValue(), lineNumber);
                Console.WriteLine("{2}: store R{1} {0}", thingToStore.GetValue(), whereToStore.GetValue(), lineNumber);
                
            } else {
                Console.WriteLine("WARNING: Attempting to store in something that isn't a register");
            }
        }
// Afer calling this, need to increase AssemblyPC by 2
        public static void LoadArray(Result array, int[] dims, int lineNumber) {
            Result[] inds = array.arrIndices;
            int addr = -1;
            // i*numCols + j
            for (int i = 0; i < inds.Length; i++) {
                if (inds[i].type == Kind.CONST) {
                    addr = 4 * Int32.Parse(inds[i].GetValue());
                }
            }
            if (addr != -1) {
                sw.WriteLine("{0}: adda {1} {2}", lineNumber, array.GetValue(), addr);
                Console.WriteLine("{0}: adda {1} {2}", lineNumber, array.GetValue(), addr);

            }
            sw.WriteLine("{0}: load ({0})", lineNumber + 1, lineNumber);
            Console.WriteLine("{0}: load ({0})", lineNumber + 1, lineNumber);
        }

        //////////////////////////////////////////////
        // Utility Functions
        //////////////////////////////////////////////

    }
}
