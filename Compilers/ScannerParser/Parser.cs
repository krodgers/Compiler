﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        OUTPUTNUM, INPUTNUM, OUTPUTNEWLINE,
        STORE, LOAD, BRANCH, CHECK, PHI
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
        private BasicBlock entryBlock;
        private BasicBlock exitBlock;
        public BasicBlock curBasicBlock;
        private BasicBlock curJoinBlock;
        private int nextBBid; // next available basic block id
        private Stack<BasicBlock> parentBlocks;

        private InstructionManager instructionManager;

        private Stack<int> scopes; // track the current scope
        private int nextScopeNumber; // next assignable scope number
        private List<Symbol> symbolTable; // all the symbols! // indexed by symbol id
        private int nextGlobalOffset;

        private Stack<BasicBlock> joinBlocks;
        public Dictionary<int, BasicBlock> joinParentBlocks;
        private Stack<BasicBlock> loopHeaderBlocks;
        private StackList joinBlockListForPhis;
        private int globalNestingLevel;
        BasicBlock trueBlock, falseBlock, joinBlock;
        Dictionary<int, BasicBlock> flowGraphNodes;

        private List<int> loopCounters;
        private int oldAssemblyPC;

        private Dotifier dotty;

        private int parserTestVar;



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
            nextGlobalOffset = 0;

            joinBlocks = new Stack<BasicBlock>();
            loopHeaderBlocks = new Stack<BasicBlock>();
            joinBlockListForPhis = new StackList();

            instructionManager = new InstructionManager(symbolTable);

            globalNestingLevel = 0;
            trueBlock = falseBlock = joinBlock = null;
            flowGraphNodes = new Dictionary<int, BasicBlock>();
            loopCounters = new List<int>();

            joinParentBlocks = new Dictionary<int, BasicBlock>();


        }

        // Sets initial state of parser
        // for now
        private void Init() {
            Next(); // set first symbol

        }

        ~Parser() {
            try {

                if (SSAWriter.sw != null)
                    SSAWriter.sw.Dispose(); // closes sw and fs
            }
            catch (Exception) {
                Console.WriteLine("File may not be closed properly....");
            }

        }

        private void Next() {
            scannerSym = scanner.GetSym();
            //HandleToken(); TODO this needs to be moved somewhere else because causes problems in next
        }

        public BasicBlock StartFirstPass(ref BasicBlock start)  {

            Main();

            dotty = new Dotifier(flowGraphNodes);
            dotty.WriteAllBlocksToDot("if_while_test", curBasicBlock.blockNum);
            dotty.WriteDominatorTree("if_while_test");

            start = entryBlock;

            return entryBlock;

        }

  public BasicBlock StartFirstPass() {

            Main();

            dotty = new Dotifier(flowGraphNodes);
            dotty.WriteAllBlocksToDot("if_while_test", curBasicBlock.blockNum);
            dotty.WriteDominatorTree("if_while_test");

            return entryBlock;

        }


        private void Error() {
            string msg;
            msg = string.Format("Unexpected {0} {1}, syntax error", scannerSym.ToString(), scanner.Id2String(scanner.id));
            scanner.Error(msg);
        }

        private void Error(String msg) {
            scanner.Error(msg);
        }


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
                res = Number();
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
                    Console.WriteLine("It happened");
                    if (res.type == Kind.VAR || res.type == Kind.REG) {

                        oldAssemblyPC = AssemblyPC;
                        //instructionManager.PutLoadInstruction(res, AssemblyPC);
                        //  res = SSAWriter.LoadVariable(res, AssemblyPC);
//                        instructionManager.GetInstruction(AssemblyPC).myResult = res;

                        res = LoadIfNeeded(res);
                        AssemblyPC++;
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                    } else if (res.type == Kind.ARR) {
                        oldAssemblyPC = AssemblyPC;
                        res = SSAWriter.LoadArrayElement(res, GetArrayDimensions(res.GetValue()), res.GetArrayIndices(), AssemblyPC);
                        AssemblyPC = res.lineNumber + 1;
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                    }
                }

            }
            return res;
        }

        // TODO:: Handle Arrays correctly --> getting address when needed

        private int[] GetArrayDimensions(string arrayName) {
            ArraySymbol currSym = (ArraySymbol)symbolTable[scanner.String2Id(arrayName)];
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
                //  Next(); // eat [
                res = MakeArrayReference(res, false);
                //                VerifyToken(Token.CLOSEBRACKET, "Designator: Unmatched [.... missing ]");
                //  Next();

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
            FunctionSymbol currentFunction;
            List<Result> optionalArguments = null;

            VerifyToken(Token.CALL, "Ended up at FuncCall but didn't recieve the keyword call");
            Next();


            BasicBlock callBlock = new BasicBlock(nextBBid++);
            flowGraphNodes[callBlock.blockNum] = callBlock;
            callBlock.childBlocks = new List<BasicBlock>();
            callBlock.parentBlocks = new List<BasicBlock>();
            callBlock.blockType = BasicBlock.BlockType.FUNCTION_CALL;
            callBlock.scopeNumber = scopes.Peek();

            BasicBlock afterBlock = new BasicBlock(nextBBid++);
            flowGraphNodes[afterBlock.blockNum] = afterBlock;
            afterBlock.childBlocks = new List<BasicBlock>();
            afterBlock.parentBlocks = new List<BasicBlock>();
            afterBlock.blockType = BasicBlock.BlockType.STANDARD;
            afterBlock.scopeNumber = scopes.Peek(); // todo, it seems to me that the scope should switch when we call a function, maybe? Idon't know, too tired
            callBlock.childBlocks.Add(afterBlock);
            afterBlock.parentBlocks.Add(callBlock);


            switch (scannerSym) {
                case Token.IDENT:
                    callBlock.blockLabel = scanner.Id2String(scanner.id);
                    res = Ident();

                    // Verify function is in scope
                    if (CheckScope(scanner.String2Id(res.GetValue()))) {
                        currentFunction = (FunctionSymbol)symbolTable[scanner.String2Id(res.GetValue())];
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


                                oldAssemblyPC = AssemblyPC;


                                currArg = LoadIfNeeded(currArg); // get the latest value 


                                instructionManager.PutFunctionArgument(currArg, AssemblyPC);
                        instructionManager.GetInstruction(AssemblyPC).myResult = currArg;

                                SSAWriter.StoreFunctionArgument(currArg, AssemblyPC);
                                // Set the value of the arguments
                                int ID = scanner.String2Id(currArg.GetValue());
                                if (ID != -1)
                                    UpdateSymbol(symbolTable[ID], currArg);
                                AssemblyPC += 2;
                                IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                            }
                            else {
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

                                    currArg = LoadIfNeeded(currArg); // get the latest value 


                                    oldAssemblyPC = AssemblyPC;
                                    instructionManager.PutFunctionArgument(currArg, AssemblyPC);
                                    instructionManager.GetInstruction(AssemblyPC).myResult = currArg;

                                    SSAWriter.StoreFunctionArgument(currArg, AssemblyPC);
                                    // Set the value of the arguments
                                    int ID = scanner.String2Id(currArg.GetValue());
                                    if (ID != -1)
                                        UpdateSymbol(symbolTable[ID], currArg);
                                    AssemblyPC += 2;
                                    IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                                }
                                else {
                                    scanner.Error("Ended up in optional arguments of function call and didn't parse a number, variable, comma, or expression");
                                }
                            }

                            VerifyToken(Token.CLOSEPAREN, "Ended up in arguments of function call and didn't parse a number, variable, comma, or expression");
                            Next();
                            if (optionalArguments.Count != currentFunction.numberOfFormalParamters)
                                Error(String.Format("{0}: Wrong number of parameters for {1}", AssemblyPC, scanner.Id2String(currentFunction.identID)));

                        }


                        // branch to function
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutFunctionEntry(res, AssemblyPC);
                        instructionManager.GetInstruction(AssemblyPC).myResult = res;

                        SSAWriter.FunctionEntry(res, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        if (symbolTable[scanner.String2Id(res.GetValue())].type == Token.FUNC) {
                            res = new Result(Kind.REG, "EAX"); // going to call return register EAX
                        }

                        curBasicBlock.childBlocks.Add(callBlock);
                        callBlock.parentBlocks.Add(curBasicBlock);

                        curBasicBlock = afterBlock;
                        instructionManager.setCurrentBlock(afterBlock);
                    } else {
                        scanner.Error(String.Format("Tried to call an undefined function : {0}", res.GetValue()));
                    }
                    break;

                case Token.OUTPUTNEWLINE:
                    callBlock.blockLabel = "OutputNewLine";
                    Next(); // eat OutputNewLine()
                    VerifyToken(Token.OPENPAREN, "Called OutputNewLine, missing (");
                    Next(); // eat (
                    VerifyToken(Token.CLOSEPAREN, "Called OutputNewLine, missing )");
                    Next(); // eat )
                    //sw.WriteLine("WRL");
                    instructionManager.PutBasicInstruction(Token.OUTPUTNEWLINE, new Result(Kind.CONST, ""), new Result(Kind.CONST, ""), AssemblyPC);
                    oldAssemblyPC = AssemblyPC;
                    SSAWriter.sw.WriteLine("{0}: wln", AssemblyPC);
                    Console.WriteLine("{0}: wln", AssemblyPC++);
                    IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                    curBasicBlock.childBlocks.Add(callBlock);
                    callBlock.parentBlocks.Add(curBasicBlock);

                    curBasicBlock = afterBlock;
                    instructionManager.setCurrentBlock(afterBlock);

                    break;

                case Token.OUTPUTNUM:
                    callBlock.blockLabel = "OutputNum";
                    Next(); // eat OutputNum
                    VerifyToken(Token.OPENPAREN, "OutputNum missing (");
                    Next();

                    if (scannerSym == Token.CALL)
                        res = FuncCall();
                    else if (scannerSym == Token.NUMBER || scannerSym == Token.IDENT)
                        res = Expression();
                    //sw.WriteLine("WRD " + res.GetValue());
                    res = LoadIfNeeded(res);
                    instructionManager.PutBasicInstruction(Token.OUTPUTNUM, res, new Result(Kind.CONST, ""), AssemblyPC);
                    oldAssemblyPC = AssemblyPC;
                    SSAWriter.sw.WriteLine("{0}: write {1}", AssemblyPC, res.GetValue());
                    Console.WriteLine("{0}: write {1} ", AssemblyPC++, res.GetValue());
                    IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                    VerifyToken(Token.CLOSEPAREN, "OutputNum missing )");
                    Next(); // eat )

                    curBasicBlock.childBlocks.Add(callBlock);
                    callBlock.parentBlocks.Add(curBasicBlock);
                    curBasicBlock = afterBlock;
                    instructionManager.setCurrentBlock(afterBlock);

                    break;


                case Token.INPUTNUM:
                    callBlock.blockLabel = "InputNum";
                    Next(); // eat InputNum
                    VerifyToken(Token.OPENPAREN, "InputNum missing (");
                    Next(); // eat (
                    VerifyToken(Token.CLOSEPAREN, "InputNum has too many arguments or is missing )");
                    Next(); // eat ) 
                    oldAssemblyPC = AssemblyPC;
                    res = new Result(Kind.REG, String.Format("({0})", AssemblyPC));
                    instructionManager.PutBasicInstruction(Token.INPUTNUM, res, new Result(Kind.CONST, ""), AssemblyPC);
                        instructionManager.GetInstruction(AssemblyPC).myResult = res;

                    SSAWriter.sw.WriteLine("{0}: read", AssemblyPC);
                    Console.WriteLine("{0}: read  ", AssemblyPC++);
                    IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                    curBasicBlock.childBlocks.Add(callBlock);
                    callBlock.parentBlocks.Add(curBasicBlock);
                    curBasicBlock = afterBlock;
                    instructionManager.setCurrentBlock(afterBlock);

                    //sw.WriteLine("RDD {0}", res.regNo);
                    break;

                default:
                    scanner.Error("Ended up at FuncCall but didn't parse an identifier");
                    break;

            }

            // TODO:: What to return here if the function doesn't return anything?
            return res;
        }

        // TODO:: NEed to print out branching instructions
        // TODO:: My idea is to add all of the instructions to the basic block as we go along
        // When we get to a place where we can know what branching address will be, we write out the
        // entire block using SSAWriter.WriteControlFlowBlock(curBlock, fixUpAddr)
        // Find out fix up addr by subtractin the current Assembly PC from the first line number in the block

        private Result IfStatement() {

            Result res = null;
            if (scannerSym == Token.IF) {
                Next();
                if (scannerSym == Token.IDENT || scannerSym == Token.NUMBER
                    || scannerSym == Token.OPENPAREN || scannerSym == Token.CALL) {
                    res = Relation();
                    if (VerifyToken(Token.THEN, "The then keyword did not follow the relation in if statement")) {
                        if (res.condition != CondOp.ERR) {
                            //string negatedTokenString = Utilities.TokenToInstruction(Utilities.NegatedConditional(res.condition));
                            //PutF1(negatedTokenString, res, new Result(Kind.CONST, "offset")); //todo, need to fix for new PutF1 stuff
                        } else {
                            scanner.Error("The relation in the if did not contain a valid conditional operator");
                        }

                        // Create the appropriate basic blocks
                        // First create the join block
                        //BasicBlock joinBlock = new BasicBlock(curBasicBlockNum++);


                        bool elseOccurred = false;
                        BasicBlock joinBlock = new BasicBlock(nextBBid++);
                        joinBlock.blockType = BasicBlock.BlockType.JOIN;
                        joinBlock.blockLabel = GetLabel(joinBlock, curBasicBlock.blockNum);
                        flowGraphNodes[joinBlock.blockNum] = joinBlock;
                        joinBlock.childBlocks = new List<BasicBlock>();
                        joinBlock.parentBlocks = new List<BasicBlock>();
                        joinBlock.nestingLevel = globalNestingLevel;
                        joinBlock.dominatingBlock = curBasicBlock;
                        joinParentBlocks[joinBlock.blockNum] = curBasicBlock;
                        curBasicBlock.blocksIDominate.Add(joinBlock);
                        joinBlocks.Push(joinBlock);
                        joinBlockListForPhis.Push(joinBlock);

                        curJoinBlock = joinBlock;
                        Debug.WriteLine("cur join: {0}", curJoinBlock.blockNum);

                        // todo, I think the current block needs to be pushed here, not the joinBlock
                        // but need to make sure that the join block gets the instructions it should


                        globalNestingLevel++;

                        Next();
                        parentBlocks.Push(curBasicBlock);
                        BasicBlock trueBlock = new BasicBlock(nextBBid++);
                        trueBlock.blockType = BasicBlock.BlockType.TRUE;
                        trueBlock.blockLabel = GetLabel(trueBlock, curBasicBlock.blockNum);
                        flowGraphNodes[trueBlock.blockNum] = trueBlock;
                        trueBlock.childBlocks = new List<BasicBlock>();
                        trueBlock.parentBlocks = new List<BasicBlock>();
                        trueBlock.parentBlocks.Add(curBasicBlock);
                        //trueBlock.childBlocks.Add(joinBlock);
                        trueBlock.nestingLevel = globalNestingLevel;
                        trueBlock.dominatingBlock = curBasicBlock;
                        curBasicBlock.blocksIDominate.Add(trueBlock);
                        curBasicBlock.childBlocks.Add(trueBlock);
                        curBasicBlock = trueBlock;
                        instructionManager.setCurrentBlock(curBasicBlock);
                        curBasicBlock.scopeNumber = scopes.Peek();

                        StatSequence();

                        Debug.WriteLine("cur join: {0}", curJoinBlock.blockNum);

                        if (curBasicBlock.blockType == BasicBlock.BlockType.FOLLOW) {
                            curBasicBlock.childBlocks.Add(joinBlock);
                            // Insert the branch that will get us back to the header at the end of the loop
                            instructionManager.PutUnconditionalBranch(Token.BRANCH, joinBlock.blockNum, AssemblyPC);
                            SSAWriter.PutUnconditionalBranch(Utilities.TokenToInstruction(Token.BRANCH), joinBlock.blockNum.ToString(), AssemblyPC++);

                        }

                        curBasicBlock = parentBlocks.Pop();
                        instructionManager.setCurrentBlock(curBasicBlock);

                        BasicBlock falseBlock = null;
                        if (scannerSym == Token.ELSE) {

                            // Restore all of the original values for the variables that
                            // were modified in the true branch

                            foreach (KeyValuePair<int, PhiInstruction> phi in curJoinBlock.phiInstructions) {

                                UpdateSymbol(symbolTable[phi.Value.symTableID], phi.Value.originalVarVal);
                            }

                            parentBlocks.Push(curBasicBlock);
                            falseBlock = new BasicBlock(nextBBid++);
                            falseBlock.blockType = BasicBlock.BlockType.FALSE;
                            falseBlock.blockLabel = GetLabel(falseBlock, curBasicBlock.blockNum);
                            flowGraphNodes[falseBlock.blockNum] = falseBlock;
                            falseBlock.childBlocks = new List<BasicBlock>();
                            falseBlock.parentBlocks = new List<BasicBlock>();
                            falseBlock.parentBlocks.Add(curBasicBlock);
                            falseBlock.nestingLevel = globalNestingLevel;
                            falseBlock.dominatingBlock = curBasicBlock;
                            curBasicBlock.blocksIDominate.Add(falseBlock);
                            curBasicBlock.childBlocks.Add(falseBlock);

                            // Fix up the branch at the end of the if block so that
                            // it branches to the else block, given that an else block
                            // exists

                            // Get to the last instruction of the if block to find the branch
                            Instruction ifBranchInstruction = curBasicBlock.firstInstruction;
                            while (ifBranchInstruction.next != null)
                                ifBranchInstruction = ifBranchInstruction.next;
                            ifBranchInstruction.secondOperand = falseBlock.blockNum.ToString();


                            curBasicBlock = falseBlock;
                            instructionManager.setCurrentBlock(curBasicBlock);
                            curBasicBlock.scopeNumber = scopes.Peek();
                            Next();

                            StatSequence();

                            if (curBasicBlock.blockType == BasicBlock.BlockType.FOLLOW) {
                                curBasicBlock.childBlocks.Add(joinBlock);
                                // Insert the branch that will get us back to the header at the end of the loop
                                instructionManager.PutUnconditionalBranch(Token.BRANCH, joinBlock.blockNum, AssemblyPC);
                                SSAWriter.PutUnconditionalBranch(Utilities.TokenToInstruction(Token.BRANCH), joinBlock.blockNum.ToString(), AssemblyPC++);

                                // todo, need to restore all of the variables that were changed in this block....I think

                            }

                            curBasicBlock = parentBlocks.Pop();
                            instructionManager.setCurrentBlock(curBasicBlock);
                            elseOccurred = true;
                        }
                        if (scannerSym == Token.FI) {
                            if (!elseOccurred) {
                                curBasicBlock.childBlocks.Add(joinBlock);
                                joinBlock.parentBlocks.Add(curBasicBlock);

                                // Get to the last instruction of the parent block for this join block
                                // so that we can fix up the branch there
                                Instruction parentsBranchInstruction = curBasicBlock.firstInstruction;
                                while (parentsBranchInstruction.next != null)
                                    parentsBranchInstruction = parentsBranchInstruction.next;
                                parentsBranchInstruction.secondOperand = joinBlock.blockNum.ToString();

                                joinBlock.joinPredecessorInstructionCount += trueBlock.instructionCount;
                                if (trueBlock.childBlocks.Count == 0) {
                                    trueBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(trueBlock);

                                    // Insert a branch instruction at the end of the true block in
                                    // the case that there was no false block, so that the code will jump to the join block
                                    /* todo, this might not be necessary, but putting it here for now
                                     * in case there is a join block that doesn't contain any code
                                     * Will need to check for branches that branch to the next line
                                     * and get rid of them in optimization
                                    */

                                    // First set the instruction manager to the true block
                                    instructionManager.setCurrentBlock(trueBlock);

                                    // Insert the branch that will get us back to the header at the end of the loop
                                    instructionManager.PutUnconditionalBranch(Token.BRANCH, joinBlock.blockNum, AssemblyPC);
                                    SSAWriter.PutUnconditionalBranch(Utilities.TokenToInstruction(Token.BRANCH), joinBlock.blockNum.ToString(), AssemblyPC++);

                                    // Switch the instruction manager back to the current block
                                    instructionManager.setCurrentBlock(curBasicBlock);
                                }
                            }
                            else {
                                joinBlock.joinPredecessorInstructionCount += trueBlock.instructionCount;
                                joinBlock.joinPredecessorInstructionCount += falseBlock.instructionCount;
                                if (curBasicBlock.childBlocks.Count < 2) {
                                    curBasicBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(curBasicBlock);

                                    // Get to the last instruction of the parent block for this join block
                                    // so that we can fix up the branch there
                                    Instruction parentsBranchInstruction = curBasicBlock.firstInstruction;
                                    while (parentsBranchInstruction.next != null)
                                        parentsBranchInstruction = parentsBranchInstruction.next;
                                    parentsBranchInstruction.secondOperand = joinBlock.blockNum.ToString();
                                }
                                if (trueBlock.childBlocks.Count == 0) {
                                    trueBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(trueBlock);


                                    // Insert a branch instruction at the end of the true block so
                                    // that the code will jump over the else statement's code

                                    // First set the instruction manager to the true block
                                    instructionManager.setCurrentBlock(trueBlock);

                                    // Insert the branch that will get us back to the header at the end of the loop
                                    instructionManager.PutUnconditionalBranch(Token.BRANCH, joinBlock.blockNum, AssemblyPC);
                                    SSAWriter.PutUnconditionalBranch(Utilities.TokenToInstruction(Token.BRANCH), joinBlock.blockNum.ToString(), AssemblyPC++);

                                    // Switch the instruction manager back to the current block
                                    instructionManager.setCurrentBlock(curBasicBlock);


                                }
                                if (falseBlock.childBlocks.Count == 0) {
                                    falseBlock.childBlocks.Add(joinBlock);
                                    joinBlock.parentBlocks.Add(falseBlock);

                                    // Insert a branch instruction at the end of the false block so
                                    // that the code will jump to the join block
                                    /* todo, this might not be necessary, but putting it here for now
                                     * in case there is a join block that doesn't contain any code
                                     * Will need to check for branches that branch to the next line
                                     * and get rid of them in optimization
                                    */

                                    // First set the instruction manager to the true block
                                    instructionManager.setCurrentBlock(falseBlock);


                                    instructionManager.PutUnconditionalBranch(Token.BRANCH, joinBlock.blockNum, AssemblyPC);
                                    SSAWriter.PutUnconditionalBranch(Utilities.TokenToInstruction(Token.BRANCH), joinBlock.blockNum.ToString(), AssemblyPC++);

                                    // Switch the instruction manager back to the current block
                                    instructionManager.setCurrentBlock(curBasicBlock);
                                }
                            }
                            curBasicBlock.instructionCount += joinBlock.joinPredecessorInstructionCount;
                            joinBlock.joinPredecessorInstructionCount = curBasicBlock.instructionCount;
                            curBasicBlock = joinBlock;
                            instructionManager.setCurrentBlock(curBasicBlock);
                            curBasicBlock.scopeNumber = scopes.Peek();


                            // Get rid of duplicate phis
                            instructionManager.RemoveUnnecessaryPhis(curBasicBlock, ref AssemblyPC);


                            // Commit all of the phis
                            if (joinBlockListForPhis.GetOuterJoin() != null) {
                                Dictionary<int, PhiInstruction> phisToCommit =
                                    instructionManager.CommitOuterPhi(ref AssemblyPC, curJoinBlock,
                                        joinBlockListForPhis.GetOuterJoin());

                                foreach (KeyValuePair<int, PhiInstruction> phi in curJoinBlock.phiInstructions) {
                                    UpdateSymbol(symbolTable[phi.Value.symTableID],
                                        new Result(Kind.REG, String.Format("({0})", phi.Value.instructionNum)));
                                }

                            }
                            else {
                                foreach (KeyValuePair<int, PhiInstruction> phi in curJoinBlock.phiInstructions) {
                                    UpdateSymbol(symbolTable[phi.Value.symTableID],
                                        new Result(Kind.REG, String.Format("({0})", phi.Value.instructionNum)));
                                }
                            }

                            joinBlockListForPhis.Pop();
                            while (joinBlocks.Peek().blockNum > joinBlock.blockNum) {
                                BasicBlock tmp = joinBlocks.Pop();

                                if (tmp.childBlocks.Count <= 0) {
                                    tmp.childBlocks.Add(joinBlock);

                                    // If we are still within a conditional, we need to branch from the end
                                    // of the join block to its child joinBlock

                                    // First set the instruction manager to the parent join block
                                    instructionManager.setCurrentBlock(tmp);

                                    // Insert the branch that will get us back to the header at the end of the loop
                                    instructionManager.PutUnconditionalBranch(Token.BRANCH, joinBlock.blockNum,
                                        AssemblyPC);
                                    SSAWriter.PutUnconditionalBranch(Utilities.TokenToInstruction(Token.BRANCH), joinBlock.blockNum.ToString(), AssemblyPC++);

                                    // Switch the instruction manager back to the current block
                                    instructionManager.setCurrentBlock(curBasicBlock);

                                    joinBlock.parentBlocks.Add(tmp);
                                }
                            }
                            globalNestingLevel--;
                            Next();
                            if (joinBlockListForPhis.Count > 0)
                                curJoinBlock = joinBlockListForPhis.Peek();
                        }
                        else {
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

        private Result WhileStatement() {
            // todo, with the while statement and the if statements, fixuploc and such need
            // to be implemented

            // todo, for phis in the while loop, we get into the body block of the while and
            // create a phi for every assignment that occurs, once we have created all of the
            // phis that are necessary, we go through the use chains for every variable that
            // we created a phi for and then we update the refereces for that variable, we
            // can do t  

            VerifyToken(Token.WHILE, "Got to while statement without seeing the while keyword");
            Next(); // eat while



            // Create the header block for this loop
            BasicBlock loopHeaderBlock = new BasicBlock(nextBBid++);
            flowGraphNodes[loopHeaderBlock.blockNum] = loopHeaderBlock;
            loopHeaderBlock.childBlocks = new List<BasicBlock>();
            loopHeaderBlock.parentBlocks = new List<BasicBlock>();
            loopHeaderBlock.nestingLevel = globalNestingLevel;
            loopHeaderBlock.dominatingBlock = curBasicBlock;
            curBasicBlock.blocksIDominate.Add(loopHeaderBlock);
            loopHeaderBlock.blockType = BasicBlock.BlockType.LOOP_HEADER;
            loopHeaderBlock.parentBlocks.Add(curBasicBlock);
            curBasicBlock.childBlocks.Add(loopHeaderBlock);
            curBasicBlock = loopHeaderBlock;
            curJoinBlock = loopHeaderBlock;
            Debug.WriteLine("cur join: {0}", curJoinBlock.blockNum);
            instructionManager.setCurrentBlock(curBasicBlock);
            curBasicBlock.scopeNumber = scopes.Peek();
            loopHeaderBlocks.Push(curBasicBlock);
            joinBlockListForPhis.Push(curBasicBlock);
            globalNestingLevel++;

            Result res = Relation();
            // After relation is called, the branch location will be wrong, so we must reset it at some point

            // Pop the header off the scope stack as the only code that will be added to it is the
            // unconditional branch and that doesn't depend on scope

            VerifyToken(Token.DO, "No do keyword after the relation in while statement");
            Next(); // eat do
            if (res.condition != CondOp.ERR) {
                //string negatedTokenString = Utilities.TokenToInstruction(Utilities.NegatedConditional(res.condition));
                //PutF1(negatedTokenString, res, new Result(Kind.CONST, "offset")); //todo, fix for new PutF1 stuff

                /* todo, for the case when a loop immediately follows another loop with no instructions
                   in between, we need to delete the empty block from the tree (curBasicBlock). Make sure that its
                 * block num is exaclty one less than this one and that it is not the root basic block
                */


                BasicBlock loopBodyBlock = new BasicBlock(nextBBid++);
                flowGraphNodes[loopBodyBlock.blockNum] = loopBodyBlock;
                loopBodyBlock.childBlocks = new List<BasicBlock>();
                loopBodyBlock.parentBlocks = new List<BasicBlock>();
                loopBodyBlock.nestingLevel = globalNestingLevel;
                loopBodyBlock.dominatingBlock = loopHeaderBlock;
                loopHeaderBlock.blocksIDominate.Add(loopBodyBlock);
                loopBodyBlock.blockType = BasicBlock.BlockType.LOOP_BODY;
                loopBodyBlock.parentBlocks.Add(curBasicBlock);
                curBasicBlock.childBlocks.Add(loopBodyBlock);
                curBasicBlock = loopBodyBlock;
                instructionManager.setCurrentBlock(curBasicBlock);
                curBasicBlock.scopeNumber = scopes.Peek();

                StatSequence();

                Debug.WriteLine("cur join: {0}", curJoinBlock.blockNum);


                // todo, here we will need a branch to loop header
                VerifyToken(Token.OD, "The while loop was not properly closed with the od keyword");

                globalNestingLevel--;
                BasicBlock correspondingHeaderBlock = loopHeaderBlock;

                // Set the child of the loop body or follow back to it's header
                curBasicBlock.childBlocks.Add(correspondingHeaderBlock);

                // Insert the branch that will get us back to the header at the end of the loop
                instructionManager.PutUnconditionalBranch(Token.BRANCH, correspondingHeaderBlock.blockNum, AssemblyPC);
                SSAWriter.PutUnconditionalBranch(Utilities.TokenToInstruction(Token.BRANCH), correspondingHeaderBlock.blockNum.ToString(), AssemblyPC++);

                // Commit all of the phis
                if (joinBlockListForPhis.GetOuterJoin() != null) {
                    Dictionary<int, PhiInstruction> phisToCommit = instructionManager.CommitOuterPhi(ref AssemblyPC,
                        curJoinBlock, joinBlockListForPhis.GetOuterJoin());
                    AssemblyPC++;
                }


                // Search the dominator tree starting at the header and propagate
                // the phi assignments to the uses of that variable that appears in
                // the body, only traverse non-follow branch

                //BasicBlock firstBody;
                //foreach (BasicBlock block in loopHeaderBlock.childBlocks)
                //{
                //    if (block.blockType != BasicBlock.BlockType.FOLLOW)
                //        firstBody = block;
                //}
                instructionManager.PropagateHeaderPhis(loopHeaderBlock);
                

                // Update all of the values so that the next instructions reference the
                // new phi values
                foreach (KeyValuePair<int, PhiInstruction> phi in loopHeaderBlock.phiInstructions) {
                    UpdateSymbol(symbolTable[phi.Value.symTableID],
                        new Result(Kind.REG, String.Format("({0})", phi.Value.instructionNum)));
                }
                

                // Create the follow block, link the header to it, and set the current block to it. Also fix up the link in the branch
                // of the header now that we know the line number of the first instruction of the follow block
                BasicBlock loopFollowBlock = new BasicBlock(nextBBid++);
                flowGraphNodes[loopFollowBlock.blockNum] = loopFollowBlock;
                loopFollowBlock.childBlocks = new List<BasicBlock>();
                loopFollowBlock.parentBlocks = new List<BasicBlock>();
                loopFollowBlock.nestingLevel = globalNestingLevel;
                loopFollowBlock.dominatingBlock = loopHeaderBlock;
                loopHeaderBlock.blocksIDominate.Add(loopFollowBlock);
                loopFollowBlock.blockType = BasicBlock.BlockType.FOLLOW;
                loopFollowBlock.parentBlocks.Add(correspondingHeaderBlock);
                correspondingHeaderBlock.childBlocks.Add(loopFollowBlock);
                curBasicBlock = loopFollowBlock;
                instructionManager.setCurrentBlock(curBasicBlock);
                curBasicBlock.scopeNumber = scopes.Peek();

                joinBlockListForPhis.Pop();

                Debug.WriteLine("At follow");

                // Link the branch at the end of the header to the follow block
                Instruction branchToInstruction = correspondingHeaderBlock.firstInstruction;
                while (branchToInstruction.next != null)
                    branchToInstruction = branchToInstruction.next;

                branchToInstruction.secondOperand = loopFollowBlock.blockNum.ToString();


                if (joinBlockListForPhis.Count > 0)
                    curJoinBlock = joinBlockListForPhis.Peek();

                Next(); //eat od
            } else {
                scanner.Error("The relation in the if did not contain a valid conditional operator");
            }
            //return new Result(); // todo, don't know what this should be either
            return null;
        }

        private Result Ident() {
            Result res = null;
            VerifyToken(Token.IDENT, "Ended up at Identifier but didn't parse an identifier");
            // make sure thing is in scope
            if (!CheckScope(scanner.id)) return null;

            // load latest value
            res = new Result(Kind.VAR, scanner.Id2String(scanner.id)); // changed to var for now to simulate output

            Next(); // eat the identifier
            return res;

        }

        //public void DfsDominator(BasicBlock startNode)
        //{
            
        //}

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
                if (res1.type == Kind.ARR) {
                    // Need to store arrays differently
                    // TODO:: for now, storing things in memory immediately, but should only store when absolutely necessary
                    //   res1 = LoadIfNeeded(res1);
                    // If the thing is potentially a variable
                    int ID = scanner.String2Id(res1.GetValue());
                    if (ID != -1) {
                        UpdateSymbol(symbolTable[ID], null); // log the line number, current result, etc
                    }
                    instructionManager.PutStoreArray(res1, res2, GetArrayDimensions(res1.GetValue()), res1.GetArrayIndices(), AssemblyPC);
                    AssemblyPC = SSAWriter.StoreArrayElement(res1, res2, GetArrayDimensions(res1.GetValue()), res1.GetArrayIndices(), AssemblyPC);

                    // TODO:: don't update symbol cause it kills everything anyways?
                } else {
                    Result oldVarVal = null;
                    oldAssemblyPC = AssemblyPC;
                    instructionManager.PutBasicInstruction(Token.BECOMES, res2, res1, AssemblyPC);
                    SSAWriter.PutInstruction("mov", res2.GetValue(), res1.GetValue(), AssemblyPC);
                    // If the thing is potentially a variable
                    int ID = scanner.String2Id(res1.GetValue());
                    if (ID != -1)
                    {

                        if (curJoinBlock == null || !curJoinBlock.phiInstructions.ContainsKey(ID))
                            oldVarVal = symbolTable[ID].GetCurrentValue(scopes.Peek());
                        else
                            oldVarVal = curJoinBlock.phiInstructions[ID].originalVarVal;
                        UpdateSymbol(symbolTable[ID], null); // log the line number, current result, etc
                    }
                    AssemblyPC++;

                    // create the phi instruction 
                    string symbolName = res1.GetValue();

                    if (curJoinBlock != null) {

                        switch (curJoinBlock.blockType) {
                            case BasicBlock.BlockType.LOOP_HEADER:
                                if (!curJoinBlock.phiInstructions.ContainsKey(ID)) {
                                    instructionManager.PutPhiInstruction(AssemblyPC++, curJoinBlock, joinParentBlocks,
                                        scanner.String2Id(symbolName), oldVarVal, symbolName);

                                    Debug.WriteLine("Current join is: {0}", curJoinBlock.blockNum);
                                    //Debug.WriteLine("And outer join is {0}", joinBlockListForPhis.GetOuterJoin().blockNum);
                                }
                                else {
                                    instructionManager.UpdatePhiInstruction(ID, curJoinBlock);

                                    Debug.WriteLine("Current join is: {0}", curJoinBlock.blockNum);
                                    //Debug.WriteLine("And outer join is {0}", joinBlockListForPhis.GetOuterJoin().blockNum);
                                }
                                break;
                            case BasicBlock.BlockType.JOIN:
                                switch (curBasicBlock.blockType) {
                                    case BasicBlock.BlockType.TRUE:
                                        // todo, update true and false blocks to store the ssa val


                                            // There is not currently a phi instruction for the variable
                                            // in the current join block, create it
                                            instructionManager.PutPhiInstruction(AssemblyPC++, curJoinBlock, joinParentBlocks,
                                                scanner.String2Id(symbolName), oldVarVal, symbolName);

                                            Debug.WriteLine("Current join is: {0}", curJoinBlock.blockNum);
                                            //Debug.WriteLine("And outer join is {0}", joinBlockListForPhis.GetOuterJoin().blockNum);

                                        break;
                                    case BasicBlock.BlockType.FALSE:

                                            instructionManager.PutPhiInstruction(AssemblyPC++, curJoinBlock, joinParentBlocks,
                                                scanner.String2Id(symbolName), oldVarVal, symbolName);

                                            Debug.WriteLine("Current join is: {0}", curJoinBlock.blockNum);
                                            //Debug.WriteLine("And outer join is {0}", joinBlockListForPhis.GetOuterJoin().blockNum);

                                        
                                        break;
                                }
                                break;
                        }
                    }

                }



            }
            else {
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
                switch (Utilities.GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticRegInstruction(Utilities.TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        //                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToInstruction(newOP), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }
                //PutF2(Utilities.TokenToInstruction(opCode), loadednewA loadedB);
            } else if (newA.type == Kind.VAR && newB.type == Kind.CONST) {
                switch (Utilities.GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticImmInstruction(Utilities.TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC - 1).myResult = res;
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToBranchInstruction(newOP), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }
                //PutF1(Utilities.TokenToInstruction(opCode) + "i", loadednewA B);

            } else if (newA.type == Kind.REG && newB.type == Kind.CONST) {
                switch (Utilities.GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticImmInstruction(Utilities.TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                                                instructionManager.GetInstruction(AssemblyPC-1).myResult = res;
IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToBranchInstruction(opCode), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }
                //PutF1(Utilities.TokenToInstruction(opCode) + "i", loadednewA B);

            } else if (newA.type == Kind.VAR && newB.type == Kind.REG) {
                // todo, fill in later because reg is weird right now

                switch (Utilities.GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticRegInstruction(Utilities.TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                                                instructionManager.GetInstruction(AssemblyPC-1).myResult = res;
IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToBranchInstruction(newOP), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }
                //PutF2(Utilities.TokenToInstruction(opCode), loadednewA B);

            } else if (newB.type == Kind.REG && newA.type == Kind.REG) {

                switch (Utilities.GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticRegInstruction(Utilities.TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToBranchInstruction(newOP), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }
                //PutF2(Utilities.TokenToInstruction(opCode), loadednewB, loadedA);
            } else if (newB.type == Kind.VAR && newA.type == Kind.CONST) {

                switch (Utilities.GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticImmInstruction(Utilities.TokenToInstruction(opCode), newB, newA, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToBranchInstruction(newOP), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }
                //PutF1(Utilities.TokenToInstruction(opCode) + "i", loadednewB, A);

            } else if (newB.type == Kind.VAR && newA.type == Kind.REG) {

                switch (Utilities.GetOpCodeClass(opCode)) {
                    case OpCodeClass.ARITHMETIC_REG:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticRegInstruction(Utilities.TokenToInstruction(opCode), newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToBranchInstruction(newOP), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }
                //PutF2(Utilities.TokenToInstruction(opCode), loadednewB, A);

            } else if (newB.type == Kind.REG && newA.type == Kind.CONST) {
                switch (Utilities.GetOpCodeClass(opCode, true)) {
                    case OpCodeClass.ARITHMETIC_IMM:
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutArithmeticImmInstruction(Utilities.TokenToInstruction(opCode), newB, newA, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                    case OpCodeClass.COMPARE:
                        // compare
                        oldAssemblyPC = AssemblyPC;
                        instructionManager.PutBasicInstruction(opCode, newA, newB, AssemblyPC);
                        res = SSAWriter.PutCompare("cmp", newA, newB, AssemblyPC++);
                        instructionManager.GetInstruction(AssemblyPC-1).myResult = res;

                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                        // branch
                        oldAssemblyPC = AssemblyPC;
                        Token newOP = Utilities.NegatedConditional(opCode);
                        string label = "FALSE_" + curBasicBlock.blockNum;
                        instructionManager.PutConditionalBranch(newOP, res, label, AssemblyPC);
                        SSAWriter.PutConditionalBranch(Utilities.TokenToBranchInstruction(newOP), res, label, AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                        break;
                }


            }
            // This case causes issues with register allocation as it
            // is possibly not needed for constants
            else if (newA.type == Kind.CONST && newB.type == Kind.CONST) {
                res = new Result(Kind.CONST, (double)0);
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
        // TODO:: Can't have function parameters and local variables with same name (?)

        // Loads a variable from stack/memory if needed
        // Returns argument if doesn't need to be loaded
        private Result LoadIfNeeded(Result variableToLoad) {
            Result res = variableToLoad;
            if (variableToLoad.type == Kind.VAR) {
                int variID = scanner.String2Id(variableToLoad.GetValue());
                Symbol curSymbol = symbolTable[variID];
                // Cases: Is a global and hasn't been initialized yet
                //        Is a global and has been given a value
                //        Is a function parameter and needs to be loaded
                //        Is a function paramter that already has a value
                //        Is a local function variable that has no value yet
                //        Is a local function variable that has a value

                // Case: Variable already has a value
                if (curSymbol.GetCurrentValue(scopes.Peek()) != null) {
                    // Check if the variable has a value already in the scope
                    res = curSymbol.GetCurrentValue(scopes.Peek());
                } else if (curSymbol.GetType() != typeof(MemoryBasedSymbol) && curSymbol.IsInScope(scopes.Peek()) && curSymbol.GetCurrentValue(scopes.Peek()) == null) {
                    // Case: Local Function Variable                   
                    oldAssemblyPC = AssemblyPC;
                    // res = SSAWriter.LoadFunctionArgument(offset, variableToLoad, AssemblyPC);
                    //instructionManager.PutBasicInstruction(Token.MINUS, new Result(Kind.CONST, argSym.stackOffset), new Result(Kind.REG, "$FP"), AssemblyPC);
                    res = new Result(Kind.CONST, (double)0);
                    instructionManager.PutBasicInstruction(Token.BECOMES, res, variableToLoad, AssemblyPC);
                        instructionManager.GetInstruction(AssemblyPC).myResult = res;

                    SSAWriter.PutInstruction("mov", res.GetValue(), variableToLoad.GetValue(), AssemblyPC);

                    AssemblyPC++;
                    //                    instructionManager.PutLoadInstruction(variableToLoad, AssemblyPC);
                    UpdateSymbol(curSymbol, res);
                    //    AssemblyPC += 1;
                    IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                } else if (curSymbol.GetType() == typeof(MemoryBasedSymbol) && curSymbol.IsInScope(scopes.Peek()) && curSymbol.GetCurrentValue(scopes.Peek()) == null) {
                    // Case: Function Parameter
                    // load function values
                    MemoryBasedSymbol argSym = (MemoryBasedSymbol)curSymbol;
                    int offset = argSym.GetFunctionArgumentOffset();
                    oldAssemblyPC = AssemblyPC;
                    res = SSAWriter.LoadFunctionArgument(offset, variableToLoad, AssemblyPC);
                    instructionManager.PutBasicInstruction(Token.MINUS, new Result(Kind.CONST, argSym.stackOffset), new Result(Kind.REG, "$FP"), AssemblyPC);
                                            instructionManager.GetInstruction(AssemblyPC).myResult = res;

                    AssemblyPC++;
                    instructionManager.PutLoadInstruction(variableToLoad, AssemblyPC);
                    UpdateSymbol(curSymbol, res);
                    AssemblyPC += 1;
                    IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                } else if (curSymbol.GetType() == typeof(MemoryBasedSymbol) && curSymbol.IsGlobal() && curSymbol.GetCurrentValue(scopes.Peek()) == null) {
                    // Case: Global variable
                    // Check for unintialized Globals
                    MemoryBasedSymbol argSym = (MemoryBasedSymbol)curSymbol;
                    int offset = argSym.GetFunctionArgumentOffset();
                    oldAssemblyPC = AssemblyPC;
                    // res = SSAWriter.LoadFunctionArgument(offset, variableToLoad, AssemblyPC);
                    //instructionManager.PutBasicInstruction(Token.MINUS, new Result(Kind.CONST, argSym.stackOffset), new Result(Kind.REG, "$FP"), AssemblyPC);
                    res = new Result(Kind.CONST, (double)0);
                    instructionManager.PutBasicInstruction(Token.BECOMES, res, variableToLoad, AssemblyPC);
                        instructionManager.GetInstruction(AssemblyPC).myResult = res;

                    SSAWriter.PutInstruction("mov", res.GetValue(), variableToLoad.GetValue(), AssemblyPC);

                    AssemblyPC++;
                    //                    instructionManager.PutLoadInstruction(variableToLoad, AssemblyPC);
                    UpdateSymbol(curSymbol, res);
                    //    AssemblyPC += 1;
                    IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                }


            } else if (variableToLoad.type == Kind.ARR) {
                if (scannerSym == Token.OPENBRACKET)
                    res = MakeArrayReference(res, true);

                instructionManager.PutLoadArray(res, GetArrayDimensions(res.GetValue()), res.GetArrayIndices(), AssemblyPC);
                res = SSAWriter.LoadArrayElement(res, GetArrayDimensions(res.GetValue()), res.GetArrayIndices(), AssemblyPC);
                        instructionManager.GetInstruction(AssemblyPC).myResult = res;

                AssemblyPC = res.lineNumber + 1;
            }
            // 

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
                s.AddScope(scopes.Peek());
                s.SetValue(scopes.Peek(), res);
            }
            return res;
        }


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

                entryBlock = new BasicBlock(0);
                entryBlock.blockLabel = "ENTRY";
                flowGraphNodes[entryBlock.blockNum] = entryBlock;
                entryBlock.childBlocks = new List<BasicBlock>();
                entryBlock.parentBlocks = new List<BasicBlock>();
                entryBlock.blockType = BasicBlock.BlockType.ENTRY;

                exitBlock = new BasicBlock(0);
                exitBlock.blockLabel = "EXIT";
                flowGraphNodes[entryBlock.blockNum] = exitBlock;
                exitBlock.childBlocks = new List<BasicBlock>();
                exitBlock.parentBlocks = new List<BasicBlock>();
                exitBlock.blockType = BasicBlock.BlockType.EXIT;
                entryBlock.childBlocks.Add(exitBlock);

                rootBasicBlock = new BasicBlock(nextBBid++);
                rootBasicBlock.blockLabel = "MAIN:";
                flowGraphNodes[rootBasicBlock.blockNum] = rootBasicBlock;
                rootBasicBlock.childBlocks = new List<BasicBlock>();
                rootBasicBlock.parentBlocks = new List<BasicBlock>();
                rootBasicBlock.nestingLevel = globalNestingLevel;
                rootBasicBlock.blockType = BasicBlock.BlockType.MAIN_ENTRY;
                entryBlock.childBlocks.Add(rootBasicBlock);
                curBasicBlock = rootBasicBlock;
                instructionManager.setCurrentBlock(curBasicBlock);

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
                        instructionManager.PutBasicInstruction(Token.END, new Result(Kind.CONST, ""), new Result(Kind.CONST, ""), AssemblyPC);
                        SSAWriter.sw.WriteLine("{0}: end", AssemblyPC++);
                        Console.WriteLine("{0}: end", AssemblyPC++);
                        IncrementLoopCounters(oldAssemblyPC, AssemblyPC);

                    }

                }

            }
        }

        // TODO:: Need to allocate space on stack for locals when we finally write out code for codifier
        private void FuncDecl() {
            Token funcType = scannerSym;
            if (scannerSym == Token.FUNC || scannerSym == Token.PROC) {
                Next();
            } else {
                scanner.Error("Function Declaration missing function/ procedure keyword");
            }

            VerifyToken(Token.IDENT, "Function/Procedure declaration missing a name");

            BasicBlock functionStart = new BasicBlock(nextBBid++);
            flowGraphNodes[functionStart.blockNum] = functionStart;
            functionStart.childBlocks = new List<BasicBlock>();
            functionStart.parentBlocks = new List<BasicBlock>();
            functionStart.nestingLevel = globalNestingLevel;
            functionStart.blockType = BasicBlock.BlockType.FUNCTION_HEADER;
            curBasicBlock = functionStart;
            instructionManager.setCurrentBlock(functionStart);

            FunctionSymbol function;
            if (scopes.Peek() == 1) { // function is in global scope as well as local scope
                function = new FunctionSymbol(funcType, scanner.id, AssemblyPC, scopes.Peek());
                scopes.Push(nextScopeNumber++);
                // function should be able to be called from anywhere
                function.AddScope(scopes.Peek());
            } else {
                scopes.Push(nextScopeNumber++);
                function = new FunctionSymbol(funcType, scanner.id, AssemblyPC, scopes.Peek());
            }
            AddToSymbolTable(function, scopes.Peek());
            functionStart.blockLabel = scanner.Id2String(function.identID);
            curBasicBlock.scopeNumber = scopes.Peek();

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
                    Symbol sym = new MemoryBasedSymbol(Token.VAR, scanner.id, AssemblyPC, scopes.Peek(), currentOffset);
                    currentOffset -= 4;
                    AddToSymbolTable(sym, scopes.Peek());
                    function.numberOfFormalParamters++;

                    while (scannerSym == Token.COMMA) {
                        Next();
                        //                      Ident();
                        Next(); // eat Ident
                        sym = new MemoryBasedSymbol(Token.VAR, scanner.id, AssemblyPC, scopes.Peek(), currentOffset);
                        currentOffset -= 4;
                        AddToSymbolTable(sym, scopes.Peek());
                        function.numberOfFormalParamters++;
                    }
                }
                VerifyToken(Token.CLOSEPAREN, "Function Declaration missing a closing parenthesis");
                Next(); // eat closeparen
            }


            VerifyToken(Token.SEMI, "Function Declaration missing a semicolon");
            Next(); // eat semi
            // Func Body
            // Variable declarations
            int symtableSize = symbolTable.Count;
            while (scannerSym == Token.VAR || scannerSym == Token.ARR) {
                VarDecl();
            }
            // Record the size of the local variables
            // TODO:: Do locals need offsets set?
            int localsSize = 0;
            for (int i = symtableSize; i < symbolTable.Count; i++ ) {
                if (symbolTable[i].type == Token.ARR)
                    localsSize += ((ArraySymbol)symbolTable[i]).GetArraySize();
                else
                    localsSize++;
            }
            function.sizeOfLocals = localsSize;
            VerifyToken(Token.BEGIN, "Function body missing open bracket");
            Next(); // eat {
            // function statements
            StatSequence();

            VerifyToken(Token.END, "Function Body missing closing bracket"); //end function body
            Next(); // eat }
            VerifyToken(Token.SEMI, "Function declaration missing semicolon"); // end function declaration
            Next(); // eat ;

            oldAssemblyPC = AssemblyPC;
            instructionManager.PutFunctionLeave(AssemblyPC);
            SSAWriter.LeaveFunction(AssemblyPC);
            AssemblyPC++;
            IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
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
                oldAssemblyPC = AssemblyPC;
                SSAWriter.ReturnFromFunction(res, AssemblyPC);
                AssemblyPC++;
                IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
                // scope may not actually change -- early return statement
                return res;

            } else {
                oldAssemblyPC = AssemblyPC;
                SSAWriter.ReturnFromProcedure(AssemblyPC);
                AssemblyPC++;
                IncrementLoopCounters(oldAssemblyPC, AssemblyPC);
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
                AddToSymbolTable(new ArraySymbol(Token.ARR, scanner.id, AssemblyPC, dims.ToArray(), scopes.Peek(), -1), scopes.Peek());

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

            } else if (s.GetType() == typeof(ArraySymbol)) {
                // need to put the whole array on the stack
                if (scope == 1) {
                    // global array4
                    ArraySymbol curSym = (ArraySymbol) s;
                    curSym.SetArgumentOffset(nextGlobalOffset);
                    Console.WriteLine("{0} gets offset {1}", scanner.Id2String(s.identID), nextGlobalOffset);
                    nextGlobalOffset -= 4 * curSym.GetArraySize();
                    symbolTable.Insert(scanner.id, curSym);

                } else {
                    // not global array
                    symbolTable.Insert(scanner.id, s);

                }

            } else if (scope == 1 && s.GetType() != typeof(FunctionSymbol) && s.GetType() != typeof(MemoryBasedSymbol)) {
                // see if symbol need to be assigned an offset
                Console.WriteLine("{0} gets offset {1}", scanner.Id2String(s.identID), nextGlobalOffset);
                MemoryBasedSymbol newSym = new MemoryBasedSymbol(s.type, s.identID, s.currLineNumber, scope, nextGlobalOffset);
                symbolTable.Insert(s.identID, newSym);
                nextGlobalOffset -= 4;
            }
            else {
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

        // outputChecks - if true, chk statemnts will be printed
        private Result MakeArrayReference(Result res, bool outputChecks) {
            // find dimensions of array -- > requires symbol table look up
            int[] arrDims = GetArrayDimensions(res.GetValue());

            Result[] indexer = new Result[arrDims.Length];


            for (int i = 0; i < arrDims.Length; i++) {
                VerifyToken(Token.OPENBRACKET, "Array missing open bracket");
                Next();
                // evaluate indices
                indexer[i] = Expression();


                // Check validity of array index
                if (indexer[i].type == Kind.CONST) {
                    int index = Int32.Parse(indexer[i].GetValue());
                    if (index >= arrDims[i])
                        Error(String.Format("{0}: Array index {1} out of bounds ({2})", AssemblyPC, index, arrDims[i]));
                } else if (outputChecks) {
                    // check at runtime
                    Result bound = new Result(Kind.CONST, arrDims[i]);
                    // put bounds in a register
                    SSAWriter.PutChk(indexer[i], bound, AssemblyPC);
                    instructionManager.PutBasicInstruction(Token.CHECK, indexer[i], bound, AssemblyPC);
                    AssemblyPC++;

                }
                VerifyToken(Token.CLOSEBRACKET, "Array missing closing bracket");
                Next();
            }


            return new Result(Kind.ARR, res.GetValue(), indexer);


        }

        private void IncrementLoopCounters(int oldPC, int newPC) {
            int numInstructions = newPC - oldPC;
            if (loopCounters.Count > 0) {
                for (int i = 0; i < loopCounters.Count; i++) {
                    loopCounters[i] += numInstructions;
                }
            }
        }

        private string GetLabel(BasicBlock block, int blockNum) {
            switch (block.blockType) {
                case BasicBlock.BlockType.TRUE:
                    return "TRUE_" + blockNum + ":";
                case BasicBlock.BlockType.FALSE:
                    return "FALSE_" + blockNum + ":";
                case BasicBlock.BlockType.JOIN:
                    return "JOIN_" + blockNum + ":";
                default:
                    Debug.WriteLine("Shouldn't get here");
                    return "";
            }
        }

        public List<Symbol> ExportSymbolTable() {
            return symbolTable;
        }


    }
}
