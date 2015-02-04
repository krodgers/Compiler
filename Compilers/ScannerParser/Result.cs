using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public enum Kind { VAR, COND, REG, CONST };
    public enum CondOp { GT, LT, LEQ, GEQ, EQ, NEQ, ERR };
    public enum ConstantType { STRING, INT, DOUBLE, BOOLEAN };
    class Result {
        public Kind type;
        public string regName;
        public CondOp? condition; // the condition operator, if this is a condition
        public ConstantType? constantType;
        public int fixUpLoc;
        public double valueD; // value of constant
        public string valueS; // value of constant or the variable name
        public bool valueB; // value of evaluatable conditional

        public Result(Kind myType, double myValue) {
            if (myType != Kind.CONST)
                Console.WriteLine("WARNING: Initializing Result with wrong value type");
            type = myType;
            valueD = myValue;
            constantType = ConstantType.DOUBLE;

        }
        public Result(Kind myType, string myValue) {
            if (myType == Kind.CONST || myType == Kind.VAR) {
                type = myType;
                valueS = myValue;

                if (myType == Kind.CONST)
                    constantType = ConstantType.STRING;
            }
            else if (myType == Kind.REG) {
                type = myType;
                regName = myValue;
            }
            else {
                Console.WriteLine("WARNING: Initializing Result with wrong value type");
            }

        }
        public Result(Kind myType, bool myValue) {
            if (myType != Kind.COND)
                Console.WriteLine("WARNING: Initializing Result with wrong value type");
            type = myType;
            valueB = myValue;

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
