using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScannerParser;
using System.Collections.Generic;

namespace CodifierTests {
    [TestClass]
    public class CodifierPrepTests {
        [TestMethod]
        public void TestReconstructResult() {
            
            PrivateType prepper = new PrivateType(typeof(CodifierPrep));

            BasicBlock parent = new BasicBlock(0);
//            InstructionManager im = new InstructionManager();
            //im.setCurrentBlock(parent);
            int linNum = 0;

            Result constant = new Result(Kind.CONST, (double) 32);
            constant.lineNumber = linNum++;
            Result stringConstant = new Result(Kind.CONST, "str");
            stringConstant.lineNumber = linNum++;
            Result var = new Result(Kind.VAR, "x");
            var.lineNumber = linNum++;
            Result reg = new Result(Kind.REG, "$FP");
            reg.lineNumber = linNum++;
            Result arr = new Result(Kind.ARR, "array", null);
            arr.lineNumber = linNum++;
            Result branch = new Result(Kind.BRA, "BranchLabel");
            branch.lineNumber = linNum++;
       //     Result condition = new Result(Kind.COND, CondOp.LEQ, linNum++);

            Result[] alltheresults = { constant, stringConstant, var, reg, arr, branch };

            int instrNum = 0;

            foreach (Result r in alltheresults) {

                object[] args = { prepper.InvokeStatic("KindToOperandType", new object[] { r }), r.GetValue(), r.lineNumber };
                Result res = (Result) prepper.InvokeStatic("ReconstructResult", args);
                Assert.AreEqual(r.type, res.type, String.Format("Types don't match : {0} {1}", r.type, res.type));
                Assert.AreEqual(r.lineNumber, res.lineNumber, String.Format("Line Numbers don't match: {0} {1}", r.lineNumber, res.lineNumber));
                Assert.AreEqual(r.GetValue(), res.GetValue(), String.Format("Values differ {0} {1}", r.GetValue(), res.GetValue()));
                Assert.AreEqual(r.condition, res.condition, String.Format("Conditions differ {0} {1}", r.condition, res.condition));
                Assert.AreEqual(r.constantType, res.constantType, String.Format("Constant types differ {0} {1}", r.constantType, res.constantType));

            }


            //// Test Move instructions
            //foreach (Result resA in alltheresults) {
            //    foreach (Result resB in alltheresults) {
            //        im.PutBasicInstruction(Token.BECOMES, resA, resB, instrNum);
            //        instrNum++;
            //    }
            //}
            //BasicBlock result = (BasicBlock) prepper.InvokeStatic("ReconstructResult");
            //Instruction currInstr = result.firstInstruction;
            //while (currInstr != null) {

            //    currInstr = currInstr.next;

            //}
        }

        [TestMethod]
        public void TestPropagate() {
            Parser p = new Parser(@"testFiles/test017.txt");
            BasicBlock start = p.StartFirstPass();
            List<Symbol> symtable = p.ExportSymbolTable();



        }
    }
}
