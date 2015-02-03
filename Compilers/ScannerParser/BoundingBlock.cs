using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class BoundingBlock {
        public int blockNum { private set; get; } // which block this is
        public Instruction firstInstruction;// 
        public BoundingBlock dominatingBlock; // 
        public BoundingBlock nextBlock;  // block(s) with the instructions that follow this block's


        public BoundingBlock(int myNumber) {
            blockNum = myNumber;

        }

    }
}
