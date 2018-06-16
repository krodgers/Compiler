Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ScannerParser.ScannerParser.Scanner
Imports ScannerParser.Token

using ScannerParser;

    <TestClass()>
    Public Class ScannerTest {
        <TestMethod()>
        Public Sub TokenMathTest() {

        ScannerParser.Scanner s = new ScannerParser.Scanner("../../../TokenTestFile.txt");
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


    End Sub
        <TestMethod()>
        Public Sub TokenRelOpTest() {

            Token res;
            ScannerParser.Scanner s = new ScannerParser.Scanner("../../../TokenTestFileRelOp.txt");
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

    End Sub
        <TestMethod()>
        Public Sub TokenAssignTest() {
            Token res;
            ScannerParser.Scanner s = new ScannerParser.Scanner("../../../TokenTestFileAssign.txt");
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
    End Sub
        <TestMethod()>
        Public Sub TokenPunctTest() {
            Token res;
            ScannerParser.Scanner s = new ScannerParser.Scanner("../../../TokenTestFilePunct.txt");

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
    End Sub
        <TestMethod()>
        Public Sub TokenKeyWordTest() {
            Token res;
            ScannerParser.Scanner s = new ScannerParser.Scanner("../../../TokenTestFileKeyWords.txt");


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

    End Sub




        //TODO:  test all tokens
        // test tokens on comment line
        // test errors - unexpected EOF
        // test errors - unmatched {,(
    End Sub
End Class
