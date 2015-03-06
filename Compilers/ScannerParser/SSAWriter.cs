using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ScannerParser {
    // This class contains methods to write out insstructions in SSA form
    public class SSAWriter {
        public static StreamWriter sw;
        //////////////////////////////////////////////
        // Basic Instructions
        //////////////////////////////////////////////
        public static Result PutArithmeticRegInstruction(string opCode, Result a, Result b, int lineNumber) {
            string Avalue = a.type == Kind.CONST ? "#" + a.GetValue() : a.GetValue();
            string Bvalue = b.type == Kind.CONST ? "#" + b.GetValue() : b.GetValue();

            PutInstruction(opCode, Avalue, Bvalue, lineNumber);
            Result res = new Result(Kind.REG, String.Format("({0})", lineNumber));

            return res;
        }
        public static Result PutArithmeticImmInstruction(string opCode, Result a, Result b, int lineNumber) {
            string Avalue = a.type == Kind.CONST ? "#" + a.GetValue() : a.GetValue();
            string Bvalue = b.type == Kind.CONST ? "#" + b.GetValue() : b.GetValue();

            PutInstruction(opCode, Avalue, Bvalue, lineNumber);
            Result res = new Result(Kind.REG, String.Format("({0})", lineNumber));

            return res;
        }

        // This function, along with the other puts output assembly in the following format:
        // Line#: opcode (argReg1) (argReg2)
        public static void PutInstruction(string opString, string a, string b, int lineNumber) {
            sw.WriteLine("{0}: {1} {2} {3}", lineNumber, opString, a, b);
            Console.WriteLine("{0}: {1} {2} {3}", lineNumber, opString, a, b);
        }

        public static void PutInstruction(string op, string a, double b, int lineNumber) {
            sw.WriteLine("{0}: {1} {2} {3}", lineNumber, op, b, a);
            Console.WriteLine("{0}: {1} {2} {3}", lineNumber, op, b, a);
        }

        //////////////////////////////////////////////
        // Function Instructions
        //////////////////////////////////////////////

        public static void FunctionEntry(Result functionName, int lineNumber) {
            sw.WriteLine("{0}: call {1}", lineNumber, functionName.GetValue().ToUpper());
            Console.WriteLine("{0}: call {1}", lineNumber, functionName.GetValue().ToUpper());
        }

        // puts argument on stack
        // lineNumber: the current line of assembly
        public static void StoreFunctionArgument(Result argument, int lineNumber) {
            sw.WriteLine("{0}: sub #4 $SP", lineNumber);
            Console.WriteLine("{0}: sub #4 $SP", lineNumber);
            Store(argument, new Result(Kind.REG, String.Format("({0})", lineNumber)), lineNumber + 1);
        }

        // gets argument from stack
        // Need to increment Assembly line by 3
        public static Result LoadFunctionArgument(int argumentOffset, Result arg, int lineNumber) {
            //sw.WriteLine("{0}: sub #{1} $FP", lineNumber, Math.Abs(argumentOffset));
            //Console.WriteLine("{0}: sub #{1} $FP", lineNumber, Math.Abs(argumentOffset));
            //sw.WriteLine("{0}: move ({1}) {2}", lineNumber + 1, lineNumber, arg.GetValue());
            //Console.WriteLine("{0}: move ({1}) {2}", lineNumber + 1, lineNumber, arg.GetValue());

            //            LoadVariable(new Result(Kind.REG, String.Format("({0})", lineNumber )), lineNumber);
            LoadVariable(arg, lineNumber);
            return new Result(Kind.REG, String.Format("({0})", lineNumber));
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
        public static void Store(Result thingToStore, Result whereToStore, int lineNumber) {
            sw.WriteLine("{1}: store {0} {2}", thingToStore.GetValue(), lineNumber, whereToStore.GetValue());
            Console.WriteLine("{1}: store {0} {2} ", thingToStore.GetValue(), lineNumber, whereToStore.GetValue());
        }

        // Returns the final line number ( so that you know what to set AssemblyPC to afterwards)
// Loads an array reference i.e. a[i][j]
        public static int LoadArrayElement(Result array, int[] dims, Result[] indices, int lineNumber) {
            Result[] inds = array.arrIndices;
            int addr = 0;
            int constantAccum = 0;
            int currentTerm = 1;
            Result currentResult = indices[indices.Length-1]; // store last indice 'cause it just gets added
            List<Result> termsToAdd = new List<Result>();

            for (int i = 0; i < indices.Length - 1; i++) {
                currentTerm = 1;
                for (int d = i + 1; d < dims.Length; d++) {
                    constantAccum *= dims[d];
                }
                if (indices[i].type == Kind.CONST) {
                    // Continue accumulating a constant address
                    addr += constantAccum * Int32.Parse(indices[i].GetValue());
                } else {
                    currentResult = new Result(Kind.REG, String.Format("({0}", lineNumber));
                    Console.WriteLine("{0}: mul #{1} {2}", lineNumber++, constantAccum, indices[i].GetValue());
                    sw.WriteLine("{0}: mul #{1} {2}", lineNumber++, constantAccum, indices[i].GetValue());
                    termsToAdd.Add(currentResult);
                }
                constantAccum = 1;
            }


            // check for constant address parts
            if (addr != 0 && currentResult != null)
                currentResult = PutArithmeticRegInstruction("add", new Result(Kind.CONST, addr), currentResult, lineNumber++);
            else if (addr != 0 && currentResult == null)
                currentResult = new Result(Kind.CONST, addr);

            // If there are no constant address parts, then currentResult sure to be initialized
            Debug.Assert(currentResult != null, "LoadArray has null result");
          

            foreach (Result r in termsToAdd) {
                // Add all of the terms
                currentResult = PutArithmeticRegInstruction("add", currentResult, r, lineNumber++);
            }

            // Multiply address by 4
            currentResult = PutArithmeticRegInstruction("mul", new Result(Kind.CONST, 4), currentResult, lineNumber++);
            // add FP + Base
            Result arrayBase = PutArithmeticRegInstruction("add", new Result(Kind.REG, "FP"), array, lineNumber++);
            // adda/store
            Console.WriteLine("{0}: adda {1} {2}", lineNumber, currentResult.GetValue(), arrayBase.GetValue());
            sw.WriteLine("{0}: adda {1} {2}", lineNumber++, currentResult.GetValue(), arrayBase.GetValue());
            
            Console.WriteLine("{0}: load ({1})", lineNumber, (lineNumber - 1));
            sw.WriteLine("{0}: load ({1})", lineNumber, (lineNumber - 1));
            lineNumber += 1;

            return lineNumber;

        }


        //////////////////////////////////////////////
        // Utility Functions
        //////////////////////////////////////////////

// TODO:: This needs to be fixed to reflect changes in Instruction Class
        // Writes out all instructions contained in a block 
        public static void WriteBlock(BasicBlock block) {
            Instruction currInstr = block.firstInstruction;
            while (currInstr != null) {
                sw.WriteLine(currInstr.ToString());
                Console.WriteLine(currInstr.ToString());
                currInstr = currInstr.next;
            }
        }
        // Writes out the the instructions in an if/while block
        // Adds the branching instruction's address to the instruction
        //##:b__ # branchAddress
        public static void WriteControlFlowBlock(BasicBlock block, String branchAddress) {
            Instruction currInstr = block.firstInstruction;
            while (currInstr != null) {
                if (currInstr.myResult.type == Kind.BRA)
                    Console.WriteLine(String.Format("{0} {1}", currInstr.ToString(), branchAddress));
                else
                    Console.WriteLine(currInstr.ToString());
                currInstr = currInstr.next;
                
            }
        }
    }
}
