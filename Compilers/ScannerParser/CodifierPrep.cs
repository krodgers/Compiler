using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ScannerParser {
    // Collection of functions to get code ready to be codified
    public class CodifierPrep {

        private static Dictionary<int, Result> lineNumToResult;
        private static Dictionary<int, List<Instruction>> blockNum_branch;

        private static int AssemblyPC;
        // Constructor because C# is dumb
        public CodifierPrep() {

        }

        ////////////////////////////////////////////////////////////
        // Optimization things 
        //////////////////////////////////////////////////////////////
        public static Queue<BasicBlock> PerformCopyPropagation(BasicBlock start, List<Symbol> symTable, out int totalNumberOfLines) {
            Queue<BasicBlock> blocks = Utilities.TraverseCFG(ref start);
            Queue<BasicBlock> blocksPropped = new Queue<BasicBlock>();

            Dictionary<int, int>[] blockNum_swapPairs = new Dictionary<int, int>[blocks.Count + 1];
            Dictionary<string, int>[] blockNum_variVals = new Dictionary<string, int>[blocks.Count + 1];
            
            Dictionary<string, int> variVals = new Dictionary<string, int>();
            Dictionary<int, int> pairsToSwap = new Dictionary<int, int>();
            lineNumToResult = new Dictionary<int, Result>(); // maps ssa values --> results
            blockNum_branch = new Dictionary<int, List<Instruction>>(); // maps block num --> instructions that needs it
            AssemblyPC = 0;

            blockNum_swapPairs[0] = pairsToSwap;
            blockNum_variVals[0] = variVals;

            while (blocks.Count > 0) {
                BasicBlock currBlock = blocks.Dequeue();

                if (currBlock.dominatingBlock != null) {
                    variVals = blockNum_variVals[currBlock.dominatingBlock.blockNum];
                    pairsToSwap = blockNum_swapPairs[currBlock.dominatingBlock.blockNum];
                } else {
                      variVals = new Dictionary<string, int>();
                      pairsToSwap = new Dictionary<int, int>();
                }
                BasicBlock res = Propagate(currBlock, symTable, ref variVals, ref pairsToSwap);

                blocksPropped.Enqueue(res);
                blockNum_swapPairs[currBlock.blockNum] = pairsToSwap;
                blockNum_variVals[currBlock.blockNum] = variVals;

            }

            // fix branch locations
            Instruction curr;
            int numLines = 0, blockNum = 0;
            int currLine;
            foreach (BasicBlock bb in blocksPropped) {
                curr = bb.firstInstruction;
                blockNum = bb.blockNum;
                if (blockNum_branch.ContainsKey(blockNum)) {
                    foreach (Instruction i in blockNum_branch[blockNum]) {
                        if (i.secondOperandType == Instruction.OperandType.BRANCH) {
                            i.secondOperand = String.Format("{0}", (numLines - i.instructionNum) +1);
                            i.secondOperandSSAVal = 0;
                        } else {
                             i.firstOperand = String.Format("{0}", bb.firstInstruction.instructionNum - numLines);
                             i.firstOperandSSAVal = 0;
                        }
                    }
                }
                numLines += bb.instructionCount;

            }
            
            totalNumberOfLines = numLines;


            return blocksPropped;
        }

        // returns a basicblock with values copy propaagted
        // swapPairs -- the pairs that should be swapped, e.g (1) --> new_Instruction -- should be initialized with dominating blocks' output
        // variableValues --> maps variable names to their instructions ---- -- should be initialized with dominating blocks' output
        //                     SSA Value of 0 means it hasn't got one
        // TODO:: Constant propagation
        // TODO:: Reset variable values
        private static BasicBlock Propagate(BasicBlock curBlock, List<Symbol> symbolTable, ref Dictionary<string, int> variableValues, ref Dictionary<int, int> swapPairs) {
            InstructionManager im = new InstructionManager(symbolTable);
            BasicBlock newblock = CopyBasicBlock(curBlock);
            im.setCurrentBlock(newblock);
            

            Instruction curInstr = curBlock.firstInstruction;
            Result resA, resB;
            while (curInstr != null) {
                // Look up to see if the opearands need to be replaced
                bool changeFirstOper = swapPairs.ContainsKey(curInstr.firstOperandSSAVal);
                bool changeSecondOper = swapPairs.ContainsKey(curInstr.secondOperandSSAVal);

                AssemblyPC++;
             //   System.Diagnostics.Debug.Assert(AssemblyPC == curInstr.instructionNum);
                switch (curInstr.opCode) {
                    // Assignment statement
                    case Token.BECOMES:
                        Result valueResult = changeFirstOper ? lineNumToResult[swapPairs[curInstr.firstOperandSSAVal]] : ReconstructResult(curInstr.firstOperandType, curInstr.firstOperand, AssemblyPC); // the assignment value
                        if (!lineNumToResult.ContainsKey(AssemblyPC))
                            lineNumToResult.Add(AssemblyPC, valueResult); // store the pair  (currLine) --> assignValue
                        int currentValue;
                        Result variableResult = ReconstructResult(Instruction.OperandType.VAR, curInstr.secondOperand, AssemblyPC);
                   
                        // remove any swap values that depended on the value of this variable
                        if (variableValues.TryGetValue(curInstr.secondOperand, out currentValue)) {
                            swapPairs.Remove(currentValue);
                            foreach (KeyValuePair<int, int> pair in swapPairs) {
                                if (pair.Value == AssemblyPC)
                                    swapPairs.Remove(pair.Key);
                            }
                        } else {
                            variableValues.Add(variableResult.GetValue(), valueResult.lineNumber);
                        }
                        swapPairs.Add(AssemblyPC, valueResult.lineNumber);
// not loading a new value, don't need the move instruction
                        if (!changeFirstOper)
                            PutInstruction(curInstr.opCode, valueResult, variableResult, AssemblyPC, ref im);
                        break;
                    case Token.RETURN:
                    case Token.END:
                        resA=  changeFirstOper ? lineNumToResult[swapPairs[curInstr.firstOperandSSAVal]] : ReconstructResult(curInstr.firstOperandType, curInstr.firstOperand, AssemblyPC);
                        PutInstruction(curInstr.opCode, resA, null, AssemblyPC, ref im);
                        break;
                    case Token.PHI:
                    // phis kill assignments
                        resA = changeFirstOper ? lineNumToResult[swapPairs[curInstr.firstOperandSSAVal]] : ReconstructResult(curInstr.firstOperandType, curInstr.firstOperand, AssemblyPC);
                        resB = changeSecondOper ? lineNumToResult[swapPairs[curInstr.secondOperandSSAVal]] : ReconstructResult(curInstr.secondOperandType, curInstr.secondOperand, AssemblyPC);

                        List<int> keysToRemove = new List<int>();
                        foreach (KeyValuePair<int, int> pair in swapPairs) {
                            if (pair.Value == curInstr.firstOperandSSAVal || pair.Value == curInstr.secondOperandSSAVal)
                                keysToRemove.Add(pair.Key);
//                               swapPairs.Remove(pair.Key);
                        }
                        foreach (int k in keysToRemove)
                            swapPairs.Remove(k);
                        break;
                    case Token.PLUS:
                    case Token.MINUS:
                    case Token.TIMES:
                    case Token.DIV:

                    default:
                         resA = changeFirstOper ? lineNumToResult[swapPairs[curInstr.firstOperandSSAVal]] : ReconstructResult(curInstr.firstOperandType, curInstr.firstOperand, AssemblyPC);
                         resB = changeSecondOper ? lineNumToResult[swapPairs[curInstr.secondOperandSSAVal]] : ReconstructResult(curInstr.secondOperandType, curInstr.secondOperand, AssemblyPC);
                      //  if (changeFirstOper || changeSecondOper)
                            PutInstruction(curInstr.opCode, resA, resB, AssemblyPC, ref im);
                            if (!lineNumToResult.ContainsKey(AssemblyPC))
                                lineNumToResult.Add(AssemblyPC, curInstr.myResult);
                        //else

                            break;
                }

                curInstr = curInstr.next;

                
            }
            return newblock;


            // for instruction i in block
            // if instr is assignment ( var = val)
            //      check if 
            //       mark var has having val variable table
            //       mark the result's SSAVal as needing to be changed to val
            //       update instruction
            // if instr is op A B
            //      check if A's SSAVAl is in table of change pairs
            //      check if B's SSAVal is in table of change pairs
            //      check if result's SSA value is in any change pairs -- if so, remove them
        }


        private static void PutInstruction(Token opcode, Result resA, Result resB, int lineNumber, ref InstructionManager im) {
// both results can't be constants
            // Add updated instruction to newblock
            switch (opcode) {
                case Token.TIMES:
                case Token.DIV:
                case Token.PLUS:
                case Token.MINUS:
                    im.PutBasicInstruction(opcode, resA, resB, lineNumber);
                    break;
                case Token.LOAD:
                    im.PutLoadInstruction(resA, lineNumber);
                    break;
                case Token.BECOMES:
                    im.PutBasicInstruction(Token.BECOMES, resA, resB, lineNumber);
                    break;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                case Token.BRANCH:
                    if (KindToOperandType(resA) == Instruction.OperandType.BRANCH) {
                        int blockToBranchTo = Int32.Parse(resA.GetValue()); 
                        im.PutUnconditionalBranch(opcode, blockToBranchTo, lineNumber);
                        // track we need to fix the jump location
                        if (blockNum_branch.ContainsKey(blockToBranchTo)) {
                            List<Instruction> branches = blockNum_branch[blockToBranchTo];
                            branches.Add(im.GetInstruction(lineNumber));
                            blockNum_branch[blockToBranchTo] = branches;
                        } else {
                            List<Instruction> branches = new List<Instruction>();
                            branches.Add(im.GetInstruction(lineNumber));
                            blockNum_branch.Add(blockToBranchTo, branches);
                        }
                     
                    } else if (KindToOperandType(resB) == Instruction.OperandType.BRANCH) {
                        int blockToBranchTo = Int32.Parse(resB.GetValue());
                        im.PutConditionalBranch(opcode, resA, resB.GetValue(), lineNumber);
                        if (blockNum_branch.ContainsKey(blockToBranchTo)) {
                            List<Instruction> branches = blockNum_branch[blockToBranchTo];
                            branches.Add(im.GetInstruction(lineNumber));
                            blockNum_branch[blockToBranchTo] = branches;
                        } else {
                            List<Instruction> branches = new List<Instruction>();
                            branches.Add(im.GetInstruction(lineNumber));
                            blockNum_branch.Add(blockToBranchTo, branches);
                        }

                    } else {
                        im.PutCompare(opcode, resA, resB, lineNumber);
                    }
                    break;
                case Token.END:
                case Token.RETURN:
                    if (resB != null)
                        im.PutFunctionReturn(resA, lineNumber);
                    else
                        im.PutProcedureReturn(lineNumber);
                    break;
                case Token.PHI:
                case Token.STORE:
                case Token.OUTPUTNUM:
                case Token.INPUTNUM:
                case Token.OUTPUTNEWLINE:
                default:
                    im.PutBasicInstruction(opcode, resA, resB, lineNumber);
                    break;
            }
        }

        private static Result ReconstructResult(Instruction.OperandType? type, string operand, int ssaval) {
            Result res = null;
            Kind myKind = OperandTypeToKind(type);
            switch (myKind) {
                case Kind.REG:
                case Kind.VAR:
                case Kind.BRA:
                    res = new Result(myKind, operand);
                    break;
                case Kind.CONST:
                    double constval;
                    if (Double.TryParse(operand, out constval))
                        res = new Result(myKind, (double)constval);
                    else
                        res = new Result(myKind, operand);
                    break;
                case Kind.ARR:
                    res = new Result(myKind, operand, null);
                    break;
                default:
                    Console.WriteLine("ERROR: unable to reconstruct result");
                   // Console.ReadKey();
                    break;
            }
            res.lineNumber = ssaval;
            return res;

        }

        private static Kind OperandTypeToKind(Instruction.OperandType? type) {
            if (type == null)
                return Kind.ARR;

            switch (type) {
                case Instruction.OperandType.SSA_VAL:
                case Instruction.OperandType.REG:
                    return Kind.REG;
                case Instruction.OperandType.VAR:
                    return Kind.VAR;
                case Instruction.OperandType.CONSTANT:
                    return Kind.CONST;
                case Instruction.OperandType.BRANCH:
                    return Kind.BRA;
                case Instruction.OperandType.ARRAY:
                    return Kind.ARR;
                default:
                    Console.WriteLine("Should not have gotten here");
                    return Kind.ARR; // have to return something..... :(
            }
        }
        private static Instruction.OperandType KindToOperandType(Result res) {
            switch (res.type) {
                case Kind.REG:
                    return Instruction.OperandType.REG;
                case Kind.VAR:
                    return Instruction.OperandType.VAR;
                case Kind.CONST:
                    return Instruction.OperandType.CONSTANT;
                case Kind.BRA:
                    return Instruction.OperandType.BRANCH;
                case Kind.ARR:
                    return Instruction.OperandType.ARRAY;
                default:
                    Console.WriteLine("Should not have gotten here");
                    return Instruction.OperandType.ERROR;
            }
        }
        private static BasicBlock CopyBasicBlock(BasicBlock toCopy) {
            BasicBlock res = new BasicBlock(toCopy.blockNum);
            res.blockLabel = toCopy.blockLabel;
            res.blocksIDominate = toCopy.blocksIDominate;
            res.blockType = toCopy.blockType;
            res.childBlocks = toCopy.childBlocks;
            res.dominatingBlock = toCopy.dominatingBlock;
            res.nestingLevel = toCopy.nestingLevel;
            res.parentBlocks = toCopy.parentBlocks;
            res.phiInstructions = toCopy.phiInstructions;
            res.scopeNumber = toCopy.scopeNumber;

            return res;
        }




        // Returns a new block with all of the code added in
        public static BasicBlock PutFunctionProlouge(BasicBlock functionBlock, FunctionSymbol function) {
            BasicBlock bookKeeping = new BasicBlock(functionBlock.blockNum);
            Instruction first = functionBlock.firstInstruction;
            //Instruction last = functionBlock.


            return bookKeeping;


        }



    }
}
