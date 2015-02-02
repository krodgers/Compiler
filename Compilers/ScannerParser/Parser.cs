﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public enum Token {
        ERROR = 0, TIMES = 1, DIV = 2,
        PLUS = 11, MINUS = 12, EQL = 20,
        NEQ, LSS, GEQ, LEQ, GTR,
        PERIOD = 30, COMMA, OPENBRACKET, CLOSEBRACKET, CLOSEPAREN,
        BECOMES = 40, THEN, DO,
        OPENPAREN = 50, NUMBER = 60, IDENT,
        SEMI = 70, END = 80, OD, FI,
        ELSE = 90, LET = 100, CALL, IF, WHILE, RETURN,
        VAR = 110, ARR, FUNC, PROC,
        BEGIN = 150, MAIN = 200, EOF = 255,
        OUTPUTNUM, INPUTNUM, OUTPUTNEWLINE
    };

    public enum OpCodeClass {
        ARITHMETIC_REG = 0, ARITHMETIC_IMM, MEM_ACCESS, CONTROL, COMPARE, ERROR
    };

    // TODO:: Where should we catch undeclared identifiers?
    public class Parser {
        private Token scannerSym; // current token on input
        private Scanner scanner;
        private FileStream fs; //debug
        private StreamWriter sw;
        private int currRegister;

        public Parser(String file) {
            scanner = new Scanner(file);
            fs = File.Open(@"../../output.txt", FileMode.Create, FileAccess.ReadWrite);
            sw = new StreamWriter(fs);
            Init();
        }

        // Sets initial state of parser
        // for now
        private void Init() {
            Next(); // set first symbol
        }

        ~Parser() {
            if (sw != null)
                sw.Close();
            if (fs != null)
                fs.Close();
        }

        private void Next() {
            scannerSym = scanner.GetSym();
            //HandleToken(); TODO this needs to be moved somewhere else because causes problems in next
        }

        public void StartFirstPass() {
            Main();
        }

        private void HandleToken() {
            switch (scannerSym) {
                #region Symbol Case Statements
                case Token.ERROR:
                    Error();
                    break;
                case Token.TIMES:
                    break;
                case Token.DIV:
                    break;
                case Token.PLUS:
                    break;
                case Token.MINUS:
                    break;
                case Token.EQL:
                    break;
                case Token.NEQ:
                    break;
                case Token.LSS:
                    break;
                case Token.GEQ:
                    break;
                case Token.LEQ:
                    break;
                case Token.GTR:
                    break;
                case Token.PERIOD:
                    break;
                case Token.COMMA:
                    break;
                case Token.OPENBRACKET:
                    break;
                case Token.CLOSEBRACKET:
                    break;
                case Token.CLOSEPAREN:
                    break;
                case Token.BECOMES:
                    break;
                case Token.THEN:
                    break;
                case Token.DO:
                    break;
                case Token.OPENPAREN:
                    break;
                case Token.NUMBER:
                    break;
                case Token.IDENT:
                    break;
                case Token.SEMI:
                    break;
                case Token.END:
                    break;
                case Token.OD:
                    break;
                case Token.FI:
                    break;
                case Token.ELSE:
                    break;
                case Token.LET:
                    break;
                case Token.CALL:
                    break;
                case Token.IF:
                    break;
                case Token.WHILE:
                    break;
                case Token.RETURN:
                    break;
                case Token.VAR:
                    break;
                case Token.ARR:
                    break;
                case Token.FUNC:
                    break;
                case Token.PROC:
                    break;
                case Token.BEGIN:
                    break;
                case Token.MAIN:
                    break;
                case Token.EOF:
                    break;
                #endregion
            }
        }

        private void Error() {
            string msg;
            msg = string.Format("Unexpected {0} {1}, syntax error", scannerSym.ToString(), scanner.Id2String(scanner.id));
            scanner.Error(msg);
        }

        //private string relation() {
        //    Result ResA = Expression();
        //    Result ResOp = RelOp();
        //    Result ResB = Expression();
        //    return CombineRelation(ResA, ResB, ResOp);
        //}
        //private string CombineRelation(Result A, Result B, Result Op) {
        //    string res;


        //    if (A.type == Kind.CONST && B.type == Kind.CONST) {
        //        switch (Op.condition) {
        //             case CondOp.EQ:
        //                return (A.valueD == B.valueD).ToString();
        //            break;
        //                // TODO:: Rest of options

        //        }

        //    }
        //    else if (A.type == Kind.VAR && B.type == Kind.VAR) {
        //        return string.Format("cmp {0}, {1}, {2}", );
        //    }


        //    return ":D";
        //}

        private Result Expression() {
            Result res;
            Result res2;
            Token opCode;
            res = Term();
            while (scannerSym == Token.PLUS || scannerSym == Token.MINUS) {
                opCode = scannerSym == Token.PLUS ? Token.PLUS : Token.MINUS;
                Next();
                res2 = Term();
                res = Combine(opCode, res, res2);
            }

            // Handled in Combine
            //res.type = Kind.REG;
            //AllocateRegister();
            //res.regNo = currRegister;

            return res;
        }

        private Result Term() {
            Result res = null, resB = null;
            if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER || scannerSym == Token.OPENPAREN || scannerSym == Token.CALL) {
                res = Factor(); // will end on next token
                while (scannerSym == Token.TIMES || scannerSym == Token.DIV) {
                    Token tmp = scannerSym;
                    Next();
                    if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER) {
                        resB = Factor();
                        res = Combine(tmp, res, resB);
                    }
                    else {
                        scanner.Error("Reached Term but don't have an Identifier or Number");
                    }

                }

            }
            else {
                scanner.Error("Reached Term but don't have an Identifier or Number");
            }
            return res;
        }

        private Result Factor() {
            Result res = null;
            if (scannerSym == Token.IDENT) {
                res = Designator();
            }
            else if (scannerSym == Token.NUMBER) {
                res = new Result(Kind.CONST, Number().valueD);

            }
            else if (scannerSym == Token.FUNC) {
                // TODO:: Function things
                res = new Result();
            }
            else if (scannerSym == Token.OPENPAREN) {
                Next();
                res = Expression();
                if (scannerSym == Token.CLOSEPAREN) {
                    Next();
                }
                else {
                    sw.WriteLine("Load {0}[{1}] R{2}", res.valueS, res.valueD, currRegister);
                    Console.WriteLine("Load {0}[{1}] R{2}", res.valueS, res.valueD, currRegister);
                    res = new Result(Kind.REG, currRegister); // TODO:: Or a variable?
                }

            }
            return res;
        }

        private Result Designator() {
            Result res = null, expr = null;
            VerifyToken(Token.IDENT, "Ended up at Designator but didn't parse an identifier");
            res = Ident();
            if (scannerSym == Token.OPENBRACKET) {
                expr = Expression();
                Next();
                VerifyToken(Token.CLOSEBRACKET, "Designator: Unmatched [.... missing ]");
                Next();
                AllocateRegister();
                sw.WriteLine("Load {0}[{1}] R{2}", res.valueS, expr.valueD, currRegister);
                Console.WriteLine("Load {0}[{1}] R{2}", res.valueS, expr.valueD, currRegister);
                res = new Result(Kind.REG, currRegister); // TODO:: Or a variable?
            }


            return res;
        }

        private Result Number() {
            Result res = null;
            VerifyToken(Token.NUMBER, "Ended up at Number but didn't parse a Number");
            Next(); // eat the number
            res = new Result(Kind.CONST, scanner.number);

            return res;
        }


        private Result FuncCall() {
            Result res = null;
            List<Result> optionalArguments = null;

            VerifyToken(Token.CALL, "Ended up at FuncCall but didn't recieve the keyword call");
            Next();

            switch (scannerSym) {
                case Token.IDENT:
                    res = Ident();
                    if (scannerSym == Token.OPENPAREN) {
                        Next();
                        if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER) {
                            optionalArguments = new List<Result>();
                            optionalArguments.Add(Expression());
                        }
                        else {
                            Next();
                        }
                        // TODO code that handles expression in parentheses or another function

                        while (scannerSym == Token.COMMA) {
                            Next();
                            if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER ||
                                scannerSym == Token.OPENPAREN || scannerSym == Token.CALL) {
                                optionalArguments.Add(Expression());
                            }
                            else {
                                scanner.Error("Ended up in optional arguments of function call and didn't parse a number, variable, comma, or expression");
                            }
                        }


                        VerifyToken(Token.CLOSEPAREN, "Ended up in arguments of function call and didn't parse a number, variable, comma, or expression");
                        Next();
                        // Combine ALL THE THINGS! -- I don't know how to do this, how does
                        // passing arguments to function work in assembly?! BWAAAAHHHH!
                    }
                    // TODO:: Verify the function being called is defined/ in scope
                    break;

                case Token.OUTPUTNEWLINE:

                    Next(); // eat OutputNewLine()
                    VerifyToken(Token.OPENPAREN, "Called OutputNewLine, missing (");
                    Next(); // eat (
                    VerifyToken(Token.CLOSEPAREN, "Called OutputNewLine, missing )");
                    Next(); // eat )
                    sw.WriteLine("WRL");
                    break;

                case Token.OUTPUTNUM:
                    Next(); // eat OutputNum
                    VerifyToken(Token.OPENPAREN, "OutputNum missing (");

                    if (scannerSym == Token.CALL)
                        res = FuncCall();
                    else if (scannerSym == Token.NUMBER || scannerSym == Token.IDENT)
                        res = Expression();
                    sw.WriteLine("WRD " + res.valueD);

                    VerifyToken(Token.CLOSEPAREN, "OutputNum missing )");
                    Next(); // eat )

                    break;


                case Token.INPUTNUM:
                    Next(); // eat InputNum
                    VerifyToken(Token.OPENPAREN, "InputNum missing (");
                    VerifyToken(Token.CLOSEPAREN, "InputNum has too many arguments or is missing )");
                    AllocateRegister();
                    res = new Result(Kind.REG, currRegister);
                    sw.WriteLine("RDD {0}", res.regNo);
                    break;

                default:
                    scanner.Error("Ended up at FuncCall but didn't parse an identifier");
                    break;

            }

            if (scannerSym == Token.SEMI)
                Next(); // eat the semicolon


            // TODO:: What to return here if the function doesn't return anything?
            return res;
        }

        private Result IfStatement() {
            Result res = null;
            if (scannerSym == Token.IF) {
                Next();
                if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER
                    || scannerSym == Token.OPENPAREN || scannerSym == Token.CALL) {
                    res = Relation();
                    if (VerifyToken(Token.THEN, "The then keyword did not follow the relation in if statement")) {
                        if (res.condition != CondOp.ERR) {
                            string negatedTokenString = TokenToInstruction(NegatedConditional(res.condition));
                            PutF1(negatedTokenString, res, new Result(Kind.CONST, "offset"));
                        }
                        else {
                            scanner.Error("The relation in the if did not contain a valid conditional operator");
                        }
                        Next();
                        StatSequence();

                        if (scannerSym == Token.ELSE) {
                            Next();
                            StatSequence();
                        }
                        else if (scannerSym == Token.FI) {
                            Next();
                        }
                        else {
                            scanner.Error("In the if statement and found no token that matches either an else or a then");
                        }
                    }


                }

            }
            else {
                scanner.Error("Ended up at If Statement but didn't parse the if keyword");
            }
            return new Result(); // todo, I don't have any idea what should be in this result, or if it's even needed
        }

        private Result Ident() {
            Result res = null;
            VerifyToken(Token.IDENT, "Ended up at Identifier but didn't parse an identifier");
            Next(); // eat the identifier
            res = new Result(Kind.VAR, scanner.Id2String(scanner.id)); // changed to var for now to simulate output
            return res;

        }



        private Result Relation() {
            Result res1 = null;
            Result res2 = null;
            Result finalResult = null; ;

            res1 = Expression();
            Token cond = scannerSym;
            Next();
            res2 = Expression();
            finalResult = Combine(cond, res1, res2);
            finalResult.type = Kind.COND;
            finalResult.condition = Result.TokenToCondition(cond);
            return finalResult;
        }
        // TODO:: Should this return a result?  Like which register the assignment has been loaded into??

        private void Assignment() {
            if (scannerSym == Token.LET) {
                Next();
                Result res1 = Designator();
                Token assignSym = Token.ERROR;
                if (VerifyToken(Token.BECOMES, "Assignment made without the becomes symbol"))
                    assignSym = scannerSym;
                Next();
                Result res2 = Expression();

                // TODO:: THis is wrong.  res1 and res2 may not be registers.  Should they be?
                sw.WriteLine("{0} {1} {2} {3}", assignSym, res1.regNo, res2.regNo, "R0");
                Console.WriteLine("{0} {1} {2} {3}", assignSym, res1.regNo, res2.regNo, "R0");
            }
            else {
                scanner.Error("Ended up at Assignment but didn't encounter let keyword");
            }
        }

        private Result Combine(Token opCode, Result A, Result B) {
            Result res = new Result();

            if (A.type == Kind.VAR && B.type == Kind.VAR) {
                switch (GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        PutArithmeticRegInstruction(TokenToInstruction(opCode), A, B);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF2(TokenToInstruction(opCode), loadedA, loadedB);
            }
            else if (A.type == Kind.VAR && B.type == Kind.CONST) {
                switch (GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        PutArithmeticImmInstruction(TokenToInstruction(opCode), A, B);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF1(TokenToInstruction(opCode) + "i", loadedA, B);

            }
            else if (A.type == Kind.VAR && B.type == Kind.REG) {
                // todo, fill in later because reg is weird right now
                //PutF2(TokenToInstruction(opCode), loadedA, B);

            }
            else if (B.type == Kind.VAR && A.type == Kind.VAR) {

                //PutF2(TokenToInstruction(opCode), loadedB, loadedA);
            }
            else if (B.type == Kind.VAR && A.type == Kind.CONST) {

                //PutF1(TokenToInstruction(opCode) + "i", loadedB, A);

            }
            else if (B.type == Kind.VAR && A.type == Kind.REG) {

                //PutF2(TokenToInstruction(opCode), loadedB, A);

            }
            // This case causes issues with register allocation as it
            // is possibly not needed for constants
            else if (A.type == Kind.CONST && B.type == Kind.CONST) {
                switch (opCode) {
                    case Token.TIMES:
                        res.valueD = (double)(A.valueD * B.valueD);
                        break;
                    case Token.DIV:
                        res.valueD = (double)(A.valueD / B.valueD);
                        break;
                    case Token.PLUS:
                        res.valueD = (double)(A.valueD + B.valueD);
                        break;
                    case Token.MINUS:
                        res.valueD = (double)(A.valueD - B.valueD);
                        break;
                }

                res.type = Kind.CONST;
            }

            return res;
        }


        private void Main() {
            if (VerifyToken(Token.MAIN, "Missing Main Function")) {// find "main"
                Next();
                while (scannerSym == Token.VAR | scannerSym == Token.ARR) { // variable declarations
                    VarDecl();
                }
                while (scannerSym == Token.FUNC || scannerSym == Token.PROC) {// function declarations
                    FuncDecl();
                }

                // start program
                if (VerifyToken(Token.BEGIN, "Missing Opening bracket of program")) {
                    Next();
                    StatSequence();
                }
                // end program
                if (VerifyToken(Token.CLOSEBRACKET, "Missing closing bracket of program")) {
                    Next();
                    if (VerifyToken(Token.PERIOD, "Unexpected end of program - missing period")) {
                        sw.WriteLine("RET 0");
                    }

                }

            }
        }


        private void FuncDecl() {
            if (scannerSym == Token.FUNC || scannerSym == Token.PROC) {
                Next();
            }
            VerifyToken(Token.IDENT, "Function/Procedure declaration missing a name");
            Ident();
            if (scannerSym == Token.OPENPAREN) {
                // Formal Parameter
                if (scannerSym == Token.IDENT) {
                    Ident();
                    while (scannerSym == Token.COMMA) {
                        Next();
                        Ident();
                    }
                }
                VerifyToken(Token.CLOSEPAREN, "Function Declaration missing a closing parenthesis");
            }
            VerifyToken(Token.SEMI, "Function Declaration missing a semicolon");
            // Func Body
            // Variable declarations
            while (scannerSym == Token.VAR || scannerSym == Token.ARR) {
                VarDecl();
            }
            VerifyToken(Token.OPENBRACKET, "Function body missing open bracket");
            // function statements
            StatSequence();

            VerifyToken(Token.CLOSEBRACKET, "Function Body missing closing bracket"); //end function body
            VerifyToken(Token.SEMI, "Function declaration missing semicolon"); // end function declaration
        }


        private void StatSequence() {
            Statement();
            while (scannerSym == Token.SEMI) {
                Next(); // eat Semicolon
                Statement();
            }
        }

        private Result Statement() {
            Result res = null;
            switch (scannerSym) {
                case Token.LET:
                    Assignment();
                    break;
                case Token.CALL:
                    res = FuncCall();
                    break;
                case Token.IF:
                    res = IfStatement();
                    break;
                case Token.WHILE:
                    res = WhileStatement();
                    break;
                case Token.RETURN:
                    res = ReturnStatement();
                    break;
                default:
                    scanner.Error("Expected a statement of some sorts.");
                    break;
            }

            return res;
        }

        private Result WhileStatement() {
            // todo, with the while statement and the if statements, fixuploc and such need
            // to be implemented

            VerifyToken(Token.WHILE, "Got to while statement without seeing the while keyword");
            Next(); // eat while
            Result res = Relation();
            VerifyToken(Token.WHILE, "No do keyword after the relation in while statement");
            Next(); // eat do
            if (res.condition != CondOp.ERR) {
                string negatedTokenString = TokenToInstruction(NegatedConditional(res.condition));
                PutF1(negatedTokenString, res, new Result(Kind.CONST, "offset"));
                StatSequence();
                // todo, here we will need a branch to loop header
                VerifyToken(Token.OD, "The while loop was not properly closed with the od keyword");
                Next(); //eat od
            }
            else {
                scanner.Error("The relation in the if did not contain a valid conditional operator");
            }
            return new Result(); // todo, don't know what this should be either

        }

        private Result ReturnStatement() {
            VerifyToken(Token.RETURN, "Missing return keyword");
            Result res = null;
            if (scannerSym == Token.IDENT) {
                res = Expression();
            }

            // TODO: How does function know where to return to?
            // pop stack?
            // restore frames?
            // load return value into return register?
            sw.WriteLine("RET ??");
            return null;
        }



        // TODO:: This would be the optimal place to create ID-->Variable Map
        private void VarDecl() {
            if (scannerSym == Token.VAR) {
                Next(); // eat "var"
                VerifyToken(Token.IDENT, "Variable declaration missing variable name");
                Next(); // eat the ident 

                while (scannerSym == Token.COMMA) {
                    Next(); // eat the comma
                    VerifyToken(Token.IDENT, "Dangling comma in variable declaration");
                    Next(); // eat the ident

                }

            }
            if (scannerSym == Token.ARR) {
                Next(); // eat  "array"
                do {
                    VerifyToken(Token.OPENBRACKET, "Array declaration missing [");
                    Next(); // eat [
                    Number();
                    VerifyToken(Token.CLOSEBRACKET, "Array declaraion missing ]");
                } while (scannerSym != Token.SEMI);


            }

            VerifyToken(Token.SEMI, "Missing semicolon at end of variable declaration");
            Next(); // eat semicolon

        }

        private void OutputNum(Result r) {
            // todo, implement outputNum, but we need way of knowing that it needs to be called
            if (r.type == Kind.REG) {

            }
            else {
                Debug.WriteLine("OutputNum was not provided with a register as an argument, this is a parser error");
            }
        }


        private void ConditionalJumpForward(Result x) {

        }

        private Token NegatedConditional(CondOp? cond) {
            // todo, do we even need CondOp? It seems redundant
            switch (cond) {
                case CondOp.EQ:
                    return Token.NEQ;
                case CondOp.NEQ:
                    return Token.EQL;
                case CondOp.LT:
                    return Token.GEQ;
                case CondOp.GT:
                    return Token.LEQ;
                case CondOp.LEQ:
                    return Token.GTR;
                case CondOp.GEQ:
                    return Token.LSS;
                default:
                    return Token.ERROR;
            }
        }

        private Result LoadVariable(Result r) {
            AllocateRegister();
            sw.WriteLine("load R{1} {0}", r.valueS, currRegister);
            Console.WriteLine("load R{1} {0}", r.valueS, currRegister);
            Result res = new Result();
            res.regNo = currRegister;
            res.type = Kind.REG;
            return res;
        }

        private Result PutArithmeticRegInstruction(string opCode, Result a, Result b) {

            return null;
        }

        private Result PutArithmeticImmInstruction(string opCode, Result a, Result b) {

            return null;
        }

        private void PutF2(string opString, Result a, Result b) {
            sw.WriteLine("{0} {1} {2} {3}", opString, currRegister, a.regNo, b.regNo);
            Console.WriteLine("{0} {1} {2} {3}", opString, currRegister, a.regNo, b.regNo);
        }


        // Creates a F1 instruction
        // Result b should be the Constant
        private void PutF1(string op, Result a, Result b) {

            if (b.type == Kind.CONST) {
                sw.WriteLine("{0} {1} {2} {3}", op, currRegister, a.regNo, b.valueD);
                Console.WriteLine("{0} {1} {2} {3}", op, currRegister, a.regNo, b.valueD);

            }
            else if (a.type == Kind.COND) {
                // todo, the value for the second parameter shouldn't be a string
                // it needs to be the offset, but don't know how to do that yet
                sw.WriteLine("{0} {1} {2}", op, a.regNo, b.valueS);
                Console.WriteLine("{0} {1} {2}", op, a.regNo, b.valueS);
            }
            else {
                Console.WriteLine("PutF1 paramters in wrong order.");
                sw.WriteLine("{0} {1} {2} {3}", op, currRegister, b.regNo, a.valueD);
                Console.WriteLine("{0} {1} {2} {3}", op, currRegister, b.regNo, a.valueD);

            }


        }
        private void AllocateRegister() {
            currRegister++;
        }
        private void DeallocateRegister() {
            currRegister--;
        }

        private OpCodeClass GetOpCodeClass(Token opCode, bool immediate = false) {
            switch (opCode) {
                case Token.TIMES:
                case Token.DIV:
                case Token.PLUS:
                case Token.MINUS:
                    if (immediate)
                        return OpCodeClass.ARITHMETIC_IMM;
                    else
                        return OpCodeClass.ARITHMETIC_REG;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GTR:
                case Token.LEQ:
                case Token.GEQ:
                    return OpCodeClass.COMPARE;
                case Token.BECOMES:
                    return OpCodeClass.MEM_ACCESS;
                default:
                    return OpCodeClass.ERROR;
            }
        }

        private string TokenToInstruction(Token t) {
            string opString;
            switch (t) {
                case Token.TIMES:
                    opString = "mul";
                    break;
                case Token.DIV:
                    opString = "div";
                    break;
                case Token.PLUS:
                    opString = "add";
                    break;
                case Token.MINUS:
                    opString = "sub";
                    break;
                case Token.BECOMES:
                    opString = "mov";
                    break;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                    opString = "cmp";
                    break;
                default:
                    opString = "nop";
                    break;

            }
            return opString;
        }

        // Checks if scannerSym == t
        // if not, pushes errMsg to Scanner and exits
        private bool VerifyToken(Token t, string errMsg) {
            if (scannerSym != t)
                scanner.Error(errMsg); // will exit program
            return true;
        }


    }
}