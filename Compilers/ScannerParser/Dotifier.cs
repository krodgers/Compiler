using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class Dotifier {
        private Dictionary<int, BasicBlock> flowGraphNodes;
        private StringBuilder dotOut;
        private string mydocpath;

        public Dotifier(Dictionary<int, BasicBlock> flowGraph) {
            flowGraphNodes = flowGraph;
            dotOut = new StringBuilder();
            mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
           // mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public void WriteAllBlocksToDot(string fileName, int lastBlockNum)
        {
            dotOut.Clear();
            dotOut.AppendLine("digraph flow_graph {");
            dotOut.AppendLine("entry [shape=Msquare];");
            dotOut.AppendLine("exit [shape=Msquare];");


            foreach (KeyValuePair<int, BasicBlock> entry in flowGraphNodes) {
                // do something with entry.Value or entry.Key
                var block = entry.Value;
                var key = entry.Key;

                if (entry.Value.blockType == BasicBlock.BlockType.MAIN_ENTRY)
                {
                    dotOut.AppendLine(String.Format("{0} -> {1}", "entry", entry.Value.blockNum));
                }

                if (block.blockType != BasicBlock.BlockType.ENTRY && 
                    block.blockType != BasicBlock.BlockType.EXIT)
                {
                    dotOut.AppendLine(BlockToDot(block));
                    dotOut.AppendLine(ConnectEdges(block));
                }

            }

            dotOut.AppendLine(String.Format("{0} -> {1}", lastBlockNum, "exit"));
            dotOut.AppendLine("}");

            using (StreamWriter outfile = new StreamWriter(mydocpath + @"\" + fileName + ".dot")) {
                outfile.Write(dotOut.ToString());
            }
        }

        public void WriteDominatorTree(string fileName)
        {
            dotOut.Clear();
            dotOut.AppendLine("digraph flow_graph {");
            dotOut.AppendLine("entry [shape=Msquare];");

            foreach (KeyValuePair<int, BasicBlock> entry in flowGraphNodes) {
                // do something with entry.Value or entry.Key
                var block = entry.Value;
                var key = entry.Key;

                if (entry.Value.blockType == BasicBlock.BlockType.MAIN_ENTRY) {
                    dotOut.AppendLine(String.Format("{0} -> {1}", "entry", entry.Value.blockNum));
                }

                if (block.blockType != BasicBlock.BlockType.ENTRY &&
                    block.blockType != BasicBlock.BlockType.EXIT) {
                    dotOut.AppendLine(BlockToDot(block));
                    dotOut.AppendLine(ConnectDominatorEdges(block));
                }

            }

            dotOut.AppendLine("}");

            using (StreamWriter outfile = new StreamWriter(mydocpath + @"\" + fileName + "_dtree.dot")) {
                outfile.Write(dotOut.ToString());
            }
        }

        private string BlockToDot(BasicBlock block)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("{0} [shape=none, margin=0, label=<", block.blockNum));
            sb.AppendLine("<table  border=\"0\" cellborder=\"1\" cellspacing=\"0\" cellpadding=\"0\">");
            
            sb.AppendLine("<tr>");
            sb.AppendLine(String.Format("<td colspan=\"3\">{0}</td>", block.blockNum));
            sb.AppendLine(String.Format("<td colspan=\"3\">{0}</td>", block.blockLabel));
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr>");
            sb.AppendLine(String.Format("<td colspan=\"2\">{0}</td>", block.blockType));
            sb.AppendLine(String.Format("<td colspan=\"2\">{0}</td>", block.scopeNumber));
            sb.AppendLine(String.Format("<td colspan=\"2\">{0}</td>", block.nestingLevel));
            sb.AppendLine("</tr>");

            Instruction blockInstr = block.firstInstruction;
            while (blockInstr != null)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine(String.Format("<td colspan=\"6\">{0}</td>", blockInstr.ToString()));
                sb.AppendLine("</tr>");
                blockInstr = blockInstr.next;
            }

            sb.AppendLine("</table>");
            sb.AppendLine(">];");
            return sb.ToString();
        }


        private string ConnectDominatorEdges(BasicBlock block)
        {
            if (block.dominatingBlock != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();

                sb.AppendLine(String.Format("{0} -> {1};", block.blockNum, block.dominatingBlock.blockNum));

                sb.AppendLine();
                return sb.ToString();
            }
            return "";
        }

        private string ConnectEdges(BasicBlock block)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            for (int i = 0; i < block.childBlocks.Count; i++)
            {
                BasicBlock curChild = block.childBlocks[i];
                sb.AppendLine(String.Format("{0} -> {1};", block.blockNum, curChild.blockNum));
            }

            sb.AppendLine();
            return sb.ToString();
        }

    }
}
