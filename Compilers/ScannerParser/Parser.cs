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


        private void Next() { }
    }
}
