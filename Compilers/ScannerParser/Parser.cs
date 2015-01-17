using System;
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

    class Parser
    {
        private Token scannerSym; // current token on input
        private Scanner scanner;
        private FileStream fs; //debug
        private StreamWriter sw;

        public Parser(String file)
        {
            scanner = new Scanner(file);
            fs = File.Open(@"C:\Users\kevin\CS241_Compiler\assembly.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            sw = new StreamWriter(fs);
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

        
    }
}
