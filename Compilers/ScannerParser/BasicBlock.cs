using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            init();
            blockNum = myNumber;
        }

        public BasicBlock(int myNumber, int myScope) {
            init();
            blockNum = myNumber;
            scopeNumber = myScope;
        }

        // Sets the block label based on the special type
        public BasicBlock(int myNumber, int myScope, BlockType mySpecialType) {
            init();
            blockNum = myNumber;
            scopeNumber = myScope;
            blockType = mySpecialType;
            blockLabel = GetLabel(mySpecialType, myNumber);
        }

        // Intializes all of the things to default values
        // blocktype gets set to standard
        private void init() {
            blockLabel = String.Empty;
            firstInstruction = null;
            instructionCount = 0;
            joinPredecessorInstructionCount = 0;
            dominatingBlock = null;
            blocksIDominate = new List<BasicBlock>();
            parentBlocks = new List<BasicBlock>(); ;
            childBlocks = new List<BasicBlock>(); ;  // block(s) with the instructions that follow this block's
            nestingLevel = 0;
            scopeNumber = 0;
            blockType = BlockType.STANDARD;
            phiInstructions = new Dictionary<int, PhiInstruction>();

        }


        public void setDominatingInformation(BasicBlock myDominatingBlock) {
            dominatingBlock = myDominatingBlock;
        }

        public void setDominatingInformation(BasicBlock myDominatingBlock, int myNestingLevel) {
            dominatingBlock = myDominatingBlock;
            nestingLevel = myNestingLevel;
        }


        private string GetLabel(BasicBlock.BlockType type, int blockNum) {
            switch (type) {
                case BasicBlock.BlockType.TRUE:
                    return "TRUE_" + blockNum + ":";
                case BasicBlock.BlockType.FALSE:
                    return "FALSE_" + blockNum + ":";
                case BasicBlock.BlockType.JOIN:
                    return "JOIN_" + blockNum + ":";
                case BlockType.LOOP_HEADER:
                    return "LOOP_HEADER_" + blockNum + ":";
                case BlockType.LOOP_BODY:
                    return "LOOP_BODY_" + blockNum + ":";
                case BlockType.ENTRY:
                    return "ENTRY";
                case BlockType.EXIT:
                    return "EXIT";
                case BlockType.MAIN_ENTRY:
                    return "MAIN:";
                default:
                    return "block_" + blockNum + ":";
            }
        }



        public override string ToString() {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}", blockLabel);
            builder.AppendLine();
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
