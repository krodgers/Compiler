using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public class PhiInstruction : Instruction
    {
        public Result originalVarVal;
        public string targetVar;
        public int symTableID { get; set; }
        public PhiInstruction(int instructionNumber, BasicBlock myBB, Result originalVarVal, string targetVar) : base(instructionNumber, myBB)
        {
            this.originalVarVal = originalVarVal;
            this.targetVar = targetVar;
        }

        // Useful for checking whether this is a necessary phi
        // If the number of operands is only one, the phi
        // is unnecessary
        public bool IsValidPhi()
        {
            int count = 0;
            if (firstOperand != null)
                count++;
            if (secondOperand != null)
                count++;

            if (count > 1)
                return true;
            else
                return false;
        }

    }
}
