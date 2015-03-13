using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {

    // Class for random yet useful functions
    public class Utilities {


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
                case Token.PHI:
                    opString = "phi";
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

        public static void PutInstruction(Token opcode, Result resA, Result resB, int lineNumber, ref InstructionManager im) {
            // Add updated instruction to newblock
            switch (opcode) {
                case Token.TIMES:
                case Token.DIV:
                case Token.PLUS:
                case Token.MINUS:
                    im.PutBasicInstruction(opcode, resA, resB, lineNumber);
                    break;
                case Token.LOAD:
                    im.PutLoadInstruction(resA, lineNumber);
                    break;
                case Token.BECOMES:
                    im.PutBasicInstruction(Token.BECOMES, resA, resB, lineNumber);
                    break;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                case Token.BRANCH:
                    if (KindToOperandType(resA) == Instruction.OperandType.BRANCH)
                        im.PutUnconditionalBranch(opcode, Int32.Parse(resA.GetValue()), lineNumber);
                    else if (KindToOperandType(resB) == Instruction.OperandType.BRANCH)
                        im.PutConditionalBranch(opcode, resA, resB.GetValue(), lineNumber);
                    else
                        im.PutCompare(opcode, resA, resB, lineNumber);
                    break;
                case Token.END:
                case Token.RETURN:
                    if (resB != null)
                        im.PutFunctionReturn(resA, lineNumber);
                    else
                        im.PutProcedureReturn(lineNumber);
                    break;
                case Token.PHI:
                case Token.STORE:
                case Token.OUTPUTNUM:
                case Token.INPUTNUM:
                case Token.OUTPUTNEWLINE:
                default:
                    im.PutBasicInstruction(opcode, resA, resB, lineNumber);
                    break;
            }
        }

        public static Kind OperandTypeToKind(Instruction.OperandType? type) {
            if (type == null)
                return Kind.ARR;

            switch (type) {
                case Instruction.OperandType.SSA_VAL:
                case Instruction.OperandType.REG:
                    return Kind.REG;
                case Instruction.OperandType.VAR:
                    return Kind.VAR;
                case Instruction.OperandType.CONSTANT:
                    return Kind.CONST;
                case Instruction.OperandType.BRANCH:
                    return Kind.BRA;
                case Instruction.OperandType.ARRAY:
                    return Kind.ARR;
                default:
                    Console.WriteLine("Should not have gotten here");
                    return Kind.ARR; // have to return something..... :(
            }
        }
        public static Instruction.OperandType KindToOperandType(Result res) {
            switch (res.type) {
                case Kind.REG:
                    return Instruction.OperandType.REG;
                case Kind.VAR:
                    return Instruction.OperandType.VAR;
                case Kind.CONST:
                    return Instruction.OperandType.CONSTANT;
                case Kind.BRA:
                    return Instruction.OperandType.BRANCH;
                case Kind.ARR:
                    return Instruction.OperandType.ARRAY;
                default:
                    Console.WriteLine("Should not have gotten here");
                    return Instruction.OperandType.ERROR;
            }
        }


        public static Queue<BasicBlock> TraverseCFG(ref BasicBlock start) {
            Queue<BasicBlock> CFG = new Queue<BasicBlock>();

            Stack<BasicBlock> blocks = new Stack<BasicBlock>();

            List<BasicBlock> children;
            List<BasicBlock> functions = new List<BasicBlock>();

            CFG.Enqueue(start);
            BasicBlock curr = start.childBlocks.Find(b => b.blockType == BasicBlock.BlockType.MAIN_ENTRY);
            CFG.Enqueue(curr);
            while (curr != null) {
                if (curr.childBlocks == null) {
                    curr = null;
                    break;
                }
                if (curr.childBlocks.Count > 1) {
                    // Case loopBody/follow
                    if (curr.blockType == BasicBlock.BlockType.LOOP_HEADER) {
                       // CFG.Enqueue(curr);
                        curr = HandleWhile(curr, ref CFG, ref functions);
                    } else {
                       // CFG.Enqueue(curr);
                        curr = HandleIf(curr, ref CFG);
                    }

                } else if (curr.childBlocks.Count == 1) {
                    curr = HandleAllOtherBlocks(curr, ref CFG, ref functions);

                } else {
                    // ?
                    Console.WriteLine("Shouldn't be here...");
                    curr = null;
                }

            }
            foreach (BasicBlock f in functions)
                CFG.Enqueue(f);
            return CFG;
        }

        private static BasicBlock HandleAllOtherBlocks(BasicBlock curr, ref Queue<BasicBlock> CFG, ref List<BasicBlock> functions) {
            curr = curr.childBlocks[0];
            if (curr.blockType == BasicBlock.BlockType.FUNCTION_HEADER)
                functions.Add(curr);
            else
                CFG.Enqueue(curr);
            return curr;

        }

        private static BasicBlock HandleIf(BasicBlock curr, ref Queue<BasicBlock> CFG) {
            // Case T/F

            List<BasicBlock> children = curr.childBlocks;
            BasicBlock tr = children.Find(b => b.blockType == BasicBlock.BlockType.TRUE);
            if (tr != null)
                CFG.Enqueue(tr);
            BasicBlock fs = children.Find(b => b.blockType == BasicBlock.BlockType.FALSE);
            if (fs != null)
                CFG.Enqueue(fs);
            if (tr != null)
                children = tr.childBlocks;
            else if (fs != null)
                children = fs.childBlocks;
            else {
                tr = children.Find(b => b.blockType == BasicBlock.BlockType.JOIN);
                CFG.Enqueue(tr);
                curr = tr;
            }
            curr = children.Find(b => b.blockType == BasicBlock.BlockType.JOIN);
            if (curr != null) {
                CFG.Enqueue(curr);
                Debug.Assert(curr.blockType == BasicBlock.BlockType.JOIN);
            }
            return curr;
        }

        private static BasicBlock HandleWhile(BasicBlock start, ref Queue<BasicBlock> CFG, ref List<BasicBlock> functions) {
           // loop header has already been pushed
            BasicBlock curr = start;           
            List<BasicBlock> children = curr.childBlocks;
            List<BasicBlock> thingsToadd = new List<BasicBlock>();
            int head = start.blockNum;
            int couldBeTheEnd = -3;

             curr = children.Find(b => b.blockType == BasicBlock.BlockType.LOOP_BODY);
            BasicBlock Follow = children.Find(b => b.blockType == BasicBlock.BlockType.FOLLOW);

            if (curr != null)
                CFG.Enqueue(curr);
            // header and body has been pushed
// curr is the loop body block
            do {
                if (curr.childBlocks == null) {
                    curr = null;
                    break;
                }
                if (curr.childBlocks.Count > 1) {
                    // Case loopBody/follow
                    if (curr.blockType == BasicBlock.BlockType.LOOP_HEADER) {
                        //if (curr.blockNum != head)
                          //  CFG.Enqueue(curr);
                        curr = HandleWhile(curr, ref CFG, ref functions);
                    } else {
                        // CFG.Enqueue(curr);
                        curr = HandleIf(curr, ref CFG);
                    }

                } else if (curr.childBlocks.Count == 1) {
                    if (curr.childBlocks[0].blockNum == head) {
                        // we don't want to add it
                        curr = curr.childBlocks[0];
                        break;
                    }
                    curr = HandleAllOtherBlocks(curr, ref CFG, ref functions);

                } else {
                    // ?
                    Console.WriteLine("Shouldn't be here...");
                }

            } while(curr.blockNum != head);

            

            CFG.Enqueue(Follow);
            curr = Follow;
            Debug.Assert(curr.blockType == BasicBlock.BlockType.FOLLOW);

            return Follow;
        }

    }
}
