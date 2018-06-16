using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScannerParser;
using System.IO;
using System.Collections.Generic;

namespace ScannerParserTest {
    [TestClass]
    public class ScannerTest {
        [TestMethod]
        public void TokenMathTest() {
            Scanner s = new Scanner("../../../ScannerParser/TokenTestFile.txt");
            Token res;
            res = s.GetSym();
            Assert.AreEqual(Token.TIMES, res);
            res = s.GetSym();
            Assert.AreEqual(Token.DIV, res);
            res = s.GetSym();
            Assert.AreEqual(Token.PLUS, res);
            res = s.GetSym();
            Assert.AreEqual(Token.MINUS, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EOF, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EOF, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EOF, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EOF, res);


        }
        [TestMethod]
        public void TokenRelOpTest() {

            Token res;
            Scanner s = new Scanner("../../../ScannerParser/TokenTestFileRelOp.txt");
            res = s.GetSym();
            Assert.AreEqual(Token.EQL, res, "First Equal");
            res = s.GetSym();
            Assert.AreEqual(Token.NEQ, res);
            res = s.GetSym();
            Assert.AreEqual(Token.LEQ, res);
            res = s.GetSym();
            Assert.AreEqual(Token.GEQ, res);
            res = s.GetSym();
            Assert.AreEqual(Token.LSS, res);
            res = s.GetSym();
            Assert.AreEqual(Token.GTR, res);
            res = s.GetSym();
            Assert.AreEqual(Token.LEQ, res);
            res = s.GetSym();
            Assert.AreEqual(Token.NUMBER, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EQL, res);
            res = s.GetSym();
            Assert.AreEqual(Token.NUMBER, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EOF, res);

        }
        [TestMethod]
        public void TokenAssignTest() {
            Token res;
            Scanner s = new Scanner("../../../ScannerParser/TokenTestFileAssign.txt");
            res = s.GetSym();

            Assert.AreEqual(Token.BECOMES, res);
            res = s.GetSym();
            Assert.AreEqual(Token.NUMBER, res);
            res = s.GetSym();
            Assert.AreEqual(Token.BECOMES, res);
            res = s.GetSym();
            Assert.AreEqual(Token.NUMBER, res);
            res = s.GetSym();
            Assert.AreEqual(Token.IDENT, res);
            res = s.GetSym();
            Assert.AreEqual(Token.BECOMES, res);
            res = s.GetSym();
            Assert.AreEqual(Token.NUMBER, res);
            res = s.GetSym();
        }
        [TestMethod]
        public void TokenPunctTest() {
            Token res;
            Scanner s = new Scanner("../../../ScannerParser/TokenTestFilePunct.txt");

            res = s.GetSym();
            Assert.AreEqual(Token.PERIOD, res);
            res = s.GetSym();
            Assert.AreEqual(Token.COMMA, res);
            res = s.GetSym();
            Assert.AreEqual(Token.OPENBRACKET, res);
            res = s.GetSym();
            Assert.AreEqual(Token.CLOSEBRACKET, res);
            res = s.GetSym();
            Assert.AreEqual(Token.END, res);
            res = s.GetSym();
            Assert.AreEqual(Token.BEGIN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.SEMI, res);
            res = s.GetSym();
            Assert.AreEqual(Token.OPENPAREN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.CLOSEPAREN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.OPENPAREN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.OPENPAREN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.CLOSEPAREN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EOF, res);
        }
        [TestMethod]
        public void TokenKeyWordTest() {
            Token res;
            Scanner s = new Scanner("../../../ScannerParser/TokenTestFileKeyWords.txt");


            res = s.GetSym();
            Assert.AreEqual(Token.THEN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.DO, res);
            res = s.GetSym();
            Assert.AreEqual(Token.OD, res);
            res = s.GetSym();
            Assert.AreEqual(Token.FI, res);
            res = s.GetSym();
            Assert.AreEqual(Token.ELSE, res);
            res = s.GetSym();
            Assert.AreEqual(Token.LET, res);
            res = s.GetSym();
            Assert.AreEqual(Token.CALL, res);
            res = s.GetSym();
            Assert.AreEqual(Token.IF, res);
            res = s.GetSym();
            Assert.AreEqual(Token.WHILE, res);
            res = s.GetSym();
            Assert.AreEqual(Token.RETURN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.VAR, res);
            res = s.GetSym();
            Assert.AreEqual(Token.ARR, res);
            res = s.GetSym();
            Assert.AreEqual(Token.FUNC, res);
            res = s.GetSym();
            Assert.AreEqual(Token.PROC, res);
            res = s.GetSym();
            Assert.AreEqual(Token.MAIN, res);
            res = s.GetSym();
            Assert.AreEqual(Token.EOF, res);
            res = s.GetSym();

        }
        [TestMethod]
        public void LineNumberTest() {
            Token res = Token.MAIN;
            Scanner s = new Scanner("../../../ScannerParser/TokenTestFileKeyWords.txt");
            while (res != Token.EOF) {
                res = s.GetSym();
            }
            Assert.AreEqual(4, s.PC);
            
            res = Token.MAIN;
            s = new Scanner("../../../ScannerParser/TokenTestFilePunct.txt");
            while (res != Token.EOF) {
                res = s.GetSym();
            }
            Assert.AreEqual(13, s.PC);

            res = Token.MAIN;
            s = new Scanner("../../../ScannerParser/TokenTestFileComment.txt");
            while (res != Token.EOF) {
                res = s.GetSym();
            }
            Assert.AreEqual(2, s.PC, "Failed on Comment test");

            res = Token.MAIN;
            s = new Scanner("../../../ScannerParser/TokenTestFileAssign.txt");
            while (res != Token.EOF) {
                res = s.GetSym();
            }
            Assert.AreEqual(5, s.PC);

            res = Token.MAIN;
            s = new Scanner("../../../ScannerParser/TextExpression.txt");
            while (res != Token.EOF) {
                res = s.GetSym();
            }
            Assert.AreEqual(1, s.PC);

            res = Token.MAIN;
            s = new Scanner("../../../ScannerParser/test001.txt");
            while (res != Token.EOF) {
                res = s.GetSym();
            }
            Assert.AreEqual(8, s.PC);

            
        }

