using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class BasicBlock {
        public int blockNum { private set; get; } // which block this is
        public Instruction firstInstruction;// 
        public BasicBlock dominatingBlock; // 
        public BasicBlock nextBlock;  // block(s) with the instructions that follow this block's


        public BasicBlock(int myNumber) {
            blockNum = myNumber;

        }
       
    }
