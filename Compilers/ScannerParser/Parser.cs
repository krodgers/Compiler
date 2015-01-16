using System;
using System.Collections.Generic;
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

    class Parser
    {
        private int scannerSym; // current token on input
        private Scanner scanner;

        public Parser(String file)
        {
            scanner = new Scanner(file);
        }

        private void Next()
        {
            scannerSym = scanner.GetSym();
            HandleToken();
        }

        private void HandleToken()
        {
            switch (scannerSym)
            {
                #region Symbol Case Statements
                case (int) Token.ERROR:
                    break;
                case (int)Token.TIMES:
                    break;
                case (int)Token.DIV:
                    break;
                case (int)Token.PLUS:
                    break;
                case (int)Token.MINUS:
                    break;
                case (int)Token.EQL:
                    break;
                case (int)Token.NEQ:
                    break;
                case (int)Token.LSS:
                    break;
                case (int)Token.GEQ:
                    break;
                case (int)Token.LEQ:
                    break;
                case (int)Token.GTR:
                    break;
                case (int)Token.PERIOD:
                    break;
                case (int)Token.COMMA:
                    break;
                case (int)Token.OPENBRACKET:
                    break;
                case (int)Token.CLOSEBRACKET:
                    break;
                case (int)Token.CLOSEPAREN:
                    break;
                case (int)Token.BECOMES:
                    break;
                case (int)Token.THEN:
                    break;
                case (int)Token.DO:
                    break;
                case (int)Token.OPENPAREN:
                    break;
                case (int)Token.NUMBER:
                    break;
                case (int)Token.IDENT:
                    break;
                case (int)Token.SEMI:
                    break;
                case (int)Token.END:
                    break;
                case (int)Token.OD:
                    break;
                case (int)Token.FI:
                    break;
                case (int)Token.ELSE:
                    break;
                case (int)Token.LET:
                    break;
                case (int)Token.CALL:
                    break;
                case (int)Token.IF:
                    break;
                case (int)Token.WHILE:
                    break;
                case (int)Token.RETURN:
                    break;
                case (int)Token.VAR:
                    break;
                case (int)Token.ARR:
                    break;
                case (int)Token.FUNC:
                    break;
                case (int)Token.PROC:
                    break;
                case (int)Token.BEGIN:
                    break;
                case (int)Token.MAIN:
                    break;
                case (int)Token.EOF:
                    break;
                #endregion
            }
        }
    }
}