        [TestMethod]
        public void EachLineNumberTest() {
            Token res = Token.MAIN;
            Scanner s = new Scanner("../../../ScannerParser/test001.txt");
            int currLine = 1;
            int[] numTokens = {1,1,5,1, 5, 7, 5, 2};
            foreach( int i in numTokens){
                Assert.AreEqual(currLine, s.PC);
           
                for (int j = 0; j < i; j++) {
                    res = s.GetSym();

                }
                currLine++;
             
            }


        }

        [TestMethod]
        public void test001Test() {
            Scanner s = new Scanner("../../../ScannerParser/test001.txt");
            Token res;
            Token[] expectedTokens = {Token.MAIN, 
                                         Token.VAR, Token.IDENT, Token.COMMA, Token.IDENT, Token.SEMI,
                                         Token.BEGIN, 
                                         Token.LET, Token.IDENT, Token.BECOMES, Token.NUMBER, Token.SEMI,
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.NUMBER, Token.TIMES, Token.IDENT, Token.SEMI, 
                                         Token.CALL, Token.OUTPUTNUM, Token.OPENPAREN, Token.IDENT, Token.CLOSEPAREN,
                                         Token.END, Token.PERIOD};
            int[] linelen= {0,1,5,1,5,7,5,2};
            int idx = 0;
            
            for (int i = 0; i < linelen.Length; i ++) {
                for (int j = 0; j < linelen[i]; j++) {
                    res = s.GetSym();
                    Assert.AreEqual(expectedTokens[idx++], res);

                }
                Assert.AreEqual(i+1, s.PC);
            }
            

        }


