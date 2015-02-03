using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class Instruction {
        public Result myResult;
        public int instructionNum { private set;  get; }
        public Instruction[] neededInstr; // instructions whose values this instruction needs
        public Instruction next;
        public Instruction prev;
        public BoundingBlock myBoundingBlock{private set; get;}
        public string actualInstruction; // i.e mul x_1 #3

        public Instruction(int instructionNumber, BoundingBlock myBB){
            neededInstr = new Instruction[2];

        }

        public Instruction(int instructionNumber, BoundingBlock myBB, string instructionText) {
            neededInstr = new Instruction[2];
            actualInstruction = instructionText;

        }

        public void AddToBoundingBlock(BoundingBlock bb) {
            myBoundingBlock = bb;
        }

        public override string ToString() {
            return String.Format("{0}: {1}", instructionNum, actualInstruction);
        }



    }
}
