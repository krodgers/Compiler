﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace ScannerParser {
    // Contains methods to write out DLX processor code

    class Codifier {
        public enum REG { R0 = 0, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12, 
            R13, R14, R15, R16, R17, R18, R19, R20, R21, R22, R23, R24, R25, R26, R27, 
            FP = 28, SP = 29, RGLOBALS = 30, RA = 31 , ERROR};


        private StreamWriter sw;
        private int AssemblyPC, currRegister;
        private Dictionary<int,REG> ssaToReg; // maps ssa values to registers
        private Dictionary<int, REG> varToReg; // maps vars to registers
        private List<REG> availableRegs;
        private Dictionary<string, int> symbols; // maps symbol string to ssaVals

        // TODO:: What thing should be passed in?  Reference to CFG?
        public Codifier(string outputFileName) {
            sw = new StreamWriter(new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write));
            sw.AutoFlush = true;
            init();

        }
        

        private void init() {
            ssaToReg = new Dictionary<int, REG>();
            varToReg = new Dictionary<int, REG>();
            availableRegs = new List<REG>();
            foreach (REG r in Enum.GetValues(typeof(REG))) {
                if (r != REG.R0 && r != REG.FP && r != REG.SP && r != REG.RGLOBALS && r != REG.RA && r != REG.ERROR)
                    availableRegs.Add(r);
            }
            symbols = new Dictionary<string, int>();
        }
        public void CloseFiles() {
            sw.Flush();
            if (sw != null)
                sw.Close();
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

        public void CodifyBlock(BasicBlock bb) {
            init(); 
            Instruction currInstr = bb.firstInstruction;
            while (currInstr != null) {
                WriteInstruction(currInstr.opCode, currInstr);
                currInstr = currInstr.next;
            }
        }
        
        ////////////////////////////////////
        //  General Instructions	      //
        ////////////////////////////////////
        public void WriteInstruction(Token opCode, Instruction currInstr) {

            switch (opCode) {
                // math instructions
                case Token.TIMES:
                case Token.DIV:
                case Token.PLUS:
                case Token.MINUS:
                    HandleMathInstruction(opCode, currInstr);
                    break;
                // Memory Instructions
                case Token.BECOMES:
                case Token.STORE:
                case Token.LOAD:
                    HandleMemoryInstruction(opCode, currInstr);
                    break;
                // Function Instruction
                case Token.CALL:
                case Token.END:
                case Token.RETURN:
                case Token.INPUTNUM:
                case Token.OUTPUTNUM:
                case Token.OUTPUTNEWLINE:
                    HandleFunctionInstruction(opCode, currInstr);
                    break;
                // Branching Instruction
                case Token.BRANCH:
                // Comparing Instruction
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                    if (currInstr.secondOperandType == Instruction.OperandType.BRANCH)
                        HandleBranchInstruction(opCode, currInstr);
                    else
                        HandleCompareInstruction(opCode, currInstr);
                    break;
                default:
                    Error(String.Format("Unable to classify opcode {0}", opCode));
                    break;
            }

        }


        private void HandleMathInstruction(Token opCode, Instruction currInstr) {
            int opB, opC;
            REG bVal;
            int cVal;
            REG storageReg = GetAvailableReg(); // TODO:: This needs to be fixed.  but while we don't have register allocation...
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
            
            PutMathInstruction(TokenToInstruction(opCode), storageReg, bVal, cVal, isImmediate);
        }

        // puts opcode A B C
        // operandC has to be an int in case it's an immediate
        private void PutMathInstruction(string opCode, REG whereToStore, REG operandB, int operandC, bool isImmediate) {
            if (isImmediate) 
                Write(String.Format("{0}i {1} {2} {3}", opCode, (int)whereToStore, (int)operandB, operandC));
             else 
                Write(String.Format("{0} {1} {2} {3}", opCode, (int)whereToStore, (int)operandB, operandC));

            
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
                    Write("ret 0");
                    break;
                case Token.RETURN:
                    FunctionExitCode(currInstr);
                    break;
                case Token.OUTPUTNEWLINE:
                    Write("wrl");
                    break;
                case Token.OUTPUTNUM:
                    Write("wrd " + (int)GetRegister(currInstr.firstOperandSSAVal));
                    break;
                case Token.INPUTNUM:
                    Write("rdd " + (int)GetRegister(currInstr.firstOperandSSAVal));
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
        private void HandleMemoryInstruction(Token opCode, Instruction currInstr) {
            int opA, opB;
            if (opCode == Token.BECOMES) {
                // assign the SSAval a register
                // TODO:: virtual registers
                REG newReg = GetAvailableReg();
                AssignRegToSsaVal(newReg, currInstr.instructionNum);

                if (CheckImmediateAndReorder(currInstr, out opA, out opB)) {
                    PutMathInstruction("add", newReg, REG.R0, opB, true);
                } else {
                    // will return the register associated with this value
                    if (opB == 0) {
                        // has no SSAValue yet
                        if (currInstr.secondOperandType == Instruction.OperandType.VAR) {
                            REG variReg = GetAvailableReg();
                            AssignRegToSsaVal(variReg, currInstr.instructionNum);
                            AssignSSAToVar(currInstr.instructionNum, currInstr.secondOperand);
                            PutMathInstruction("add", variReg, REG.R0, (int)GetRegister(opA), false);
                        }
                    }
               
                   
                }

            } else {
            }

        }

        //////////////////////////////////////////////
        // Branching/Comparing  Instructions	    //
        //////////////////////////////////////////////
        private void HandleBranchInstruction(Token opCode, Instruction currInstr) {
            
        }


        private void HandleCompareInstruction(Token opCode, Instruction currInstr) {
            throw new NotImplementedException();
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


        private void Write(string toWrite) {
            sw.WriteLine(toWrite);
            Console.WriteLine(toWrite);
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
        /// </summary>
        /// <param name="instr"> The crrent instruction</param>
        /// <param name="opAValue"> out param: returns the SSA value of the operand</param>
        /// <param name="opBValue">out param: returns the value of the immediate operand</param>
        /// <returns>True if has a constant operand, false else</returns>
        private bool CheckImmediateAndReorder(Instruction instr, out int opAValue, out int opBValue) {
            bool immediate = false;

            // Can't have both be constants
            if (instr.firstOperandType == Instruction.OperandType.CONSTANT && instr.secondOperandType == Instruction.OperandType.CONSTANT) {
                Error("Both operands cannot be constants");
            }


            if (instr.firstOperandType == Instruction.OperandType.CONSTANT) {
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

        // Returns the instruction associated with the token
        private string TokenToInstruction(Token t) {
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
                case Token.LOAD:
                    opString = "LD";
                    break;
                case Token.STORE:
                    opString = "ST";
                    break;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                    opString = "cmp";
                    break;
                default:
                    Error(String.Format("Unknown Token {0} -- can't find instruction equivalent", t));
                    opString = String.Empty; // to appease the compiler....
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