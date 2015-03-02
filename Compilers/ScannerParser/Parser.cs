﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO:: we're going to have to add some accessors for the next steps
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

    public class Parser {
        private Token scannerSym; // current token on input
        private Scanner scanner;
        private FileStream fs; //debug
        private int currRegister;
        private int AssemblyPC;
        private BasicBlock rootBasicBlock;
        private BasicBlock curBasicBlock;
        private int nextBBid; // next available basic block id
        private Stack<BasicBlock> parentBlocks;

        private Stack<int> scopes; // track the current scope
        private int nextScopeNumber; // next assignable scope number
        //   private Dictionary<Symbol, List<Symbol>> symbolTable; // indexed by function
        private List<Symbol> symbolTable; // all the symbols! // indexed by symbol id

        private Stack<BasicBlock> joinBlocks;
        private Stack<BasicBlock> loopHeaderBlocks; 
        private int globalNestingLevel;
        BasicBlock trueBlock, falseBlock, joinBlock;
        Dictionary<int, BasicBlock> flowGraphNodes;


        public Parser(String file) {
            scanner = new Scanner(file);
            fs = File.Open(@"../../output.txt", FileMode.Create, FileAccess.ReadWrite);
            SSAWriter.sw = new StreamWriter(fs);
            Init();
            AssemblyPC = 1;
            nextBBid = 1;
            curBasicBlock = null;
            parentBlocks = new Stack<BasicBlock>();

            scopes = new Stack<int>();
            symbolTable = new List<Symbol>();
            nextScopeNumber = 2;

            joinBlocks = new Stack<BasicBlock>();
            loopHeaderBlocks = new Stack<BasicBlock>();
            
            globalNestingLevel = 0;
            trueBlock = falseBlock = joinBlock = null;
            flowGraphNodes = new Dictionary<int, BasicBlock>();
        }

        // Sets initial state of parser
        // for now
        private void Init() {
            Next(); // set first symbol

        }

        ~Parser() {
            try {
                if (SSAWriter.sw != null)
                    SSAWriter.sw.Close();
                if (fs != null)
                    fs.Close();
            } catch (Exception) {
                Console.WriteLine("File may not be closed properly....");
            }

        }

        private void Next() {
            scannerSym = scanner.GetSym();
            //HandleToken(); TODO this needs to be moved somewhere else because causes problems in next
        }

        public void StartFirstPass() {
            rootBasicBlock = new BasicBlock(nextBBid++);
            flowGraphNodes[rootBasicBlock.blockNum] = rootBasicBlock;
            rootBasicBlock.childBlocks = new List<BasicBlock>();
            rootBasicBlock.parentBlocks = new List<BasicBlock>();
            rootBasicBlock.nestingLevel = globalNestingLevel;
            curBasicBlock = rootBasicBlock;

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
        //                return (A.GetValue() == B.GetValue()).ToString();
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
            if (res == null)
                Console.WriteLine("{0}: Expression() got null res", AssemblyPC);
            while (scannerSym == Token.PLUS || scannerSym == Token.MINUS) {
                opCode = scannerSym == Token.PLUS ? Token.PLUS : Token.MINUS;
                Next();
                res2 = Term();
                res = Combine(opCode, res, res2);
            }

            return res;
        }

        private Result Term() {
            Result res = null, resB = null;
            if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER || scannerSym == Token.OPENPAREN || scannerSym == Token.CALL) {
                res = Factor(); // will end on next token
                if (res == null)
                    Console.WriteLine("{0}: Term() got null res");

                while (scannerSym == Token.TIMES || scannerSym == Token.DIV) {
                    Token tmp = scannerSym;
                    Next();
                    if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER) {
                        resB = Factor();
                        res = Combine(tmp, res, resB);
                    } else {
                        scanner.Error("Reached Term but don't have an Identifier or Number");
                    }

                }

            } else {
                scanner.Error("Reached Term but don't have an Identifier or Number");
            }
            return res;
        }

        private Result Factor() {
            Result res = null;
            if (scannerSym == Token.IDENT) {
                res = Designator();
                if (res == null)
                    Console.WriteLine("{0}: Factor got null res", AssemblyPC);

            } else if (scannerSym == Token.NUMBER) {
                res = new Result(Kind.CONST, Number().GetValue());

            } else if (scannerSym == Token.CALL) {
                // TODO:: where to put the result of a function call?
                res = FuncCall();
                // TODO:: THis might be a hack
                // move result out of EAX
                if (res.GetValue().CompareTo("EAX") == 0) {
                    //TODO:: Something here
                }

            } else if (scannerSym == Token.OPENPAREN) {
                Next();
                res = Expression();
                if (scannerSym == Token.CLOSEPAREN) {
                    Next();
                } else {
                    // todo, evaluate when this happens
                    res = SSAWriter.LoadVariable(res, AssemblyPC);
                    AssemblyPC++;
                    //sw.WriteLine("{3}: Load {0}[{1}] R{2}", res.GetValue(), res.GetValue(), currRegister, AssemblyPC);
                    //Console.WriteLine("{3}: Load {0}[{1}] R{2}", res.GetValue(), res.GetValue(), currRegister, AssemblyPC++);
                    //res = new Result(Kind.REG, currRegister); // TODO:: Or a variable?
                }

            }
            return res;
        }

        // TODO:: Handle Arrays correctly --> getting address when needed

        private int[] GetArrayDimensions(string arrayName) {
            Symbol currSym = symbolTable[scanner.String2Id(arrayName)];
            if (currSym == null || !currSym.IsInScope(scopes.Peek()) || !currSym.IsGlobal()) {
                scanner.Error(String.Format("{1}: Array {0} not in scope", arrayName, AssemblyPC));
                return null;
            }
            return currSym.GetArrayDimensions();

        }

        // Returns result with the designator thing in a variable
        private Result Designator() {
            Result res = null, expr = null;
            VerifyToken(Token.IDENT, "Ended up at Designator but didn't parse an identifier");
            res = Ident();
            if (res == null)
                Console.WriteLine("{0}: Designator got null res", AssemblyPC);

            if (scannerSym == Token.OPENBRACKET) {
                Next(); // eat [

                res = MakeArrayReference(res);


                VerifyToken(Token.CLOSEBRACKET, "Designator: Unmatched [.... missing ]");
                Next();

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

                    // Verify function is in scope
                    if (CheckScope(scanner.String2Id(res.GetValue()))) {
                        // Parse arguments
                        if (scannerSym == Token.OPENPAREN) {
                            Next();
                            if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER) {
                                optionalArguments = new List<Result>();
                                Result currArg = Expression();
                                if (currArg == null) {
                                    Console.WriteLine("WARNING:{0}: Got Null argument in Function Argument ", AssemblyPC);
                                }
                                optionalArguments.Add(currArg);
                                // TODO:: Need to store the function's offset somewhere -- done - in Symbol Class
                                SSAWriter.StoreFunctionArgument(currArg, AssemblyPC);
                                // Set the value of the arguments
                                int ID = scanner.String2Id(currArg.GetValue());
                                if (ID != -1)
                                    UpdateSymbol(symbolTable[ID], currArg);
                                AssemblyPC+=2;
                            } else {
                                Next();
                            }
                            // TODO code that handles expression in parentheses or another function

                            while (scannerSym == Token.COMMA) {
                                Next();
                                if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER ||
                                scannerSym == Token.OPENPAREN || scannerSym == Token.CALL) {
                                    Result currArg = Expression();
                                    optionalArguments.Add(currArg);
                                // TODO:: Need to store the function's offset somewhere, i.e which argument is it
                                    SSAWriter.StoreFunctionArgument(currArg, AssemblyPC);
                                // Set the value of the arguments
                                    int ID = scanner.String2Id(currArg.GetValue());
                                    if (ID != -1)
                                        UpdateSymbol(symbolTable[ID], currArg);
                                    AssemblyPC+=2;
                                } else {
                                    scanner.Error("Ended up in optional arguments of function call and didn't parse a number, variable, comma, or expression");
                                }
                            }

                            VerifyToken(Token.CLOSEPAREN, "Ended up in arguments of function call and didn't parse a number, variable, comma, or expression");
                            Next();

                        }

                        // branch to function
                        SSAWriter.FunctionEntry(res, AssemblyPC++);
                     
                        if (symbolTable[scanner.String2Id(res.GetValue())].type == Token.FUNC) {
                            res = new Result(Kind.REG, "EAX"); // going to call return register EAX
                        }
                    } else {
                        scanner.Error(String.Format("Tried to call an undefined function : {0}", res.GetValue()));
                    }
                    break;

                case Token.OUTPUTNEWLINE:
                    Next(); // eat OutputNewLine()
                    VerifyToken(Token.OPENPAREN, "Called OutputNewLine, missing (");
                    Next(); // eat (
                    VerifyToken(Token.CLOSEPAREN, "Called OutputNewLine, missing )");
                    Next(); // eat )
                    //sw.WriteLine("WRL");
                    SSAWriter.sw.WriteLine("{0}: wln", AssemblyPC);
                    Console.WriteLine("{0}: wln", AssemblyPC++);

                    break;

                case Token.OUTPUTNUM:
                    Next(); // eat OutputNum
                    VerifyToken(Token.OPENPAREN, "OutputNum missing (");
                    Next();

                    if (scannerSym == Token.CALL)
                        res = FuncCall();
                    else if (scannerSym == Token.NUMBER || scannerSym == Token.IDENT)
                        res = Expression();
                    //sw.WriteLine("WRD " + res.GetValue());
                    SSAWriter.sw.WriteLine("{0}: write {1}", AssemblyPC,  res.GetValue());
                    Console.WriteLine("{0}: write {1} " , AssemblyPC++,  res.GetValue());

                    VerifyToken(Token.CLOSEPAREN, "OutputNum missing )");
                    Next(); // eat )

                    break;


                case Token.INPUTNUM:
                    Next(); // eat InputNum
                    VerifyToken(Token.OPENPAREN, "InputNum missing (");
                    Next(); // eat (
                    VerifyToken(Token.CLOSEPAREN, "InputNum has too many arguments or is missing )");
                    Next(); // eat ) 
                    res = new Result(Kind.REG, String.Format("({0})", AssemblyPC));
                    SSAWriter.sw.WriteLine("{0}: read", AssemblyPC);
                    Console.WriteLine("{0}: read  " , AssemblyPC++);

                    //sw.WriteLine("RDD {0}", res.regNo);
                    break;

                default:
                    scanner.Error("Ended up at FuncCall but didn't parse an identifier");
                    break;

            }

            //if (scannerSym == Token.SEMI)
              //  Next(); // eat the semicolon // if semi exists, means there's another statement coming


            // TODO:: What to return here if the function doesn't return anything?
            return res;
        }

