using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ScannerParser {
    // Contains methods to write out DLX processor code

    public class Codifier {
        public enum REG { R0 = 0, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12, 
            R13, R14, R15, R16, R17, R18, R19, R20, R21, R22, R23, R24, R25, R26, R27, 
            FP = 28, SP = 29, RGLOBALS = 30, RA = 31 , ERROR};


        private StreamWriter sw;
        private int nextExtraSSAVal;
        private int assemblyPc, currRegister;
        private Dictionary<int,REG> ssaToReg; // maps ssa values to registers
    //    private Dictionary<int, REG> varToReg; // maps vars to registers
        private List<REG> availableRegs;
        private Dictionary<string, int> symbols; // maps symbol string to ssaVals

        // TODO:: What thing should be passed in?  Reference to CFG?
        public Codifier(string outputFileName, int numberofLinesInFile) {
            sw = new StreamWriter(new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write));
            sw.AutoFlush = true;
            nextExtraSSAVal = numberofLinesInFile + 5; // for good measure
            init();

        }
        

        private void init() {
            ssaToReg = new Dictionary<int, REG>();
           // varToReg = new Dictionary<int, REG>();
            availableRegs = new List<REG>();
            foreach (REG r in Enum.GetValues(typeof(REG))) {
                if (r != REG.R0 && r != REG.FP && r != REG.SP && r != REG.RGLOBALS && r != REG.RA && r != REG.ERROR)
                    availableRegs.Add(r);
            }
            symbols = new Dictionary<string, int>();
        }
        public void CloseFiles() {
            if (sw != null) {
                sw.Flush();
                sw.Close();
            }
        }

        // Writes error message and exits
        public void Error(string msg) {
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
            Write(msg);
            sw.Dispose();
            Console.ReadLine();
            System.Environment.Exit(0);
        }

        // symbolsAndOffsets -- the symbols relevant to the current block
        public void CodifyBlock(BasicBlock bb) {
//            init();  // I don't think we want to reset the symbol table and such for every block
            Instruction currInstr = bb.firstInstruction;
            int numInstrUsed = 0;
            while (currInstr != null) {
                numInstrUsed = WriteInstruction(currInstr.opCode, currInstr);
                if (numInstrUsed != 1) {
                    for (int i = 0; i < numInstrUsed; i ++ )
                        currInstr = currInstr.next;
                } else
                    currInstr = currInstr.next;
            }
            
        }
        
        ////////////////////////////////////
        //  General Instructions	      //
        ////////////////////////////////////
        // Returns the number of instructions consumed
        public int WriteInstruction(Token opCode, Instruction currInstr) {

            switch (opCode) {
                // math instructions
                case Token.TIMES:
                case Token.DIV:
                case Token.PLUS:
                    HandleMathInstruction(opCode, currInstr);
                    return 1;
                case Token.MINUS:
                    if (currInstr.secondOperand.Equals("$SP")) {
                        // signals pushing
                        currInstr = currInstr.next;
                        HandleMemoryInstruction(currInstr.opCode, currInstr);
                        return 2;

                    } else if (currInstr.secondOperand.Equals("$FP")) {
                        // needs to be loaded from memory
                        HandleMemoryInstruction(opCode, currInstr);
                        return 2;
                    }

                    HandleMathInstruction(opCode, currInstr);
                    return 1;
                    
                // Memory Instructions
                case Token.BECOMES:
                case Token.STORE:
                case Token.LOAD:
                    HandleMemoryInstruction(opCode, currInstr);
                    return 1;
                    
                // Function Instruction
                // Branching Instruction
                case Token.BRANCH:
                case Token.CALL:
                case Token.END:
                case Token.RETURN:
                case Token.INPUTNUM:
                case Token.OUTPUTNUM:
                case Token.OUTPUTNEWLINE:
                    HandleFunctionInstruction(opCode, currInstr);
                    return 1; // HOW MANY??
                    
                // Comparing Instruction
                case Token.CHECK:
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                    // Is an unconditional branch
                    if (currInstr.firstOperandType == Instruction.OperandType.BRANCH) {
                        PutUnconditionalBranch(currInstr);
                        return 1;

                    } else if (currInstr.secondOperandType == Instruction.OperandType.BRANCH){
                        // is a conditional branch
                        HandleBranchInstruction(opCode, currInstr);
                        return 1;
                    } else {
                        // must be a compare instruction
                        HandleCompareInstruction(opCode, currInstr);
                        return 2; // eats cmp _ _ and bra _ _ 

                    }

                case Token.PHI:
                    return 1;
                default:
                    Error(String.Format("Unable to classify opcode {0}", opCode));
                    break;
            }
            return -1;
        }


        private void HandleMathInstruction(Token opCode, Instruction currInstr) {
            int opB, opC;
            REG bVal;
            int cVal;
            REG storageReg = GetAvailableReg(); // TODO:: This needs to be fixed.  but while we don't have register allocation...
//            AssignRegToSsaVal(storageReg, currInstr.instructionNum);
              AssignRegToSsaVal(storageReg, currInstr.instructionNum);

              bool isImmediate = CheckImmediateAndReorder(currInstr, out opB, out opC);
            if (isImmediate) {
                bVal = GetRegister(opB);
            //    cVal =  Int32.Parse(Regex.Match(currInstr.secondOperand, "[0-9]+").Value);
                cVal = opC;
            } else {
                bVal = GetRegister(opB);
                cVal = (int) GetRegister(opC);
            }
            
            PutMathInstruction(opCode, storageReg, bVal, cVal, isImmediate);

           
        }

        // puts opcode A B C
        // operandC has to be an int in case it's an immediate
        private void PutMathInstruction(Token opCode, REG whereToStore, REG operandB, int operandC, bool isImmediate) {
            if (isImmediate) 
                PutImmInstruction(opCode, (int)whereToStore, (int)operandB, operandC);
             else 
                PutInstruction(opCode, (int)whereToStore, (int)operandB, operandC);

            assemblyPc++;
        }



        ///////////////////////////
        // Function Instructions //
        ///////////////////////////
        private void HandleFunctionInstruction(Token opCode, Instruction currInstr) {
            switch (opCode) {
                case Token.CALL:
                    FunctionEntryCode(currInstr);
                    break;
                case Token.END:
                    PutImmInstruction(Token.RETURN, 0);
                    break;
                case Token.RETURN:
                    FunctionExitCode(currInstr);
                    break;
                case Token.OUTPUTNEWLINE:
                    Write("wrl");
                    break;
                case Token.OUTPUTNUM:
                    PutInstruction(Token.OUTPUTNUM, (int)GetRegister(currInstr.firstOperandSSAVal));
                    break;
                case Token.INPUTNUM:
                    PutInstruction(Token.INPUTNUM, (int)GetRegister(currInstr.firstOperandSSAVal));
                    break;
            }
        }

        private void FunctionEntryCode(Instruction instr) {

            // DLX form
            // push return address
            Write("psh");
            // push current FP
            // push callee save registers // Can we get away with making the caller push all needed registers?
            // set fp = sp - 4*numLocals
            // set sp = fp



        }

        // Generates the code that should happen upon exiting a function
        private void FunctionExitCode(Instruction instr) {
// NEED TO STORE ALL GLOBAL VARIABLES

            // Put ret val in ret reg
            // reset stackpointer: SP = FP + 4*numLocals
            // pop callee save registers
            // pop/restore FP

            // pop return address into $ra
            // return to caller --> jump $ra
            switch (instr.opCode) {
                case Token.RETURN:
                case Token.END:
                    int retval, useless;
                    if (CheckImmediateAndReorder(instr, out useless, out retval)) {
                        PutImmInstruction(instr.opCode, retval);
                    } else {
                        PutImmInstruction(instr.opCode, 0);
                    }
                    break;
            }
        }

        // Stores $RA and branches to 4*c
        private void BranchAndLink(int c) {
            PutImmInstruction("bsr", c);
        }
        // Stores $RA and jumps to addr (absolute)
        private void JumpAndLink(int addr) {
            PutImmInstruction("jsr", addr);
        }


        //////////////////////////////////////
        // Memory Access Instrutions	    //
        //////////////////////////////////////
        private void HandleMemoryInstruction(Token opCode, Instruction currInstr) {
            int opA, opB;
            if (opCode == Token.BECOMES) {
                // assign the SSAval a register
                // TODO:: virtual registers
                REG newReg = GetAvailableReg();
                AssignRegToSsaVal(newReg, currInstr.instructionNum);


                if (CheckImmediateAndReorder(currInstr, out opA, out opB)) {
                    // essentially a move
                    PutConstantInRegister(newReg, opB);

                } else {
                    // will return the register associated with this value
                    if (opB == 0) {
                        // has no SSAValue yet
                        if (currInstr.secondOperandType == Instruction.OperandType.VAR) {
                            REG variReg = GetAvailableReg();
                            AssignRegToSsaVal(variReg, currInstr.instructionNum);
                            AssignSSAToVar(currInstr.instructionNum, currInstr.secondOperand);
                            PutMathInstruction(Token.PLUS, variReg, REG.R0, (int)GetRegister(opA), false);
                        }
                    }
               
                   
                }

            } else if (opCode == Token.STORE) {
                Push(currInstr);
            } else if (opCode == Token.MINUS) {
                // stack operation
                if (!currInstr.secondOperand.Equals("$FP"))
                    Error("Ended up in memory handler without FP as argument");
                if (currInstr.firstOperandType != Instruction.OperandType.CONSTANT)
                    Error("In memory -- need a constant for FP offset");

                // firstOper == offset
                int offset = Int32.Parse(currInstr.firstOperand);
                currInstr = currInstr.next;

                if (currInstr.opCode != Token.LOAD)
                    Error("Load didn't follow modifying FP");

                int ssaVal = symbols[currInstr.firstOperand];

                if (ssaVal == -1)
                    Error("Symbol uninitialized -- has no SSAVAl on record");

                Load((int)GetRegister(ssaVal), (int)REG.FP, -offset, false);
            }
        }


        private void Push(Instruction instr) {
            // store thing place --- place is irrelevant (?)
            REG restingPlace = GetAvailableReg();
            if (instr.firstOperandType == Instruction.OperandType.CONSTANT) {
                PutConstantInRegister(restingPlace, Int32.Parse(instr.firstOperand));
                PutInstruction("psh", (int)restingPlace, (int)REG.SP, -4);
            } else {
                PutInstruction("psh", (int)GetRegister(instr.firstOperandSSAVal), (int)REG.SP, -4);
            }
            ReleaseRegister(restingPlace);
        }

        private void Pop(int whereToStore, int memLoc, int offset = -4) {
            // pop A B C --- pop whereToPopTo MemLoc howMuchToDecreaseBy
            PutInstruction("pop", whereToStore, memLoc, offset);
        }

        private void Store(int whatToStore, int memBase, int offset, bool offsetIsRegister) {
            if (offsetIsRegister) {
                PutInstruction("stx", whatToStore, memBase, offset);
            } else {
                PutInstruction("stw", whatToStore, memBase, offset);
            }
        }
        
        private void Load(int whereToStore, int memBase, int offset, bool offsetIsRegister) {
            if (offsetIsRegister) {
                PutInstruction("ldx", whereToStore, memBase, offset);
            } else {
                PutInstruction("ldw", whereToStore, memBase, offset);
            }
        }


        //////////////////////////////////////////////
        // Branching/Comparing  Instructions	    //
        //////////////////////////////////////////////
        private void HandleBranchInstruction(Token opCode, Instruction currInstr) {
            // Check for wrong opcode
            switch (opCode) {
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                    break;
                default:
                    Error("In compare but didn't get a compare opcode");
                    break;
            }  
            int opA, opB;
            if (CheckImmediateAndReorder(currInstr, out opA, out opB)) {
                PutImmBranchInstruction(opCode, (int)GetRegister(opA), opB);
            } else {
                // location assumed to be a string
                PutBranchInstruction(opCode, (int)GetRegister(opA), currInstr.firstOperand);
            }
        }


        private void HandleCompareInstruction(Token opCode, Instruction currInstr) {
            // check for wrong opcode
            switch (opCode) {
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                    break;
                default:
                    Error(String.Format("In compare but didn't get a compare opcode: got {0}", opCode));
                    break;
            }

            REG storage = GetAvailableReg();
            int opA, opB;
            REG A, B;
            // put cmp instruction
            if (CheckImmediateAndReorder(currInstr, out opA, out opB)) {
                A = GetRegister(opA);
                PutImmInstruction(opCode, (int)storage, (int)A, opB);
            } else {
                A = GetRegister(opA);
                B = GetRegister(opB);
                PutInstruction(opCode, (int)storage, (int)A, (int)B);
            }
            // put branch instruction
            currInstr = currInstr.next;
            // branch result location
            if (CheckImmediateAndReorder(currInstr, out opA, out opB)) {
                PutImmBranchInstruction(opCode, (int)storage, opB);
            } else {
                // assuming branch location is a string.....
                PutBranchInstruction(opCode, (int)storage, currInstr.secondOperand);
            }

            // free the comparision registerx
            ReleaseRegister(storage);

        }

        private void PutUnconditionalBranch(Instruction instr) {
            PutBranchInstruction(Token.EQL, 0, Int32.Parse(instr.firstOperand));
        }


        private void PutBranchInstruction(Token opcode, int registerToCheck, int placeToBranch) {
            sw.WriteLine(String.Format("{0} {1} {2}", Utilities.TokenToBranchInstruction(opcode), registerToCheck, placeToBranch));
            Console.WriteLine(String.Format("{0} {1} #{2}", Utilities.TokenToBranchInstruction(opcode), (REG)registerToCheck, placeToBranch));
            assemblyPc++;
        }

        private void PutBranchInstruction(Token opcode, int registerToCheck, string placeToBranch) {
            sw.WriteLine(String.Format("{0} {1} {2}", Utilities.TokenToBranchInstruction(opcode), registerToCheck, placeToBranch));
            Console.WriteLine(String.Format("{0} {1} {2}", Utilities.TokenToBranchInstruction(opcode), (REG)registerToCheck, placeToBranch));
            assemblyPc++;
        }
        private void PutImmBranchInstruction(Token opcode, int registerToCheck, int placeToBranch) {
            sw.WriteLine(String.Format("{0} {1} {2}", Utilities.TokenToBranchInstruction(opcode), registerToCheck, placeToBranch));
            Console.WriteLine(String.Format("{0} {1} #{2}", Utilities.TokenToBranchInstruction(opcode), (REG)registerToCheck, placeToBranch));
            assemblyPc++;        
        }
        

        ///////////////////////////////
        // Input/Output Instructions //
        ///////////////////////////////



        /////////////////////////////////
        // Fake Register Things	       //
        /////////////////////////////////
        public void AssignSSAToVar(int ssaVal, string varName){
            symbols[varName] = ssaVal;
                       
        }


        public void AssignRegToSsaVal(REG regNo, int ssaVal) {
            CheckRegAvailability(regNo); // will exit if not free
            ssaToReg[ssaVal] = (REG) regNo;
            availableRegs.Remove(regNo);
        }

        // Free register
        public void ReleaseRegister(REG regNo) {
            availableRegs.Add(regNo);

        }
        // Returns true if regNo is available to be used
        public bool CheckRegAvailability(REG regNo) {
            if (availableRegs.Contains(regNo))
                return true;

            Error(String.Format("Register {0} is unavailable", regNo));
            return false;
        }


        // Returns next available register
        // Does not allocate a register --> call AssignX() to allocate
        // Returns -1 if none available
        public REG GetAvailableReg() {
            if (availableRegs.Count == 0)
                return REG.ERROR;
            else {
                REG pickMe = availableRegs[0];
                return pickMe;
            }

        }
        ///////////////////////////////////
        // Utilities                     //
        ///////////////////////////////////
        private void PutInstruction(Token opcode, int A, int B, int C) {
            sw.WriteLine(String.Format("{0} {1} {2} {3}", TokenToInstruction(opcode), A, B, C));
            Console.WriteLine(String.Format("{0} {1} {2} {3}", TokenToInstruction(opcode), (REG)A, (REG)B, (REG)C));
            assemblyPc++;
        }
        private void PutImmInstruction(Token opcode, int A, int B, int C) {
            sw.WriteLine(String.Format("{0}i {1} {2} {3}", TokenToInstruction(opcode), A, B, C));
            Console.WriteLine(String.Format("{0}i {1} {2} #{3}", TokenToInstruction(opcode), (REG)A, (REG)B, C));
            assemblyPc++;
        }
        private void PutInstruction(string opcode, int A, int B, int C) {
            sw.WriteLine(String.Format("{0} {1} {2} {3}", opcode, A, B, C));
            Console.WriteLine(String.Format("{0} {1} {2} {3}", opcode, (REG)A, (REG)B, (REG)C));
            assemblyPc++;

        }

        private void PutInstruction(Token opcode, int A, int B) {
            sw.WriteLine(String.Format("{0} {1} {2}", TokenToInstruction(opcode), A, B));
            Console.WriteLine(String.Format("{0} {1} {2}", TokenToInstruction(opcode), (REG)A, (REG)B));
            assemblyPc++;

        }
        private void PutImmInstruction(Token opcode, int A, int B) {
            sw.WriteLine(String.Format("{0} {1} {2}", TokenToInstruction(opcode), A, B));
            Console.WriteLine(String.Format("{0} {1} #{2}", TokenToInstruction(opcode), (REG)A, B));
            assemblyPc++;

        }
        private void PutInstruction(string opcode, int A, int B) {
            sw.WriteLine(String.Format("{0} {1} {2}", opcode, A, B));
            Console.WriteLine(String.Format("{0} {1} {2}", opcode, (REG)A, (REG)B));
            assemblyPc++;

        }
        private void PutInstruction(Token opcode, int A) {
            sw.WriteLine(String.Format("{0} {1}", TokenToInstruction(opcode), A));
            Console.WriteLine(String.Format("{0} {1}", TokenToInstruction(opcode), (REG)A));
            assemblyPc++;

        }
        private void PutInstruction(string opcode, int A) {
            sw.WriteLine(String.Format("{0} {1}", opcode, A));
            Console.WriteLine(String.Format("{0} {1}", opcode, (REG)A));
            assemblyPc++;
        }
        private void PutImmInstruction(Token opcode, int A) {
            sw.WriteLine(String.Format("{0} {1}", TokenToInstruction(opcode), A));
            Console.WriteLine(String.Format("{0} #{1}", TokenToInstruction(opcode), A));
            assemblyPc++;
        }
          private void PutImmInstruction(string opcode, int A) {
            sw.WriteLine(String.Format("{0} {1}", opcode, A));
            Console.WriteLine(String.Format("{0} #{1}", opcode, A));
            assemblyPc++;
        }


        private void Write(string toWrite) {
            sw.WriteLine(toWrite);
            Console.WriteLine(toWrite);
        }

        private void PutConstantInRegister(REG storage, int constant) {
            PutImmInstruction(Token.PLUS, (int)storage, 0, constant);
        }

        // Returns the register associated with the ssaValue
        // ssaValue - ssa value associated with value wanted
        // type - operandtype of the value wanted
        // valueString - Instruction.Xoperand
        public REG GetRegister(int ssaValue) {
         
            if (ssaToReg.ContainsKey(ssaValue))                
                return ssaToReg[ssaValue];

            return REG.ERROR;

        }
       
        /// <summary>
        /// Checks if the instruction has a constant as a value. Reorders operands if needed.
        ///  Returns the SSA val of anything not a constant
        /// </summary>
        /// <param name="instr"> The crrent instruction</param>
        /// <param name="opAValue"> out param: returns the SSA value of the operand</param>
        /// <param name="opBValue">out param: returns the value of the immediate operand</param>
        /// <returns>True if has a constant operand, false else</returns>
        private bool CheckImmediateAndReorder(Instruction instr, out int opAValue, out int opBValue) {
            bool immediate = false;

            // Can't have both be constants
            if (instr.firstOperandType == Instruction.OperandType.CONSTANT && instr.secondOperandType == Instruction.OperandType.CONSTANT) {
                if (CheckDoubleConstants(instr, out opAValue, out opBValue)) {
                    immediate = true;
                    AssignRegToSsaVal((REG)opAValue, nextExtraSSAVal);

                    opAValue = nextExtraSSAVal++;
                    opBValue = Int32.Parse(instr.secondOperand);
                    
                }
            } else if (instr.firstOperandType == Instruction.OperandType.CONSTANT) {
                immediate = true;
                Debug.Assert(instr.secondOperandType != Instruction.OperandType.CONSTANT, "Both operands are constants");
                opAValue = instr.secondOperandSSAVal;
                opBValue = Int32.Parse(instr.firstOperand); // 1st operand is the immediate value


            } else if (instr.secondOperandType == Instruction.OperandType.CONSTANT) {
                immediate = true;
                opAValue = instr.firstOperandSSAVal;
                opBValue = Int32.Parse(instr.secondOperand);
                Debug.Assert(instr.firstOperandType != Instruction.OperandType.CONSTANT, "Both operands are constants");
            } else {
                // no constants
                opAValue = instr.firstOperandSSAVal;
                opBValue = instr.secondOperandSSAVal;
            }


            return immediate;
        }
        /// <summary>
        /// Checks if both operands are constants
        /// If so, puts one into a register
        /// If returns false, values in opAValue and opBValue are invalid
        /// </summary>
        /// <param name="instr"></param>
        /// <param name="opAValue"> The register value the immediate got put into</param>
        /// <param name="opBValue"> The value of the constant</param>
        /// <returns> true if both were constants; opBvalue is a constant value</returns>
        private bool CheckDoubleConstants(Instruction instr, out int opAValue, out int opBValue) {
            bool immediate = false;

//Both are constants
            if (instr.firstOperandType == Instruction.OperandType.CONSTANT && instr.secondOperandType == Instruction.OperandType.CONSTANT) {
                // load one into a register
                REG storage = GetAvailableReg();
                PutConstantInRegister(storage, Int32.Parse(instr.firstOperand));
                opAValue = (int)storage;
                opBValue = Int32.Parse(instr.secondOperand);

                immediate = true;
            }
                //}else  if (instr.firstOperandType == Instruction.OperandType.CONSTANT) {
                //    immediate = true;
                //    Debug.Assert(instr.secondOperandType != Instruction.OperandType.CONSTANT, "Both operands are constants");
                //    opAValue = instr.secondOperandSSAVal;
                //    opBValue = Int32.Parse(instr.firstOperand); // 1st operand is the immediate value


            //} else if (instr.secondOperandType == Instruction.OperandType.CONSTANT) {
                //    immediate = true;
                //    opAValue = instr.firstOperandSSAVal;
                //    opBValue = Int32.Parse(instr.secondOperand);
                //    Debug.Assert(instr.firstOperandType != Instruction.OperandType.CONSTANT, "Both operands are constants");
                //} else {
                //    // no constants
                //    opAValue = instr.firstOperandSSAVal;
                //    opBValue = instr.secondOperandSSAVal;
                //}
            else {
                opAValue = -3;
                opBValue = -4;
            }

            return immediate;
        }


        // Returns the instruction associated with the token
        private string TokenToInstruction(Token t) {
            string opString = String.Empty;
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
                case Token.CHECK:
                    opString = "chk";
                    break;    
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GTR:
                case Token.LEQ:
                case Token.GEQ:
                    opString =  "cmp";
                    break;
                case Token.OUTPUTNUM:
                    opString = "wrd";
                    break;
                case Token.INPUTNUM:
                    opString = "rdd";
                    break;
                case Token.OUTPUTNEWLINE:
                    opString = "wrl";
                    break;
                case Token.END:
                case Token.RETURN:
                    opString = "ret";
                    break;
               
                default:
                    Error(String.Format("Unsupported token {0}", t));
                    break;

            }
            return opString;
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
                Write(String.Format("{2}: store MEM[{1}] {0}", thingToStore.GetValue(), whereToStore.GetValue(), lineNumber));
     

            } else {
                Console.WriteLine("WARNING: Attempting to store in something that isn't a register");
            }
        }


    }
}
