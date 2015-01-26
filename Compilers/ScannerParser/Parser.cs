﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser
{
    public enum Token
    {
        ERROR = 0, TIMES = 1, DIV = 2,
        PLUS = 11, MINUS = 12, EQL = 20,
        NEQ, LSS, GEQ, LEQ, GTR,
        PERIOD = 30, COMMA, OPENBRACKET, CLOSEBRACKET, CLOSEPAREN,
        BECOMES = 40, THEN, DO,
        OPENPAREN = 50, NUMBER = 60, IDENT,
        SEMI = 70, END = 80, OD, FI,
        ELSE = 90, LET = 100, CALL, IF, WHILE, RETURN,
        VAR = 110, ARR, FUNC, PROC,
        BEGIN = 150, MAIN = 200, EOF = 255
    };

    public class Parser
    {
        private Token scannerSym; // current token on input
        private Scanner scanner;
        private FileStream fs; //debug
        private StreamWriter sw;
        private int currRegister;

        public Parser(String file)
        {
            scanner = new Scanner(file);
            fs = File.Open(@"../../output.txt", FileMode.Create, FileAccess.ReadWrite);
            sw = new StreamWriter(fs);
            Init();
        }

        // Sets initial state of parser
        // for now
        private void Init()
        {
            Next(); // set first symbol
        }

        ~Parser()
        {
            if (sw != null)
                sw.Close();
            if (fs != null)
                fs.Close();
        }

        private void Next()
        {
            scannerSym = scanner.GetSym();
            //HandleToken(); TODO this needs to be moved somewhere else because causes problems in next
        }

        private void HandleToken()
        {
            switch (scannerSym)
            {
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

        private void Error()
        {
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

        private Result Expression()
        {
            Result res;
            Result res2;
            Token opCode;
            res = Term();
            while (scannerSym == Token.PLUS || scannerSym == Token.MINUS)
            {
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

        private Result Term()
        {
            Result res = null, resB = null;
            if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER)
            {
                res = Factor(); // will end on next token
                while (scannerSym == Token.TIMES || scannerSym == Token.DIV)
                {
                    Token tmp = scannerSym;
                    Next();
                    if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER)
                    {
                        resB = Factor();
                        res = Combine(tmp, res, resB);
                    }
                    else
                    {
                        scanner.Error("Reached Term but don't have an Identifier or Number");
                    }

                }

            }
            else
            {
                scanner.Error("Reached Term but don't have an Identifier or Number");
            }
            return res;
        }

        private Result Factor()
        {
            Result res = null;
            if (scannerSym == Token.IDENT)
            {
                res = Designator();
            }
            else if (scannerSym == Token.NUMBER)
            {
                res = new Result(Kind.CONST, (double)Number());

            }
            else if (scannerSym == Token.FUNC)
            {
                // TODO:: Function things
                res = new Result();
            }
            else if (scannerSym == Token.OPENPAREN)
            {
                Next();
                res = Expression();
                if (scannerSym == Token.CLOSEPAREN)
                {
                    Next();
                }
                else
                {
                    sw.WriteLine("Load {0}[{1}] R{2}", res.valueS, res.valueD, currRegister);
                    Console.WriteLine("Load {0}[{1}] R{2}", res.valueS, res.valueD, currRegister);
                    res = new Result(Kind.REG, currRegister); // TODO:: Or a variable?
                }

            }
            return res;
        }

        private Result Designator()
        {
            Result res, expr;
            if (scannerSym == Token.IDENT)
            {
                res = Ident();
                if (scannerSym == Token.OPENBRACKET)
                {
                    expr = Expression();
                    Next();
                    if (scannerSym == Token.CLOSEBRACKET)
                    {
                        Next();
                        AllocateRegister();
                        sw.WriteLine("Load {0}[{1}] R{2}", res.valueS, expr.valueD, currRegister);
                        Console.WriteLine("Load {0}[{1}] R{2}", res.valueS, expr.valueD, currRegister);
                        res = new Result(Kind.REG, currRegister); // TODO:: Or a variable?

                    }
                    else
                    {
                        scanner.Error("Designator: Unmatched [.... missing ]");
                        res = new Result(); // this is unreachable;
                    }

                }
                else
                {
                    // returning an identifier
                }

            }
            else
            {
                scanner.Error("Ended up at Designator but didn't parse an identifier");
                res = new Result(); // this is unreachable;
            }
            return res;
        }

        private double Number()
        {
            // TODO figure out if we need to have a function for number, but for now
            // it will return the last number seen

            //Result res = null;
            //if (scannerSym == Token.NUMBER)
            //{
            //    Next();
            //    res = Digit();

            //}
            //else
            //{
            //    scanner.Error("Ended up at Identifier but didn't parse an identifier");
            //    res = new Result(); // this is unreachable;

            //}
            Next();
            return scanner.number;
        }


        private Result FuncCall()
        {
            Result res = null;
            List<Result> optionalArguments = null;
            if (scannerSym == Token.CALL)
            {
                Next();
                if (scannerSym == Token.IDENT)
                {
                    res = Ident();
                    //Next(); // Taken care of by Ident
                    if (scannerSym == Token.OPENPAREN)
                    {
                        Next();
                        if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER)
                        {
                            optionalArguments = new List<Result>();
                            optionalArguments.Add(Expression());
                        }
                        else
                        {
                            Next();
                        }
                        // TODO code that handles expression in parentheses or another function

                        while (scannerSym == Token.COMMA)
                        {
                            if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER ||
                                scannerSym == Token.OPENPAREN || scannerSym == Token.CALL)
                            {
                                Next();
                                optionalArguments.Add(Expression());
                            }
                            else
                            {
                                scanner.Error("Ended up in optional arguments of function call and didn't parse a number, variable, comma, or expression");
                            }
                        }

                        if (scannerSym == Token.CLOSEPAREN)
                        {
                            Next();
                            // Combine ALL THE THINGS! -- I don't know how to do this, how does
                            // passing arguments to function work in assembly?! BWAAAAHHHH!
                        }
                        else
                        {
                            scanner.Error("Ended up in arguments of function call and didn't parse a number, variable, comma, or expression");
                        }
                    }
                }
                else
                {
                    scanner.Error("Ended up at FuncCall but didn't parse an identifier");
                }
            }
            else
            {
                scanner.Error("Ended up at FuncCall but didn't recieve the keyword call");
            }
            return res;
        }

        private Result Ident()
        {
            Result res = null;
            if (scannerSym == Token.IDENT)
            {
                res = Letter();
            }
            else
            {
                scanner.Error("Ended up at Identifier but didn't parse an identifier");
                res = new Result(); // this is unreachable;
            }
            return res;

        }

        private Result Letter()
        {
            Result res;
            if (scannerSym == Token.IDENT)
            {
                Next();
                res = new Result(Kind.VAR, scanner.Id2String(scanner.id)); // changed to var for now to simulate output

            }
            else
            {
                scanner.Error("Ended up at Letter but didn't parse an identifier");
                res = new Result(); // this is unreachable;
            }

            return res;


        }

        private Result Digit()
        {
            Result res;
            if (scannerSym == Token.NUMBER)
            {
                Next();
                res = new Result(Kind.CONST, scanner.number);

            }
            else
            {
                scanner.Error("Ended up at Digit but didn't parse a Number");
                res = new Result(); // this is unreachable;
            }

            return res;

        }

        private Result Relation()
        {
            Result res1 = null;
            Result res2 = null;
            Result finalResult = null; ;

            res1 = Expression();
            Next();
            Token cond = scannerSym;
            Next();
            res2 = Expression();
            Next();
            finalResult = Combine(cond, res1, res2);
            finalResult.type = Kind.COND;
            finalResult.condition = Result.TokenToCondition(cond);
            return finalResult;
        }

        private void Assignment()
        {
            if (scannerSym == Token.LET)
            {
                Next();
                Result res1 = Designator();
                Next();
                Result res2 = Expression();
                sw.WriteLine("{0} {1} {2} {3}", "add", res1.regNo, res2.regNo, "R0");
                Console.WriteLine("{0} {1} {2} {3}", "add", res1.regNo, res2.regNo, "R0");
            }
            else
            {
                scanner.Error("Ended up at Assignment but didn't encounter let keyword");
            }
        }

        private Result Combine(Token opCode, Result A, Result B)
        {
            Result res = new Result();
            res.type = Kind.REG;
            AllocateRegister();// probably needs to change
            res.regNo = currRegister;

            if (A.type == Kind.VAR && B.type == Kind.VAR)
            {
                Result loadedA = LoadVariable(A);
                Result loadedB = LoadVariable(B);
                PutF2(TokenToInstruction(opCode), loadedA, loadedB);
            }
            else if (A.type == Kind.VAR && B.type == Kind.CONST)
            {
                Result loadedA = LoadVariable(A);
                PutF1(TokenToInstruction(opCode) + "i", loadedA, B);

            }
            else if (A.type == Kind.VAR && B.type == Kind.REG)
            {
                Result loadedA = LoadVariable(A);
                PutF2(TokenToInstruction(opCode), loadedA, B);

            }
            else if (B.type == Kind.VAR && A.type == Kind.VAR)
            {
                Result loadedB = LoadVariable(B);
                Result loadedA = LoadVariable(A);
                PutF2(TokenToInstruction(opCode), loadedB, loadedA);
            }
            else if (B.type == Kind.VAR && A.type == Kind.CONST)
            {
                Result loadedB = LoadVariable(B);
                PutF1(TokenToInstruction(opCode) + "i", loadedB, A);

            }
            else if (B.type == Kind.VAR && A.type == Kind.REG)
            {
                Result loadedB = LoadVariable(B);
                PutF2(TokenToInstruction(opCode), loadedB, A);

            }
            // This case causes issues with register allocation as it
            // is possibly not needed for constants
            else if (A.type == Kind.CONST && B.type == Kind.CONST)
            {
                switch (opCode)
                {
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

        private void Main(){
            if(VerifyToken(Token.MAIN, "Missing Main Function"))
            {
                if(scannerSym == Token.VAR | scannerSym == Token.ARR){
                    VarDecl();
                } else if(scannerSym == Token.FUNC || scannerSym == Token.PROC){
                    FuncDecl();
                }
                if(VerifyToken(Token.OPENBRACKET, "Missing Opening bracket of program"){
                StatSequence();
                }
				if(VerifyToken(Token.CLOSEBRACKET, "Missing closing bracket of program")){
                    Next();
                    if(VerifyToken(Token.PERIOD, "Unexpected end of program - missing period")){
                        sw.WriteLine("RET 0");
                    }
                }

        }

        private Result LoadVariable(Result r)
        {
            AllocateRegister();
            sw.WriteLine("load R{1} {0}", r.valueS, currRegister);
            Console.WriteLine("load R{1} {0}", r.valueS, currRegister);
            Result res = new Result();
            res.regNo = currRegister;
            res.type = Kind.REG;
            return res;
        }

        private void PutF2(string opString, Result a, Result b)
        {
            sw.WriteLine("{0} {1} {2} {3}", opString, currRegister, a.regNo, b.regNo);
            Console.WriteLine("{0} {1} {2} {3}", opString, currRegister, a.regNo, b.regNo);
        }


        // Creates a F1 instruction
        // Result b should be the Constant
        private void PutF1(string op, Result a, Result b)
        {
            sw.WriteLine("{0} {1} {2} {3}", op, currRegister, a.regNo, b.valueD);
            Console.WriteLine("{0} {1} {2} {3}", op, currRegister, a.regNo, b.valueD);


        }
        private void AllocateRegister()
        {
            currRegister++;
        }
        private void DeallocateRegister()
        {
            currRegister--;
        }

        private string TokenToInstruction(Token t)
        {
            string opString;
            switch (t)
            {
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
        private bool VerifyToken(Token t, string errMsg){
            if(scannerSym != t)
				scanner.Error(errMsg); // will exit program
            return true;
        }


    }
}