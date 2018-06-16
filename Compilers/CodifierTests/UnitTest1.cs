using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ScannerParser;

namespace CodifierTests {
    [TestClass]
    public class UnitTest1 {
        private InstructionManager im;
        private BasicBlock bb0;
        private Codifier coder;
       
        [TestInitialize]
        private void SetUp() {
            bb0 = new BasicBlock(0);
            im = new InstructionManager(null); // just don't add PHis!
            im.setCurrentBlock(bb0);
            int numlines = 50;
            coder = new Codifier("../../test_assem.txt", numlines);
            string filename = @"../../test_output.txt";
            Utilities.OpenStreams(filename);
        }
        [TestCleanup]
        private void TearDown() {
            coder.CloseFiles();
            if (SSAWriter.sw != null)
                SSAWriter.sw.Dispose();

        }

        [TestMethod]
        public void TestBranchCompareInstructions() {
            // open files and junk
            SetUp();

            // Make a basic block 
            Result a = new Result(Kind.VAR, "a");
            Result b = new Result(Kind.VAR, "b");
            Result one = new Result(Kind.CONST, 1);
            Result two = new Result(Kind.CONST, 2);
            Result regOne = new Result(Kind.REG, "(1)");
            Result regTwo = new Result(Kind.REG, "(2)");
            Result regThree = new Result(Kind.REG, "(3)");
           


            int lineNum = 1;
            // a = 1; b = 2;
            im.PutBasicInstruction(Token.BECOMES, one, a, lineNum++);
            im.PutBasicInstruction(Token.BECOMES, two, b, lineNum++);
            // if  a > b      
            im.PutCompare(Token.GTR, regOne, regTwo, lineNum++);
            im.PutConditionalBranch(Token.LEQ, regThree, String.Format("{0}", 3), lineNum++);
            // true - a = a + 2
            im.PutBasicInstruction(Token.PLUS, regOne, two, lineNum++);
            Result a_2 = new Result(Kind.REG, String.Format("({0})", lineNum-1));
            im.PutUnconditionalBranch(Token.EQL, 2, lineNum++);
            // false
            im.PutBasicInstruction(Token.MINUS, a_2, two, lineNum++);
            Result regFour = new Result(Kind.REG, String.Format("({0})", lineNum-1));

            // outputnum(a)
            im.PutBasicInstruction(Token.OUTPUTNUM, a_2, new Result(Kind.CONST, ""), lineNum++);
            // outputnum(b)
            im.PutBasicInstruction(Token.OUTPUTNUM, regFour, new Result(Kind.CONST, ""), lineNum++);

            // Return
            im.PutProcedureReturn(lineNum++);

            SSAWriter.WriteBlock(bb0);

            SSAWriter.sw.Dispose();


            // codify it
            coder.CodifyBlock(bb0);
            // make sure output matches


            // tear down
            coder.CloseFiles();
//            TearDown();
        }
    }
}