        [TestMethod]
        public void test002Test() {
            Scanner s = new Scanner("../../../ScannerParser/testFiles/test002.txt");
            Token res;
            Token[] expectedTokens = {Token.MAIN, 
                                         Token.VAR, Token.IDENT, Token.COMMA, Token.IDENT, Token.COMMA,  Token.IDENT,  Token.SEMI,
                                         Token.ARR, Token.OPENBRACKET, Token.NUMBER, Token.CLOSEBRACKET, Token.IDENT, Token.SEMI,
                                         Token.ARR, Token.OPENBRACKET, Token.NUMBER, Token.CLOSEBRACKET, Token.OPENBRACKET, Token.NUMBER, Token.CLOSEBRACKET, Token.IDENT, Token.SEMI,
                                         Token.VAR, Token.IDENT, Token.SEMI,
                                         Token.FUNC, Token.IDENT, Token.OPENPAREN, Token.CLOSEPAREN, Token.SEMI,
                                         Token.VAR, Token.IDENT, Token.COMMA, Token.IDENT, Token.SEMI,
                                         Token.BEGIN,
                                         Token.LET, Token.IDENT, Token.BECOMES, Token.NUMBER, Token.SEMI,
                                         Token.WHILE, Token.IDENT, Token.LSS, Token.NUMBER, Token.DO,
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.IDENT, Token.PLUS, Token.NUMBER, Token.SEMI, 
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.IDENT, Token.PLUS, Token.NUMBER, Token.SEMI, 
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.IDENT, Token.PLUS, Token.IDENT, Token.SEMI, 
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.IDENT, Token.PLUS, Token.NUMBER,
                                         Token.OD, Token.SEMI,
                                         Token.RETURN, Token.IDENT,
                                         Token.END, Token.SEMI,

                                         Token.PROC, Token.IDENT, Token.OPENPAREN, Token.IDENT, Token.COMMA, Token.IDENT, Token.CLOSEPAREN, Token.SEMI,
                                         Token.VAR, Token.IDENT, Token.COMMA, Token.IDENT, Token.COMMA,  Token.IDENT,  Token.SEMI,
                                         Token.BEGIN,
                                         Token.LET, Token.IDENT, Token.BECOMES, Token.NUMBER, Token.SEMI,
                                         Token.LET, Token.IDENT, Token.BECOMES, Token.NUMBER, Token.SEMI,
                                         Token.WHILE, Token.IDENT, Token.LSS, Token.NUMBER, Token.DO,
                                         Token.WHILE, Token.IDENT, Token.LSS, Token.NUMBER, Token.DO,
                                         Token.LET, Token.IDENT, Token.OPENBRACKET, Token.IDENT, Token.CLOSEBRACKET, Token.OPENBRACKET, Token.IDENT, Token.CLOSEBRACKET, Token.BECOMES, Token.IDENT, Token.SEMI,
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.IDENT, Token.PLUS, Token.NUMBER,
                                         Token.OD, Token.SEMI,
                                         Token.LET, Token.IDENT,  Token.OPENBRACKET, Token.IDENT, Token.CLOSEBRACKET, Token.BECOMES, Token.IDENT, Token.SEMI,
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.IDENT, Token.PLUS, Token.NUMBER,
                                         Token.OD,
                                         Token.END,Token.SEMI,

                                         Token.BEGIN,
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.NUMBER, Token.SEMI,
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.NUMBER, Token.SEMI,
                                         Token.LET,Token.IDENT,  Token.BECOMES, Token.NUMBER, Token.SEMI,
                                       
                                         Token.CALL, Token.IDENT, Token.OPENPAREN, Token.IDENT, Token.COMMA, Token.IDENT, Token.CLOSEPAREN, Token.SEMI,
                                         Token.LET, Token.IDENT, Token.BECOMES, Token.CALL, Token.IDENT, Token.SEMI,

                                         Token.CALL, Token.OUTPUTNUM, Token.OPENPAREN, Token.IDENT, Token.CLOSEPAREN,

                                        Token.END, Token.PERIOD};

            foreach(Token t in expectedTokens){
                res = s.GetSym();
                    Assert.AreEqual(t, res, "breaks line " + s.PC.ToString());

                }
              
            }


        
        [TestMethod]
        public void ConsistentVariIDTest001() {
            Scanner s = new Scanner("../../../ScannerParser/test001.txt");
            Token res;

            String[] expectedNames = { "x", "y", "x", "y", "x" , "y"};
            int idx = 0;
            do {
                res = s.GetSym();
                if (res == Token.IDENT) {
                    Assert.AreEqual(expectedNames[idx++], s.Id2String(s.id), "Missed at index " + idx.ToString());
                }


            } while (res != Token.EOF);

        
        }
        [TestMethod]
        public void ConsistentVariIDTest002() {
            Scanner s = new Scanner("../../../ScannerParser/testFiles/test002.txt");
            Token res;

            String[] expectedNames = { "x", "y", "z","a", "b", "c", "foo","i", "d",

	"i","i","y","y","z","x","d","y","z", "i", "i","d","bar", "x", "z","i","j", "e","i","j","i", 
            "j","b",  "i" , "j" , "j",
			"j","j", "a", "i" , "i","i" , "i" ,"x" ,"y" ,"z","bar", "x", "z","c" , "foo", "c" };
            int idx = 0;
            do {
                res = s.GetSym();
                if (res == Token.IDENT) {
                    Assert.AreEqual(expectedNames[idx++], s.Id2String(s.id), "Missed at index " + idx.ToString());
                }


            } while (res != Token.EOF);


        }

        [TestMethod]
        public void ErrorTest002() {
            Scanner s = new Scanner("../../../ScannerParser/testFiles/test002.txt");
            Token res = Token.ERROR;

            while (s.PC <= 5)
                res = s.GetSym();

            Assert.AreEqual(Token.FUNC, res);
            Assert.AreEqual(Token.IDENT, s.GetSym());
            Assert.AreEqual(Token.OPENPAREN, s.GetSym());
            s.Error("Error Message");
            s.GetSym();
            Assert.AreEqual(Token.ERROR, s.GetSym());
            Assert.AreEqual(Token.ERROR, s.GetSym());
            Assert.AreEqual(Token.ERROR, s.GetSym());
            s.Error("Error Message");
            s.GetSym();
            Assert.AreEqual(Token.ERROR, s.GetSym());
            Assert.AreEqual(Token.ERROR, s.GetSym());
            Assert.AreEqual(Token.ERROR, s.GetSym());




        }
        
       
       
       
    }
}
