using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public class BasicBlock {
        public enum BlockType {ENTRY, STANDARD, TRUE, FALSE, JOIN, LOOP_HEADER, FOLLOW, LOOP_BODY, EXIT, FUNCTION_HEADER, FUNCTION_CALL, MAIN_ENTRY}
        public int blockNum { set; get; } // which block this is
        public string blockLabel;
        public Instruction firstInstruction;// 
        public int instructionCount;
        public int joinPredecessorInstructionCount;
        public BasicBlock dominatingBlock; // 
        public List<BasicBlock> blocksIDominate; 
        public List<BasicBlock> parentBlocks;
        public List<BasicBlock> childBlocks;  // block(s) with the instructions that follow this block's
        public int nestingLevel;
        public int scopeNumber;
        public BlockType blockType;
        public Dictionary<int, PhiInstruction> phiInstructions;

        public BasicBlock(int myNumber) {
            blockNum = myNumber;
            joinPredecessorInstructionCount = 0;
            instructionCount = 0;
            phiInstructions = new Dictionary<int, PhiInstruction>();
            blocksIDominate = new List<BasicBlock>();
        }


        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Block Number: {0}", blockNum);
            builder.AppendLine();
            builder.AppendFormat("Nesting Level: {0}", nestingLevel);
            builder.AppendLine();
            builder.AppendFormat("Number of child blocks:{0}", childBlocks.Count);
            builder.AppendLine();
            foreach (BasicBlock child in childBlocks) {
                builder.Append("\t");
                builder.AppendFormat("child block num = {0}", child.blockNum);
                builder.AppendLine();
            }
            builder.AppendFormat("Number of parent blocks:{0}", parentBlocks.Count);
            builder.AppendLine();
            foreach (BasicBlock parent in parentBlocks) {
                builder.Append("\t");
                builder.AppendFormat("parent block num = {0}", parent.blockNum);
                builder.AppendLine();
            }
            return builder.ToString();                       
        }

    }
}
