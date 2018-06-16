using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScannerParser;
using System.Collections.Generic;

namespace BoundingBlockStructureTests {
    [TestClass]
    public class TraversingTests {
        [TestMethod]
        public void SimpleFlowTest() {
            BasicBlock start = MakeBlock(0, BasicBlock.BlockType.ENTRY);
            BasicBlock main = MakeBlock(1, BasicBlock.BlockType.MAIN_ENTRY);
            BasicBlock std = MakeBlock(2, BasicBlock.BlockType.STANDARD);
            BasicBlock std2 = MakeBlock(3, BasicBlock.BlockType.STANDARD);
            BasicBlock std3= MakeBlock(4, BasicBlock.BlockType.STANDARD);
            BasicBlock exit = MakeBlock(5, BasicBlock.BlockType.EXIT);

            BasicBlock[] order = new BasicBlock[] { start, main, std, std2, std3, exit};
            for (int i = 0; i < order.Length - 1; i++) {
                order[i].childBlocks = new List<BasicBlock>();
                order[i].childBlocks.Add(order[i + 1]);
            }

            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);

            CheckLists(order, res.ToArray());
        }


[TestMethod]
        public void FlowTestIf() {
            BasicBlock start = MakeBlock(0, BasicBlock.BlockType.ENTRY);
            BasicBlock main = MakeBlock(1, BasicBlock.BlockType.MAIN_ENTRY);
            BasicBlock troo = MakeBlock(2, BasicBlock.BlockType.TRUE);
            BasicBlock fls = MakeBlock(3, BasicBlock.BlockType.FALSE);
            BasicBlock jn = MakeBlock(4, BasicBlock.BlockType.JOIN);
            BasicBlock exit = MakeBlock(5, BasicBlock.BlockType.EXIT);

            BasicBlock[] trueOrder = new BasicBlock[] { start, main, troo, jn, exit };
            LinkBlocks(ref trueOrder);
            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);
            CheckLists(trueOrder, res.ToArray());

            BasicBlock[] trueFalseOrder = new BasicBlock[] { start, main, troo, fls, jn, exit };
            start.childBlocks = new List<BasicBlock>() { main };
            main.childBlocks = new List<BasicBlock>() { troo, fls };
            troo.childBlocks = new List<BasicBlock>() { jn };
            fls.childBlocks = new List<BasicBlock>() { jn };
            jn.childBlocks = new List<BasicBlock>() { exit };
            res = Utilities.TraverseCFG(ref start);
            CheckLists(trueFalseOrder, res.ToArray());




            BasicBlock[] jnOrder = new BasicBlock[] { start, main, fls, jn, exit };
            LinkBlocks(ref jnOrder);
            res = Utilities.TraverseCFG(ref start);
            CheckLists(jnOrder, res.ToArray());
  

        }

[TestMethod]
        public void FlowTestWhile() {
            BasicBlock start = MakeBlock(0, BasicBlock.BlockType.ENTRY);
            BasicBlock main = MakeBlock(1, BasicBlock.BlockType.MAIN_ENTRY);
            BasicBlock head = MakeBlock(2, BasicBlock.BlockType.LOOP_HEADER);
            BasicBlock body = MakeBlock(3, BasicBlock.BlockType.LOOP_BODY);
            BasicBlock follow = MakeBlock(4, BasicBlock.BlockType.FOLLOW);
            BasicBlock exit = MakeBlock(5, BasicBlock.BlockType.EXIT);

            BasicBlock[] trueOrder = new BasicBlock[] { start, main, head, body, follow, exit };
            start.childBlocks = new List<BasicBlock>() { main };
            main.childBlocks = new List<BasicBlock>() { head};
            head.childBlocks = new List<BasicBlock>() { body, follow };
            body.childBlocks = new List<BasicBlock> { head };
            follow.childBlocks = new List<BasicBlock>() { exit };

            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);
            CheckLists(trueOrder, res.ToArray());
          

        }
