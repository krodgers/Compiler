using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public class BasicBlock {
        public enum BlockType {ENTRY, STANDARD, TRUE, FALSE, JOIN, LOOP_HEADER, EXIT}
        public int blockNum { set; get; } // which block this is
        public string blockLabel;
        public Instruction firstInstruction;// 
        public BasicBlock dominatingBlock; // 
        public List<BasicBlock> parentBlocks;
        public List<BasicBlock> childBlocks;  // block(s) with the instructions that follow this block's
        public int nestingLevel;
        public BlockType blockType;


        public BasicBlock(int myNumber) {
            blockNum = myNumber;
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
