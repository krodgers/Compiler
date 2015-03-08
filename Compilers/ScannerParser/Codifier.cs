using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ScannerParser {
    // Contains methods to write out DLX processor code
    class Codifier {

        private StreamWriter sw;
        private int AssemblyPC, currRegister;

        // TODO:: What thing should be passed in?  Reference to CFG?
        public Codifier(string outputFileName) {
            sw = new StreamWriter(new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write));

        }


        ////////////////////////////////////
        //  General Instructions	      //
        ////////////////////////////////////




        ///////////////////////////
        // Function Instructions //
        ///////////////////////////
        private void FunctionEntryCode(int numLocals) {

            // DLX form
            // push return address
            // push current FP
            // push callee save registers // Can we get away with making the caller push all needed registers?
            // set fp = sp - 4*numLocals
            // set sp = fp



        }

        // Generates the code that should happen upon exiting a function
        private void FunctionExitCode(int numLocals, Result returnValue) {
            // Put ret val in ret reg
            // reset stackpointer: SP = FP + 4*numLocals
            // pop callee save registers
            // pop/restore FP

            // pop return address into $ra
            // return to caller --> jump $ra
        }




        //////////////////////////////////////
        // Memory Access Instrutions	    //
        //////////////////////////////////////




        //////////////////////////////////////////////
        // Branching/Comparing  Instructions	    //
        //////////////////////////////////////////////



        ///////////////////////////////
        // Input/Output Instructions //
        ///////////////////////////////



        /////////////////////////////////
        // Fake Register Things	       //
        /////////////////////////////////







        // This function puts an arithmetic instruction where all arguments are registers
        // or variables (but will need to only be registers in the final output, but this
        // should be handled by SSA)
        private Result PutArithmeticRegInstruction(string opCode, Result a, Result b) {

            if (a.type == Kind.REG && b.type == Kind.REG)
                PutF2(opCode, a.GetValue(), b.GetValue());
            else if (a.type == Kind.REG && b.type == Kind.VAR) {
                PutF2(opCode, a.GetValue(), b.GetValue());
            } else if (a.type == Kind.VAR && b.type == Kind.REG) {
                PutF2(opCode, a.GetValue(), b.GetValue());
            } else {
                PutF2(opCode, a.GetValue(), b.GetValue());
            }

            Result res = new Result(Kind.REG, String.Format("({0})", AssemblyPC));
            AssemblyPC++;
            return res;
        }

        // This function puts an arithmetic instruction where the first arguments is a register
        // or a variable and the second argument is an immediate
        private Result PutArithmeticImmInstruction(string opCode, Result a, Result b) {

            opCode += "i";

            if (a.type == Kind.REG && b.constantType == ConstantType.DOUBLE)
                PutF1(opCode, a.GetValue(), b.GetValue());
            else if (a.type == Kind.REG && b.constantType == ConstantType.STRING) {
                PutF1(opCode, a.GetValue(), b.GetValue());
            } else if (a.type == Kind.VAR && b.constantType == ConstantType.DOUBLE) {
                PutF1(opCode, a.GetValue(), b.GetValue());
            } else {
                PutF1(opCode, a.GetValue(), b.GetValue());
            }

            Result res = new Result(Kind.REG, String.Format("({0})", AssemblyPC));
            AssemblyPC++;
            return res;
        }

        // todo, both PutF1 and PutF2 were changed to take strings rather than Results because
        // registers are not available
        // This function, along with the other puts output assembly in the following format:
        // opcode (resultReg) (argReg1) (argReg2)
        // This may need to be changed because in his examples the instructions only have
        // two arguments and do not have the result register specified.
        private void PutF2(string opString, string a, string b) {
            sw.WriteLine("{4}: {1} {2} {3}", opString, String.Format("({0})", AssemblyPC), a, b, AssemblyPC);
            Console.WriteLine("{4}: {0} {1} {2} {3}", opString, String.Format("({0})", AssemblyPC), a, b, AssemblyPC++);
        }


        private void PutF1(string op, string a, double b) {
            sw.WriteLine("{4}: {0} {1} {2} {3}", op, String.Format("({0})", AssemblyPC), a, b, AssemblyPC);
            Console.WriteLine("{4}: {0} {1} {2} {3}", op, String.Format("({0})", AssemblyPC), a, b, AssemblyPC++);
        }

        // Creates a F1 instruction
        // Result b should be the Constant
        private void PutF1(string op, string a, string b) {
            sw.WriteLine("{4}: {0} {1} {2} {3}", op, String.Format("({0})", AssemblyPC), a, b, AssemblyPC);
            Console.WriteLine("{4}: {0} {1} {2} {3}", op, String.Format("({0})", AssemblyPC), a, b, AssemblyPC++);
            //if (b.type == Kind.CONST) {
            //    sw.WriteLine("{0} {1} {2} {3}", op, currRegister, a.regNo, b.GetValue());
            //    Console.WriteLine("{0} {1} {2} {3}", op, currRegister, a.regNo, b.GetValue());

            //}
            //else if (a.type == Kind.COND) {
            //    // todo, the value for the second parameter shouldn't be a string
            //    // it needs to be the offset, but don't know how to do that yet
            //    sw.WriteLine("{0} {1} {2}", op, a.regNo, b.GetValue());
            //    Console.WriteLine("{0} {1} {2}", op, a.regNo, b.GetValue());
            //}
            //else {
            //    Console.WriteLine("PutF1 paramters in wrong order.");
            //    sw.WriteLine("{0} {1} {2} {3}", op, currRegister, b.regNo, a.GetValue());
            //    Console.WriteLine("{0} {1} {2} {3}", op, currRegister, b.regNo, a.GetValue());

        }
        // TODO:: When storing/loading, need 
        // ADDA Base_Ptr Array_Loc
        // LOAD/STORE

        private void AllocateRegister() {
            currRegister++;
        }
        private void DeallocateRegister() {
            currRegister--;
        }



        // Stores an argument at SP-4
        private void StoreFunctionArgument(Result Argument) {
            Result SP = new Result(Kind.REG, "$SP");
            Result FP = new Result(Kind.REG, "$FP");
            Result PC = new Result(Kind.REG, "$PC");
            Result ConstFour = new Result(Kind.CONST, 4);
            Result currRes;

            //  currRes = Combine(Token.MINUS, SP, ConstFour);
            //  Store(Argument, currRes);

        }




        public void StoreVariable(Result thingToStore, Result whereToStore, int lineNumber) {
            if (whereToStore.type == Kind.CONST && whereToStore.constantType == ConstantType.ADDR) {
                sw.WriteLine("{2}: store MEM[{1}] {0}", thingToStore.GetValue(), whereToStore.GetValue(), lineNumber);
                Console.WriteLine("{2}: store MEM[{1}] {0}", thingToStore.GetValue(), whereToStore.GetValue(), lineNumber);

            } else {
                Console.WriteLine("WARNING: Attempting to store in something that isn't a register");
            }
        }


    }
}