// TODO:: NEed to print out branching instructions
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
                            //PutF1(negatedTokenString, res, new Result(Kind.CONST, "offset")); //todo, need to fix for new PutF1 stuff
                        } else {
                            scanner.Error("The relation in the if did not contain a valid conditional operator");
                        }

                        // Create the appropriate basic blocks
                        // First create the join block
                        //BasicBlock joinBlock = new BasicBlock(curBasicBlockNum++);

                        bool elseOccurred = false;
                        BasicBlock joinBlock = new BasicBlock(nextBBid++);
                        flowGraphNodes[joinBlock.blockNum] = joinBlock;
                        joinBlock.childBlocks = new List<BasicBlock>();
                        joinBlock.parentBlocks = new List<BasicBlock>();
                        joinBlock.nestingLevel = globalNestingLevel;
                        joinBlocks.Push(joinBlock);

                        globalNestingLevel++;

                        Next();
                        parentBlocks.Push(curBasicBlock);
                        BasicBlock trueBlock = new BasicBlock(nextBBid++);
                        flowGraphNodes[trueBlock.blockNum] = trueBlock;
                        trueBlock.childBlocks = new List<BasicBlock>();
                        trueBlock.parentBlocks = new List<BasicBlock>();
                        trueBlock.parentBlocks.Add(curBasicBlock);
                        //trueBlock.childBlocks.Add(joinBlock);
                        trueBlock.nestingLevel = globalNestingLevel;
                        curBasicBlock.childBlocks.Add(trueBlock);
                        curBasicBlock = trueBlock;
                        StatSequence();
                        curBasicBlock = parentBlocks.Pop();

                        BasicBlock falseBlock = null;
                        if (scannerSym == Token.ELSE) {
                            parentBlocks.Push(curBasicBlock);
                            falseBlock = new BasicBlock(nextBBid++);
                            flowGraphNodes[falseBlock.blockNum] = falseBlock;
                            falseBlock.childBlocks = new List<BasicBlock>();
                            falseBlock.parentBlocks = new List<BasicBlock>();
                            falseBlock.parentBlocks.Add(curBasicBlock);
                            falseBlock.nestingLevel = globalNestingLevel;
                            curBasicBlock.childBlocks.Add(falseBlock);
                            curBasicBlock = falseBlock;
                            Next();
                            StatSequence();
                            curBasicBlock = parentBlocks.Pop();
                            elseOccurred = true;
                        }
                        if (scannerSym == Token.FI) {
                            if (!elseOccurred) {
                                curBasicBlock.childBlocks.Add(joinBlock);
                                joinBlock.parentBlocks.Add(curBasicBlock);
                                if (trueBlock.childBlocks.Count == 0) {
                                    trueBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(trueBlock);
                                }
                            }
                            else {
                                if (curBasicBlock.childBlocks.Count < 2) {
                                    curBasicBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(curBasicBlock);
                                }
                                if (trueBlock.childBlocks.Count == 0) {
                                    trueBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(trueBlock);
                                }
                                if (falseBlock.childBlocks.Count == 0) {
                                    falseBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(falseBlock);
                                }
                            }
                            curBasicBlock = joinBlock;
                            while (joinBlocks.Peek().blockNum > joinBlock.blockNum) {
                                BasicBlock tmp = joinBlocks.Pop();
                                tmp.childBlocks.Add(joinBlock);
                                joinBlock.parentBlocks.Add(tmp);
                            }
                            globalNestingLevel--;
                            Next();
                        } else {
                            scanner.Error("In the if statement and found no token that matches either an else or a then");
                        }
                    }


                }

            } else {
                scanner.Error("Ended up at If Statement but didn't parse the if keyword");
            }
            // return new Result(); // todo, I don't have any idea what should be in this result, or if it's even needed
            return null;
        }

        private Result Ident() {
            Result res = null;
            VerifyToken(Token.IDENT, "Ended up at Identifier but didn't parse an identifier");
            // make sure thing is in scope
            if (!CheckScope(scanner.id)) return null;
         
            res = new Result(Kind.VAR, scanner.Id2String(scanner.id)); // changed to var for now to simulate output
            Next(); // eat the identifier
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
            finalResult = new Result(Kind.COND, Result.TokenToCondition(cond), AssemblyPC);


            return finalResult;
        }
        // TODO:: Should this return a result?  Like which register the assignment has been loaded into??

        private void Assignment() {
            if (scannerSym == Token.LET) {
                Next();
                Result res1 = Designator(); // thing being assigned to
                Token assignSym = Token.ERROR;
                VerifyToken(Token.BECOMES, "Assignment made without the becomes symbol");
                assignSym = scannerSym;
                Next();
                Result res2 = Expression(); // value of assignment

                // Needs to output a move instruction which for now will be done here,
                // but maybe should be moved elsewhere
                res2 = LoadIfNeeded(res2);
                SSAWriter.PutInstruction("mov", res2.GetValue(), res1.GetValue(), AssemblyPC);
                // If the thing is potentially a variable
                int ID = scanner.String2Id(res1.GetValue());
                if (ID != -1) {
                    UpdateSymbol(symbolTable[ID], null); // log the line number, current result, etc
                }
                AssemblyPC++;
            } else {
                scanner.Error("Ended up at Assignment but didn't encounter let keyword");
            }
        }


        // TODO:: Need to get function arguments from the stack
        private Result Combine(Token opCode, Result A, Result B) {
            Result newA = A, newB = B; // in case we need to change them b/c they need to be loaded
            Result res = null;
            // Check if the variable needs to be loaded from somewhere
            newA = LoadIfNeeded(A);
            newB = LoadIfNeeded(B);
           

            if (newA.type == Kind.VAR && newB.type == Kind.VAR) {
                switch (GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        res = SSAWriter.PutArithmeticRegInstruction(TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF2(TokenToInstruction(opCode), loadednewA loadedB);
            } else if (newA.type == Kind.VAR && newB.type == Kind.CONST) {
                switch (GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        res = SSAWriter.PutArithmeticImmInstruction(TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF1(TokenToInstruction(opCode) + "i", loadednewA B);

            } else if (newA.type == Kind.REG && newB.type == Kind.CONST) {
                switch (GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        res = SSAWriter.PutArithmeticImmInstruction(TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF1(TokenToInstruction(opCode) + "i", loadednewA B);

            } else if (newA.type == Kind.VAR && newB.type == Kind.REG) {
                // todo, fill in later because reg is weird right now

                switch (GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        res = SSAWriter.PutArithmeticRegInstruction(TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF2(TokenToInstruction(opCode), loadednewA B);

            } else if (newB.type == Kind.REG && newA.type == Kind.REG) {

                switch (GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        res = SSAWriter.PutArithmeticRegInstruction(TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF2(TokenToInstruction(opCode), loadednewB, loadedA);
            } else if (newB.type == Kind.VAR && newA.type == Kind.CONST) {

                switch (GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        res = SSAWriter.PutArithmeticImmInstruction(TokenToInstruction(opCode), newB, newA, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF1(TokenToInstruction(opCode) + "i", loadednewB, A);

            } else if (newB.type == Kind.VAR && newA.type == Kind.REG) {

                switch (GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        res = SSAWriter.PutArithmeticRegInstruction(TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }
                //PutF2(TokenToInstruction(opCode), loadednewB, A);

            } else if (newB.type == Kind.REG && newA.type == Kind.CONST) {
                switch (GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        res = SSAWriter.PutArithmeticImmInstruction(TokenToInstruction(opCode), newB, newA, AssemblyPC++);
                        break;
                    case OpCodeClass.COMPARE:
                        break;
                }


            }
                // This case causes issues with register allocation as it
                // is possibly not needed for constants
              else if (newA.type == Kind.CONST && newB.type == Kind.CONST) {
                  res = new Result(Kind.CONST, "");
                switch (opCode) {
                    case Token.TIMES:
                        res.SetValue(Double.Parse(newA.GetValue()) * Double.Parse(newB.GetValue()));
                        break;
                    case Token.DIV:
                        res.SetValue(Double.Parse(newA.GetValue()) / Double.Parse(newB.GetValue()));
                        break;
                    case Token.PLUS:
                        res.SetValue(Double.Parse(newA.GetValue()) + Double.Parse(newB.GetValue()));
                        break;
                    case Token.MINUS:
                        res.SetValue(Double.Parse(newA.GetValue()) - Double.Parse(newB.GetValue()));
                        break;
                }
            }

            return res;
        }

        // TODO:: Update symbol table with variable's current values

        // Loads a variable from stack/memory if needed
        // Returns argument if doesn't need to be loaded
        private Result LoadIfNeeded(Result variableToLoad) {
            Result res = variableToLoad;
            // Check if it's a function argument of this function
            if (variableToLoad.type == Kind.VAR) {
                int variID = scanner.String2Id(variableToLoad.GetValue());
                Symbol curSymbol = symbolTable[variID];

                int offset = symbolTable[variID].GetFunctionArgumentOffset();
                if (offset != 0 && curSymbol.IsInScope(scopes.Peek())) {
                    res = SSAWriter.LoadFunctionArgument(offset, variableToLoad, AssemblyPC);
                    UpdateSymbol(curSymbol, res);
                    AssemblyPC += 1;

                } else if (curSymbol.IsGlobal() && curSymbol.GetCurrentValue(scopes.Peek()) == null) {

                    // Check if it's a global that hasn't been loaded yet
                    res = SSAWriter.LoadVariable(variableToLoad, AssemblyPC);
                    UpdateSymbol(curSymbol, res);
                    AssemblyPC += 1;

                } else if (curSymbol.GetCurrentValue(scopes.Peek()) != null){
                    // Check if the variable has a value already in the scope
                    res = curSymbol.GetCurrentValue(scopes.Peek());
                }
            }

            return res;

        }
        // Updates the Symbol's value in the symbol table
        // Update's symbol's last seen line number
        // If currResult is null, will make a new result of type REG with value (AssemmblyPC)
        // Returns the newly created result, or the result passed in
        private Result UpdateSymbol(Symbol s, Result currResult) {
            Result res = currResult;
            if (s.type == Token.VAR) {
                // If no new result given, make one
                if (currResult == null)
                    res = new Result(Kind.REG, String.Format("({0})", AssemblyPC));

                s.currLineNumber = AssemblyPC;
                s.SetValue(scopes.Peek(), res);
            }
            return res;
        }

        // TODO:: Functions must have a return statement
        private void Main() {
            if (VerifyToken(Token.MAIN, "Missing Main Function")) {// find "main"
                Next();
                scopes.Push(1);
                while (scannerSym == Token.VAR | scannerSym == Token.ARR) { // variable declarations
                    VarDecl();
                }
                while (scannerSym == Token.FUNC || scannerSym == Token.PROC) {// function declarations
                    FuncDecl();
                }

                // start program
                SSAWriter.sw.WriteLine("MAIN: ");
                Console.WriteLine("MAIN: ");
                if (VerifyToken(Token.BEGIN, "Missing Opening bracket of program")) {

                    Next();
                    StatSequence();
                }
                // end program
                if (VerifyToken(Token.END, "Missing closing bracket of program")) {
                    Next();
                    if (VerifyToken(Token.PERIOD, "Unexpected end of program - missing period")) {
                        SSAWriter.sw.WriteLine("{0}: end", AssemblyPC++);
                        Console.WriteLine("{0}: end", AssemblyPC++);

                    }

                }

            }
        }


        private void FuncDecl() {
            Token funcType = scannerSym;
            if (scannerSym == Token.FUNC || scannerSym == Token.PROC) {
                Next();
            } else {
                scanner.Error("Function Declaration missing function/ procedure keyword");
            }

            VerifyToken(Token.IDENT, "Function/Procedure declaration missing a name");
            
            Symbol function;
            if (scopes.Peek() == 1) { // function is in global scope as well as local scope
                function = new Symbol(funcType, scanner.id, AssemblyPC, scopes.Peek());
                scopes.Push(nextScopeNumber++);
                // function should be able to be called from anywhere
                function.AddScope(scopes.Peek());
            } else {
                scopes.Push(nextScopeNumber++);
                function = new Symbol(funcType, scanner.id, AssemblyPC, scopes.Peek());
            }
            AddToSymbolTable(function, scopes.Peek());

            SSAWriter.sw.WriteLine("{0}:", scanner.Id2String(function.identID).ToUpper());
            Console.WriteLine("{0}:", scanner.Id2String(function.identID).ToUpper());

            Next(); // eat ident

            int currentOffset = -4;
            if (scannerSym == Token.OPENPAREN) {
                Next(); // eat openParen
                // Formal Parameter
                if (scannerSym == Token.IDENT) {
                    //                    Ident();
                    Next(); // eat ident
                    Symbol sym = new Symbol(Token.VAR, scanner.id, AssemblyPC, scopes.Peek());
                    sym.SetArgumentOffset(currentOffset);
                    currentOffset -= 4;
                    AddToSymbolTable(sym, scopes.Peek());

                    while (scannerSym == Token.COMMA) {
                        Next();
                        //                      Ident();
                        Next(); // eat Ident
                        sym = new Symbol(Token.VAR, scanner.id, AssemblyPC, scopes.Peek());
                        sym.SetArgumentOffset(currentOffset);
                        currentOffset -= 4;
                        AddToSymbolTable(sym, scopes.Peek());
                    }
                }
                VerifyToken(Token.CLOSEPAREN, "Function Declaration missing a closing parenthesis");
                Next(); // eat closeparen
            }


            VerifyToken(Token.SEMI, "Function Declaration missing a semicolon");
            Next(); // eat semi
            // Func Body
            // Variable declarations
            while (scannerSym == Token.VAR || scannerSym == Token.ARR) {
                VarDecl();
            }
            VerifyToken(Token.BEGIN, "Function body missing open bracket");
            Next(); // eat {
            // function statements
            StatSequence();

            VerifyToken(Token.END, "Function Body missing closing bracket"); //end function body
            Next(); // eat }
            VerifyToken(Token.SEMI, "Function declaration missing semicolon"); // end function declaration
            Next(); // eat ;

            SSAWriter.LeaveFunction(AssemblyPC);
            AssemblyPC++;
            // current scope ends
            scopes.Pop();
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
                case Token.ELSE:
                case Token.FI:
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
            VerifyToken(Token.DO, "No do keyword after the relation in while statement");
            Next(); // eat do
            if (res.condition != CondOp.ERR) {
                string negatedTokenString = TokenToInstruction(NegatedConditional(res.condition));
                //PutF1(negatedTokenString, res, new Result(Kind.CONST, "offset")); //todo, fix for new PutF1 stuff

                BasicBlock loopHeaderBlock = new BasicBlock(nextBBid++);
                flowGraphNodes[loopHeaderBlock.blockNum] = loopHeaderBlock;
                loopHeaderBlock.childBlocks = new List<BasicBlock>();
                loopHeaderBlock.parentBlocks = new List<BasicBlock>();
                loopHeaderBlock.nestingLevel = globalNestingLevel;
                loopHeaderBlock.parentBlocks.Add(curBasicBlock);
                curBasicBlock.childBlocks.Add(loopHeaderBlock);
                curBasicBlock = loopHeaderBlock;
                loopHeaderBlocks.Push(curBasicBlock);
                globalNestingLevel++;
                
                BasicBlock loopBodyBlock = new BasicBlock(nextBBid++);
                flowGraphNodes[loopBodyBlock.blockNum] = loopBodyBlock;
                loopBodyBlock.childBlocks = new List<BasicBlock>();
                loopBodyBlock.parentBlocks = new List<BasicBlock>();
                loopBodyBlock.nestingLevel = globalNestingLevel;
                loopBodyBlock.parentBlocks.Add(curBasicBlock);
                loopBodyBlock.childBlocks.Add(curBasicBlock);
                curBasicBlock.childBlocks.Add(loopBodyBlock);
                curBasicBlock = loopBodyBlock;
                
                StatSequence();

                globalNestingLevel--;
                curBasicBlock = loopHeaderBlocks.Pop();
                BasicBlock loopFollowBlock = new BasicBlock(nextBBid++);
                flowGraphNodes[loopFollowBlock.blockNum] = loopFollowBlock;
                loopFollowBlock.childBlocks = new List<BasicBlock>();
                loopFollowBlock.parentBlocks = new List<BasicBlock>();
                loopFollowBlock.nestingLevel = globalNestingLevel;
                loopFollowBlock.parentBlocks.Add(curBasicBlock);
                curBasicBlock.childBlocks.Add(loopFollowBlock);
                curBasicBlock = loopFollowBlock;
                

                // todo, here we will need a branch to loop header
                VerifyToken(Token.OD, "The while loop was not properly closed with the od keyword");
                Next(); //eat od
            } else {
                scanner.Error("The relation in the if did not contain a valid conditional operator");
            }
            //return new Result(); // todo, don't know what this should be either
            return null;
        }


// TODO:: Need to move the return value OUT of EAX
        private Result ReturnStatement() {
            VerifyToken(Token.RETURN, "Missing return keyword");
            Next();
            //todo:: check if return expression necessary 

            Result res = null;
            // if (curFunc.type == Token.FUNC) {
            // if (!(scannerSym == Token.IDENT || scannerSym == Token.NUMBER))
            //    scanner.Error("Function should return a value");
            if (scannerSym != Token.END) {

                res = Expression();
                if (res == null)
                    Console.WriteLine("{0}: Expression got null res", AssemblyPC);
                res = LoadIfNeeded(res);
                SSAWriter.ReturnFromFunction(res, AssemblyPC);
                AssemblyPC++;
               // scope may not actually change -- early return statement
                return res;

            } else {
                SSAWriter.ReturnFromProcedure(AssemblyPC);
                AssemblyPC++;
                return null;
            }
        }



        private void VarDecl() {
            if (scannerSym == Token.VAR) {
                Next(); // eat "var"
                VerifyToken(Token.IDENT, "Variable declaration missing variable name");
                AddToSymbolTable(new Symbol(Token.VAR, scanner.id, AssemblyPC, scopes.Peek()), scopes.Peek());

                Next(); // eat the ident 

                while (scannerSym == Token.COMMA) {
                    Next(); // eat the comma
                    VerifyToken(Token.IDENT, "Dangling comma in variable declaration");
                    AddToSymbolTable(new Symbol(Token.VAR, scanner.id, AssemblyPC, scopes.Peek()), scopes.Peek());

                    Next(); // eat the ident

                }

            }
            if (scannerSym == Token.ARR) {
                Next(); // eat  "array"
                List<int> dims = new List<int>();
                Result res;

                do {
                    VerifyToken(Token.OPENBRACKET, "Array declaration missing [");
                    Next(); // eat [
                    res = Number();

                    dims.Add(Int32.Parse(res.GetValue()));
                    VerifyToken(Token.CLOSEBRACKET, "Array declaraion missing ]");
                    Next();


                } while (scannerSym != Token.IDENT);

                VerifyToken(Token.IDENT, "Array declaration missing name");
                AddToSymbolTable(new Symbol(Token.ARR, scanner.id, AssemblyPC, dims.ToArray(), scopes.Peek()), scopes.Peek());

                Next(); // eat ident

            }

            VerifyToken(Token.SEMI, "Missing semicolon at end of variable declaration");
            Next(); // eat semicolon

        }


        private void ConditionalJumpForward(Result x) {

        }

        //////////////////////////////////////////////
        // Utility Functions
        //////////////////////////////////////////////

        // Checks if scannerSym == t
        // if not, pushes errMsg to Scanner and exits
        private bool VerifyToken(Token t, string errMsg) {
            if (scannerSym != t)
                scanner.Error(errMsg); // will exit program
            return true;
        }

        // Verifies the identifier is indeed in scope
        private bool CheckScope(int identID) {

            if (identID == -1) {
                Console.WriteLine("{0}: Identifier {1} doesn't exist", AssemblyPC, identID);
                return false;
            }
            if (identID > symbolTable.Count) {
                Console.WriteLine("{0}: Unknown symbol.", AssemblyPC);
                return false;
            }
            Symbol curSym = symbolTable[identID];
            return curSym.IsInScope(scopes.Peek()) || curSym.IsGlobal();

        }

        // Adds the symbol to the symbol table, if its ID is unique
        // else, adds the scope to the symbol
        private void AddToSymbolTable(Symbol s, int scope) {

            if (s.identID < symbolTable.Count && symbolTable[s.identID] != null) {
                // already have a symbol with this name
                symbolTable[s.identID].AddScope(scope);
            } else {
                symbolTable.Insert(scanner.id, s);
            }
        }

        // Call when we want to keep compiling after an error
// Scans until we hit the next line of the file
        private void AbortLine() {
            int currPC = scanner.PC;
            while (scanner.PC == currPC) {
                Next();
            }


        }

        private Result MakeArrayReference(Result res) {
            // find dimensions of array -- > requires symbol table look up
            int[] arrDims = GetArrayDimensions(res.GetValue());

            Result[] indexer = new Result[arrDims.Length];


            for (int i = 0; i < arrDims.Length; i++) {
                VerifyToken(Token.OPENBRACKET, "Array missing open bracket");
                Next();
                // evaluate indices
                indexer[i] = Expression();
                VerifyToken(Token.CLOSEBRACKET, "Array missing closing bracket");
                Next();
            }


            return new Result(Kind.ARR, res.GetValue(), indexer);

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




    }
}
