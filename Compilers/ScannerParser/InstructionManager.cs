using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class InstructionManager {
        private Dictionary<int, Instruction> instructionDictionary;
        private BasicBlock curBasicBlock;

        public InstructionManager() {
            instructionDictionary = new Dictionary<int, Instruction>();
        }

        public void setCurrentBlock(BasicBlock current) {
            curBasicBlock = current;
        }

        public BasicBlock getCurrentBlock() {
            return curBasicBlock;
        }

        public void PutBasicInstruction(Token opCode, Result a, Result b, int lineNumber) {


            // Initialize all of the non-pointer fields for the instruction
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = opCode;

            tmp.firstOperand = String.Format("{0}", a.GetValue());
            if (a.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(a.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(a);
            }

            tmp.secondOperand = String.Format("{0}", b.GetValue());
            if (b.GetValue().Contains("(")) {
                tmp.secondOperandSSAVal = GetNumFromSSAReg(b.GetValue());
                tmp.secondOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.secondOperandType = KindToOperandType(b);
            }

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);

        }

        public void PutLoadInstruction(Result itemToLoad, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.LOAD;

            tmp.firstOperand = itemToLoad.GetValue();
            if (itemToLoad.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(itemToLoad.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(itemToLoad);
            }


            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutProcedureReturn(int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.RETURN;

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutFunctionReturn(Result res, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.RETURN;

            tmp.firstOperand = res.GetValue();
            if (res.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(res.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(res);
            }

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutFunctionArgument(Result argument, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.MINUS;

            tmp.firstOperand = "#4";
            tmp.firstOperandType = Instruction.OperandType.CONSTANT;

            tmp.secondOperand = "$SP";
            tmp.secondOperandType = Instruction.OperandType.REG;

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);

            PutBasicInstruction(Token.STORE, argument, new Result(Kind.REG, String.Format("({0})", lineNumber)), lineNumber + 1);

        }

        public void PutFunctionEntry(Result function, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.BRANCH;

            tmp.firstOperand = function.GetValue().ToUpper();
            tmp.firstOperandType = KindToOperandType(function);

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutFunctionLeave(int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.BRANCH;

            tmp.firstOperand = "$RA";
            tmp.firstOperandType = Instruction.OperandType.REG; 

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutConditionalBranch(Token opCode, Result condResult, string label, int lineNumber)
        {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = opCode;

            tmp.firstOperand = condResult.GetValue();
            if (condResult.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(condResult.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(condResult);
            }

            tmp.secondOperand = label;
            tmp.secondOperandType = Instruction.OperandType.LABEL;

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        private int GetNumFromSSAReg(string p) {
            return (int)Decimal.Parse(p, NumberStyles.AllowParentheses) * -1;
        }

        private Instruction.OperandType KindToOperandType(Result res) {
            switch (res.type) {
                case Kind.REG:
                    return Instruction.OperandType.REG;
                case Kind.VAR:
                    return Instruction.OperandType.VAR;
                case Kind.CONST:
                    return Instruction.OperandType.CONSTANT;
                case Kind.BRA:
                    return Instruction.OperandType.BRANCH;
                default:
                    Debug.WriteLine("Should not have gotten here");
                    return Instruction.OperandType.ERROR;
            }
        }

        private void InsertAndLink(Instruction curInstruction) {
            if (curBasicBlock.firstInstruction == null) {
                curBasicBlock.firstInstruction = curInstruction;
            }
            else {
                Instruction tmp = curBasicBlock.firstInstruction;
                while (tmp.next != null)
                    tmp = tmp.next;
                tmp.next = curInstruction;
                curInstruction.prev = tmp;
            }

            if (curInstruction.firstOperandType == Instruction.OperandType.SSA_VAL)
                curInstruction.neededInstr[0] = instructionDictionary[curInstruction.firstOperandSSAVal];

            if (curInstruction.secondOperandType == Instruction.OperandType.SSA_VAL)
                curInstruction.neededInstr[1] = instructionDictionary[curInstruction.secondOperandSSAVal];
        }
    }
}
