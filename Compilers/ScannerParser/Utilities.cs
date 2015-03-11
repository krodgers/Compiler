using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

    // Class for random yet useful functions
   public  class Utilities {


// Checks the output in filename against the expected string tokens in expected
// Do not put line numbers in expected
// example : string[] expected  = {"mul", "i", "j"} when you want "1: mul i j"
// filename - the name of the file to use for comparing to expected
        public static bool CheckFile(string filename, string[] expected) {
            StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            string result = sr.ReadToEnd();
            string[] delims = { " ", "\n", "\r\n", "1:", "2:", "3:", "4:", "5:", "6:", "7:", "8:", "9:", "0:" };
            string[] splitted = result.Split(delims, StringSplitOptions.RemoveEmptyEntries);


            string[] splittedString = { " ", " ", " " };
            string[] expectedString = { " ", " ", " " };

            for (int s = 0; s < splitted.Length; s++) {
                // Clear out arrays so only one instruction is in them
                if (s % 3 == 0) {
                    splittedString[0] = splittedString[1] = splittedString[2] = " ";
                    expectedString[0] = expectedString[1] = expectedString[2] = " ";
                }

                splittedString[s % 3] = splitted[s];
                expectedString[s % 3] = expected[s];
                if (!splitted[s].Equals(expected[s])) {
                    Console.WriteLine("Failed");

                    Console.WriteLine("Got: {0} {1} {2} Wanted: {3} {4} {5}", splittedString[0], splittedString[1], splittedString[2], expectedString[0], expectedString[1], expectedString[2]);
                    sr.Dispose();
                    return false;
                }

            }
            sr.Dispose();
            Console.WriteLine("Passed");
            return true;
        }

        // opens SSAWriter file stream -- useful for testing stuffs
        public static FileStream OpenStreams(string filename) {
            FileStream fs = File.Open(filename, FileMode.Create, FileAccess.ReadWrite);
            System.Diagnostics.Debug.Assert(fs != null);
            SSAWriter.sw = new StreamWriter(fs);
            return fs;
        }

       // converts token to SSA instruction
        public static string TokenToInstruction(Token t) {
            string opString;
            switch (t) {
                case Token.TIMES:
                    opString = "mul";
                    break;
                case Token.DIV:
                    opString = "div";
                    break;
                case Token.PLUS:
                    opString = "add";
                    break;
                case Token.MINUS:
                    opString = "sub";
                    break;
                case Token.BECOMES:
                    opString = "mov";
                    break;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                    opString = "cmp";
                    break;
                case Token.LOAD:
                    opString = "load";
                    break;
                case Token.STORE:
                    opString = "store";
                    break;
                case Token.BRANCH:
                    opString = "bra";
                    break;
                case Token.OUTPUTNUM:
                    opString = "write";
                    break;
                case Token.INPUTNUM:
                    opString = "read";
                    break;
                case Token.OUTPUTNEWLINE:
                    opString = "wrl";
                    break;
                case Token.END:
                case Token.RETURN:
                    opString = "ret";
                    break;
                default:
                    opString = "nop";
                    break;

            }
            return opString;
        }

        // returns the negated form of the condition
        public static Token NegatedConditional(Token cond) {
            // todo, do we even need CondOp? It seems redundant
            switch (cond) {
                case Token.EQL:
                    return Token.NEQ;
                case Token.NEQ:
                    return Token.EQL;
                case Token.LSS:
                    return Token.GEQ;
                case Token.GTR:
                    return Token.LEQ;
                case Token.LEQ:
                    return Token.GTR;
                case Token.GEQ:
                    return Token.LSS;
                default:
                    return Token.ERROR;
            }
        }


        public static string TokenToBranchInstruction(Token opCode) {
            switch (opCode) {
                case Token.EQL:
                    return "beq";
                case Token.NEQ:
                    return "bne";
                case Token.LSS:
                    return "blt";
                case Token.GTR:
                    return "bgt";
                case Token.LEQ:
                    return "ble";
                case Token.GEQ:
                    return "bge";
                default:
                    return String.Empty;
            }
        }

        public static OpCodeClass GetOpCodeClass(Token opCode, bool immediate = false) {
            switch (opCode) {
                case Token.TIMES:
                case Token.DIV:
                case Token.PLUS:
                case Token.MINUS:
                    if (immediate)
                        return OpCodeClass.ARITHMETIC_IMM;
                    else
                        return OpCodeClass.ARITHMETIC_REG;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GTR:
                case Token.LEQ:
                case Token.GEQ:
                    return OpCodeClass.COMPARE;
                case Token.BECOMES:
                    return OpCodeClass.MEM_ACCESS;
                default:
                    return OpCodeClass.ERROR;
            }
        }
   }
}
