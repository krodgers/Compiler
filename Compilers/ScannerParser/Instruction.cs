using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public class Instruction {
        public enum OperandType {SSA_VAL, CONSTANT, REG, VAR, IDENT, BRANCH, LABEL, ERROR}
        public Result myResult;
        public int instructionNum { private set;  get; }
        public Instruction[] neededInstr; // instructions whose values this instruction needs
        public Instruction next;
        public Instruction prev;
        public BasicBlock myBasicBlock{private set; get;}
        public Token opCode;
        public string firstOperand;
        public OperandType? firstOperandType;
        public string secondOperand;
        public OperandType? secondOperandType;
        public int firstOperandSSAVal;
        public int secondOperandSSAVal;


        public Instruction(int instructionNumber, BasicBlock myBB){
            instructionNum = instructionNumber;
            myBasicBlock = myBB;
            neededInstr = new Instruction[2];
            next = null;
            prev = null;
            secondOperandType = null;

        }

        public Instruction(int instructionNumber, BasicBlock myBB, string opCode, string firstOperand, string secondOperand) {
            neededInstr = new Instruction[2];
            this.opCode = opCode;
            this.firstOperand = firstOperand;
            this.secondOperand = secondOperand;
            instructionNum = instructionNumber;
            myBasicBlock = myBB;
            secondOperandType = null;

        }

        public void AddToBoundingBlock(BasicBlock bb) {
            myBasicBlock = bb;
        }

        public override string ToString() {
            return String.Format("{0}: {1} {2} {3}", instructionNum, opCode, firstOperand, secondOperand);
        }


    }
}
