using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public enum Kind { VAR, COND, BRA, REG, CONST, ARR };
    public enum CondOp { GT=-200, LT, LEQ, GEQ, EQ, NEQ, ERR };
    public enum ConstantType { STRING, INT, DOUBLE, BOOLEAN, ADDR};

  public  class Result {

        public Kind? type;
        public int lineNumber;
        public CondOp? condition { private set; get; } // the condition operator, if this is a condition
        public ConstantType? constantType { private set; get; }
        public Result[] arrIndices { private set; get; }
       


        private string regName;
        private int fixUpLoc;
        private double valueD; // value of constant
        private string valueS; // value of constant or the variable name or an address
        private bool valueB; // value of evaluatable conditional
        private string arrBase;
        private int arrAddr;
        private Instruction whereICameFrom; // the instruction that generated this result


        // Constructors

        public Result(Kind myType, string arrName, Result[] indicesForArray) {
            if (myType != Kind.ARR) {
                type = null;
                Console.WriteLine("WARNING: failed to initialize result. Wrong Kind for Array constructor");
                return;
            }
            type = Kind.ARR;
            arrBase = arrName;
            arrIndices = indicesForArray;

        }

        public Result(Kind myType, ConstantType otherType, double value) {
            type = myType;
            constantType = otherType;
            valueD = value;
        }
        public Result(Kind myType, ConstantType otherType, string value) {
            type = myType;
            constantType = otherType;
            valueS = value;
        }
        public Result(Kind myType, ConstantType otherType, bool value) {
            type = myType;
            constantType = otherType;
            valueB = value;
        }
       // Conditional result
        public Result(Kind myType, CondOp op) {
            if (myType != Kind.COND) {
                Console.WriteLine("WARNING: Initializing Result with wrong value type; Expecting Conditional");
            }
            type = myType;
            condition = op;
        }
        public Result(Kind myType, CondOp op, int myLine) {
            if (myType != Kind.COND) {
                Console.WriteLine("WARNING: Initializing Result with wrong value type Expecting Conditional");
            }
            type = myType;
            condition = op;
            lineNumber = myLine;
        }

        public Result(Kind myType, double myValue) {
            if (myType != Kind.CONST)
                Console.WriteLine("WARNING: Initializing Result with wrong value type: Expecting CONST");
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
            } else if (myType == Kind.ARR) {
                type = myType;
                arrBase = myValue;
            } else if (myType == Kind.BRA) {
                type = Kind.BRA;
                valueS = myValue;
            }
            else {
                Console.WriteLine("WARNING: Initializing Result with wrong value type: Expectiong CONST, VAR, ARR, or REG");
            }

        }
        public Result(Kind myType, bool myValue) {
            if (myType != Kind.COND)
                Console.WriteLine("WARNING: Initializing Result with wrong value type: Expecting COND");
            type = myType;
            valueB = myValue;

        }
        
        

        // Accessors

        // If you feed a Result to this, it will pull out the right value
        public string GetValue() {
            string s = null;
            switch (type) {
                case Kind.VAR:
                    s = valueS;
                    break;
                case Kind.REG:
                    s = regName;
                    break;
                case Kind.COND:
                    s = regName;
                    break;
                case Kind.CONST:
                    switch (constantType) {
                        case ConstantType.DOUBLE:
                            s = valueD.ToString();
                            break;
                        case ConstantType.STRING:
                            s = valueS;
                            break;
                        case ConstantType.ADDR:
                            if (valueD != 0)
                                s = String.Format("MEM[{0}]", valueD);
                            else
                                s = valueS;
                            break;
                    }
                    break;
                case Kind.ARR:
                    s = arrBase;
                    break;
                case Kind.BRA:
                    s = valueS;
                    break;
            }
            return s;
        }

        public Result[] GetArrayIndices() {
            if (type == Kind.ARR) {
                return arrIndices;
            }
            return null;
        }

        public void SetValue(string s) {
            valueS = s;

        }
        public void SetValue(double d) {
            valueD = d;

        }
      
        // Utilities
      
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
