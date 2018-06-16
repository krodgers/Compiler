using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScannerParser;

namespace CodifierTests {
    class PrepTest017 {

        public static void Main() {


            // Make the block...
            int pc = 1;
            Result a = new Result(Kind.VAR, "a");
            Result b = new Result(Kind.VAR, "b");
            Result c = new Result(Kind.VAR, "c");
            Result d = new Result(Kind.VAR, "d");
            Result e = new Result(Kind.VAR, "e");
            Result one = new Result(Kind.CONST, ConstantType.DOUBLE, 1);
            Result two = new Result(Kind.CONST, ConstantType.DOUBLE, 2);
            Result four = new Result(Kind.CONST, ConstantType.DOUBLE, 4);
            Result zero = new Result(Kind.CONST, ConstantType.DOUBLE, 0);

            Result regOne = new Result(Kind.REG, String.Format("({0})", 1));
            Result regTwo = new Result(Kind.REG, String.Format("({0})", 2));
            Result regThree = new Result(Kind.REG, String.Format("({0})", 3));
            Result regFour = new Result(Kind.REG, String.Format("({0})", 4));
            Result regFive = new Result(Kind.REG, String.Format("({0})", 5));
            Result regSeven = new Result(Kind.REG, String.Format("({0})", 7));
            Result regNine= new Result(Kind.REG, String.Format("({0})", 9));


            BasicBlock start = MakeBlock(0, BasicBlock.BlockType.ENTRY);
            BasicBlock main = MakeBlock(1, BasicBlock.BlockType.MAIN_ENTRY);
            BasicBlock troo = MakeBlock(2, BasicBlock.BlockType.TRUE);
            BasicBlock fls = MakeBlock(3, BasicBlock.BlockType.FALSE);
            BasicBlock jn = MakeBlock(4, BasicBlock.BlockType.JOIN);
            BasicBlock exit = MakeBlock(5, BasicBlock.BlockType.EXIT);

            InstructionManager im = new InstructionManager(null);
            im.setCurrentBlock(main);

            Utilities.PutInstruction(Token.BECOMES, one, a, pc++, ref im);
                        im.GetInstruction(pc-1).myResult = regOne;

            Utilities.PutInstruction(Token.BECOMES, regOne, b, pc++, ref im);
                        im.GetInstruction(pc-1).myResult = regOne;
            Utilities.PutInstruction(Token.TIMES, four, regTwo, pc++, ref im);
                        im.GetInstruction(pc-1).myResult = regThree;

            Utilities.PutInstruction(Token.BECOMES, regThree, c, pc++, ref im);
            im.PutCompare(Token.GTR, regFour, regTwo, pc++);
            Utilities.PutInstruction(Token.BRANCH, regFive, new Result(Kind.BRA, String.Format("{0}",4)), pc++, ref im);

            im.setCurrentBlock(troo);
            Utilities.PutInstruction(Token.PLUS, two, regTwo, pc++, ref im);
                        im.GetInstruction(pc-1).myResult = regSeven;

            Utilities.PutInstruction(Token.BECOMES, regSeven, d, pc++, ref im);

            im.setCurrentBlock(jn);
            Utilities.PutInstruction(Token.PLUS, regOne, regTwo, pc++, ref im);
                        im.GetInstruction(pc-1).myResult = regNine;

            Utilities.PutInstruction(Token.BECOMES, regNine, e, pc++, ref im);

            im.setCurrentBlock(exit);
            Utilities.PutInstruction(Token.END, zero, null, pc++, ref im);

            
            BasicBlock[] trueOrder = new BasicBlock[] { start, main, troo, jn, exit };
            LinkBlocks(ref trueOrder);
            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);
            CheckLists(trueOrder, res.ToArray());
            

            Utilities.OpenStreams(@"../../output.txt");
            Console.WriteLine("Original Program: ");
            foreach (BasicBlock bl in res)
                SSAWriter.WriteBlock(bl);
            Console.WriteLine("\n\n");
            int lastLineNo;
            Queue<BasicBlock> prpped = CodifierPrep.PerformCopyPropagation(start, null, out lastLineNo);

            Codifier coder = new Codifier(@"../../assem_17.txt", lastLineNo);
            foreach (BasicBlock bl in prpped) {
                SSAWriter.WriteBlock(bl);
               
            }
            foreach (BasicBlock bl in prpped) {
                coder.CodifyBlock(bl);
            }
            coder.CloseFiles();
            SSAWriter.sw.Dispose();
            Console.ReadLine();

        }


     

private static void LinkBlocks(ref BasicBlock[] order) {
           for (int i = 0; i < order.Length - 1; i++) {
                order[i].childBlocks = new List<BasicBlock>();
                order[i].childBlocks.Add(order[i + 1]);
                order[i + 1].dominatingBlock = order[i];
            }

}


        private static void CheckLists(BasicBlock[]  expected, BasicBlock[]  actual) {
            for (int i = 0; i < expected.Length; i++) {
                System.Diagnostics.Debug.Assert(expected[i].blockType == actual[i].blockType, String.Format("i: {0}", i));
            }
        }
        private static BasicBlock MakeBlock(int num, BasicBlock.BlockType type) {
            BasicBlock b = new BasicBlock(num);
            b.blockType = type;
            return b;
        } 
    }
}