[TestMethod]
        public void FlowTestWhileIf() {
            BasicBlock start = MakeBlock(0, BasicBlock.BlockType.ENTRY);
            BasicBlock main = MakeBlock(1, BasicBlock.BlockType.MAIN_ENTRY);
            BasicBlock std = MakeBlock(2, BasicBlock.BlockType.STANDARD);
            BasicBlock head = MakeBlock(2, BasicBlock.BlockType.LOOP_HEADER);
            BasicBlock body = MakeBlock(3, BasicBlock.BlockType.LOOP_BODY);
            BasicBlock follow = MakeBlock(4, BasicBlock.BlockType.FOLLOW);
            BasicBlock troo = MakeBlock(2, BasicBlock.BlockType.TRUE);
            BasicBlock fls = MakeBlock(3, BasicBlock.BlockType.FALSE);
            BasicBlock jn = MakeBlock(4, BasicBlock.BlockType.JOIN);

            BasicBlock exit = MakeBlock(5, BasicBlock.BlockType.EXIT);

            BasicBlock[] trueOrder = new BasicBlock[] { start, main, std,head, body, troo, fls, jn, follow, exit };
            start.childBlocks = new List<BasicBlock>() { main };
            main.childBlocks = new List<BasicBlock>() { std};
            std.childBlocks = new List<BasicBlock>() { head };
            head.childBlocks = new List<BasicBlock>() { body, follow };
            body.childBlocks = new List<BasicBlock>() { troo, fls };
            troo.childBlocks = new List<BasicBlock>() { jn };
            fls.childBlocks = new List<BasicBlock>() { jn };
            jn.childBlocks = new List<BasicBlock>() { head };
            follow.childBlocks = new List<BasicBlock>() { exit };
            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);
            CheckLists(trueOrder, res.ToArray());
          

        }

        [TestMethod]
        public void FlowFunctionTest() {
            BasicBlock start = MakeBlock(0, BasicBlock.BlockType.ENTRY);
            BasicBlock main = MakeBlock(1, BasicBlock.BlockType.MAIN_ENTRY);
            BasicBlock std = MakeBlock(2, BasicBlock.BlockType.STANDARD);
            BasicBlock call = MakeBlock(3, BasicBlock.BlockType.FUNCTION_CALL);
            BasicBlock func= MakeBlock(4, BasicBlock.BlockType.FUNCTION_HEADER);
            BasicBlock exit = MakeBlock(5, BasicBlock.BlockType.EXIT);

            BasicBlock[] order = new BasicBlock[] { start, main, std, call, exit, func};
            for (int i = 0; i < order.Length - 1; i++) {
                order[i].childBlocks = new List<BasicBlock>();
                order[i].childBlocks.Add(order[i + 1]);
            }

            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);

            CheckLists(order, res.ToArray());
        }

        [TestMethod]
        public void FlowTestComplex() {
            BasicBlock start = MakeBlock(0, BasicBlock.BlockType.ENTRY);
            BasicBlock main = MakeBlock(1, BasicBlock.BlockType.MAIN_ENTRY);
            BasicBlock std = MakeBlock(2, BasicBlock.BlockType.STANDARD);
            BasicBlock call = MakeBlock(3, BasicBlock.BlockType.FUNCTION_CALL);
            BasicBlock func= MakeBlock(4, BasicBlock.BlockType.FUNCTION_HEADER);
            BasicBlock exit = MakeBlock(5, BasicBlock.BlockType.EXIT);
            BasicBlock head = MakeBlock(6, BasicBlock.BlockType.LOOP_HEADER);
            BasicBlock body = MakeBlock(7, BasicBlock.BlockType.LOOP_BODY);
            BasicBlock follow = MakeBlock(8, BasicBlock.BlockType.FOLLOW);
            BasicBlock head2 = MakeBlock(9, BasicBlock.BlockType.LOOP_HEADER);
            BasicBlock body2 = MakeBlock(10, BasicBlock.BlockType.LOOP_BODY);
            BasicBlock follow2 = MakeBlock(11, BasicBlock.BlockType.FOLLOW);
            BasicBlock troo = MakeBlock(12, BasicBlock.BlockType.TRUE);
            BasicBlock fls = MakeBlock(13, BasicBlock.BlockType.FALSE);
            BasicBlock jn = MakeBlock(14, BasicBlock.BlockType.JOIN);
            BasicBlock troo2 = MakeBlock(15, BasicBlock.BlockType.TRUE);
            BasicBlock fls2 = MakeBlock(16, BasicBlock.BlockType.FALSE);
            BasicBlock jn2 = MakeBlock(17, BasicBlock.BlockType.JOIN);

            BasicBlock[] order = new BasicBlock[] { start, main, std, head, body, troo, fls, jn, troo2, fls2, jn2, head2, body2, follow2, follow, exit };

            start.childBlocks = new List<BasicBlock>() { main };
            main.childBlocks = new List<BasicBlock>() { std };
            std.childBlocks = new List<BasicBlock>() { head };
            head.childBlocks = new List<BasicBlock>() { body, follow };
            body.childBlocks = new List<BasicBlock>() { troo, fls };
            troo.childBlocks = new List<BasicBlock>() { jn };
            fls.childBlocks = new List<BasicBlock>() { jn };
            jn.childBlocks = new List<BasicBlock>() { troo2, fls2 };
            troo2.childBlocks = new List<BasicBlock>() { jn2 };
            fls2.childBlocks = new List<BasicBlock>() { jn2 };
            jn2.childBlocks = new List<BasicBlock>() { head2 };
            head2.childBlocks = new List<BasicBlock>() { body2, follow2 };
            follow2.childBlocks = new List<BasicBlock>() { head };
            follow.childBlocks = new List<BasicBlock>() { exit };

            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);
            CheckLists(order, res.ToArray());
        }



private void LinkBlocks(ref BasicBlock[] order) {
           for (int i = 0; i < order.Length - 1; i++) {
                order[i].childBlocks = new List<BasicBlock>();
                order[i].childBlocks.Add(order[i + 1]);
            }

}


        private void CheckLists(BasicBlock[]  expected, BasicBlock[]  actual) {
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].blockType, actual[i].blockType, String.Format("i: {0}", i));
            }
        }
        private BasicBlock MakeBlock(int num, BasicBlock.BlockType type) {
            BasicBlock b = new BasicBlock(num);
            b.blockType = type;
            return b;
        }
    }
}
