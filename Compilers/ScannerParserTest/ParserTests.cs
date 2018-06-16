using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ScannerParser;

namespace ScannerParserTest {
    [TestClass]
    public class ParserTests {
        [TestMethod]
        
        public void TestAddingToSymbolTable001() {
            Parser target = new Parser("test001.txt");
            PrivateObject testParser = new PrivateObject(target);
            
            target.StartFirstPass();

            //var retVal = testParser.Invoke("PrivateMethod");
            
            // should have a 1 scopes, for main 
             
            Stack<int> scopes = (Stack<int>)testParser.GetField("scopes");
            Assert.AreEqual(1, scopes.Count, "Too many Scopes found");

            // all variables should be in global scope
            List<Symbol> symbs = (List<Symbol>) testParser.GetField("symbolTable");
            
            foreach (Symbol s in symbs) {
                Assert.IsTrue(s.IsGlobal(), "Symbol not global");
                Assert.IsTrue(s.IsInScope(scopes.Peek()), "Symbol not in scope");

            }


        }
    

    [TestMethod]
     public void TestSymbolTableWithFunctions_factorial() {
            Parser target = new Parser("factorial.txt");
            PrivateObject testParser = new PrivateObject(target);
            
            target.StartFirstPass();

            //var retVal = testParser.Invoke("PrivateMethod");
            
            // should have a 3 scopes 
            int scopes = (int)testParser.GetField("nextScopeNumber");
            Assert.AreEqual(4, scopes, "Too many Scopes found");

            // all variables should be in correct scope
            List<Symbol> symbs = (List<Symbol>) testParser.GetField("symbolTable");
            Scanner scanner = (Scanner)testParser.GetField("scanner");

        // Main scope variables
        Assert.IsTrue(symbs[scanner.String2Id("input")].IsInScope(1));

        // factIter scope variables
        Assert.IsTrue(symbs[scanner.String2Id("factIter")].IsInScope(1));
        Assert.IsTrue(symbs[scanner.String2Id("factIter")].IsInScope(2));
        Assert.IsTrue(symbs[scanner.String2Id("i")].IsInScope(2));
        Assert.IsTrue(symbs[scanner.String2Id("f")].IsInScope(2));
        Assert.IsTrue(symbs[scanner.String2Id("n")].IsInScope(2));

            // factRec variables
        Assert.IsTrue(symbs[scanner.String2Id("factRec")].IsInScope(1));
        Assert.IsTrue(symbs[scanner.String2Id("factRec")].IsInScope(3));
        Assert.IsTrue(symbs[scanner.String2Id("n")].IsInScope(3));


        }
    }
}
