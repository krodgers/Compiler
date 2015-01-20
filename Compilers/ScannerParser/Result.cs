using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public enum Kind { VAR, COND, REG, CONST };
    public enum CondOp { GT, LT, LEQ, GEQ, EQ, NEQ, ERR };
    class Result {
        public Kind type;
        public int regNo;
        public CondOp condition; // the condition operator, if this is a condition
        public int fixUpLoc;
        public double valueD; // value of constant
        public string valueS; // value of constant or the variable name
        public bool valueB; // value of evaluatable conditional

        public Result(Kind myType, double myValue) {
            if (myType != Kind.CONST)
                Console.WriteLine("WARNING: Initializing Result with wrong value type");
            type = myType;
            valueD = myValue;

        }
        public Result(Kind myType, string myValue) {
            if (myType != Kind.CONST || myType != Kind.VAR)
                Console.WriteLine("WARNING: Initializing Result with wrong value type");
            type = myType;
            valueS = myValue;

        }
        public Result(Kind myType, bool myValue) {
            if (myType != Kind.COND)
                Console.WriteLine("WARNING: Initializing Result with wrong value type");
            type = myType;
            valueB = myValue;

        }
        public Result(Kind myType, int registerNum) {
            if (myType != Kind.REG)
                Console.WriteLine("WARNING: Initializing Result with wrong value type");
            type = myType;
            regNo = registerNum;

        }
        public Result() {

        }

        public static CondOp TokenToCondition(Token cond) {
            switch (cond)
            {
                case Token.GTR:
                    return CondOp.GT;
                case Token.LSS:
                    return CondOp.LT;
                case Token.LEQ:
                    return CondOp.LEQ;
                case Token.GEQ:
                    return CondOp.GEQ;
                case Token.EQL:
                    return CondOp.EQ;
                case Token.NEQ:
                    return CondOp.NEQ;
                default:
                    return CondOp.ERR;
            }
        }

    }
}
