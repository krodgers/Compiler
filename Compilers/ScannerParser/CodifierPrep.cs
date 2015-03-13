using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    // Collection of functions to get code ready to be codified
    public class CodifierPrep {
        // Constructor because C# is dumb
        public CodifierPrep() {

        }

        ////////////////////////////////////////////////////////////
        // Optimization things 
        //////////////////////////////////////////////////////////////
        public static void PerformCopyPropagation(BasicBlock start) {

        }

        // returns a basicblock with values copy propaagted
        // swapPairs -- the pairs that should be swapped, e.g (1) --> new_Instruction -- should be initialized with dominating blocks' output
        // variableValues --> maps variable names to their instructions ---- -- should be initialized with dominating blocks' output
        //                     SSA Value of 0 means it hasn't got one

        private static BasicBlock Propagate(BasicBlock curBlock, ref Dictionary<string, int> variableValues, ref Dictionary<int, int> swapPairs) {
            InstructionManager im = new InstructionManager();
            BasicBlock newblock = CopyBasicBlock(curBlock);
            Dictionary<int, Result> lineNumToResult = new Dictionary<int, Result>(); // maps ssa values --> results
            im.setCurrentBlock(newblock);
            int AssemblyPC = 0;


            Instruction curInstr = curBlock.firstInstruction;
            while (curInstr != null) {
                // Look up to see if the opearands need to be replaced
                bool changeFirstOper = swapPairs.ContainsKey(curInstr.firstOperandSSAVal);
                bool changeSecondOper = swapPairs.ContainsKey(curInstr.secondOperandSSAVal);

                switch (curInstr.opCode) {
                    // Assignment statement
                    case Token.BECOMES:
                        Result valueResult = changeFirstOper ? lineNumToResult[swapPairs[curInstr.firstOperandSSAVal]] : ReconstructResult(curInstr.firstOperandType, curInstr.firstOperand, AssemblyPC); // the assignment value
                        lineNumToResult.Add(AssemblyPC, valueResult); // store the pair  (currLine) --> assignValue
                        int currentValue;
                        // remove any swap values that depended on the value of this variable
                        if (variableValues.TryGetValue(curInstr.secondOperand, out currentValue)) {
                            swapPairs.Remove(currentValue);
                            foreach (KeyValuePair<int, int> pair in swapPairs) {
                                if (pair.Value == AssemblyPC)
                                    swapPairs.Remove(pair.Key);
                            }
                        }
                        Result variableResult = ReconstructResult(Instruction.OperandType.VAR, curInstr.secondOperand, AssemblyPC);
                        PutInstruction(curInstr.opCode, valueResult, variableResult, AssemblyPC, ref im);
                        break;
                    case Token.RETURN:
                    case Token.END:
                        break;
                    default:
                        break;
                }

                AssemblyPC++;
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
     

        private static BasicBlock old(BasicBlock curBlock, ref Dictionary<string, int> variableValues, ref Dictionary<int, int> swapPairs) {
            InstructionManager im = new InstructionManager();
            BasicBlock newblock = CopyBasicBlock(curBlock);
            Dictionary<int, Result> ssaValToResult = new Dictionary<int, Result>(); // maps ssa values --> results
            im.setCurrentBlock(newblock);
          


            Instruction currInstr = curBlock.firstInstruction;
            int currInstrNum = 1;
            Result first, second;

            while (currInstr != null) {
                first = second = null;
                // check swapPairs
                if (currInstr.firstOperandType == Instruction.OperandType.SSA_VAL && swapPairs.ContainsKey(currInstr.firstOperandSSAVal)) {
                    // need to swap out the operands
                    first = ssaValToResult[currInstr.firstOperandSSAVal];
                }
                if (currInstr.secondOperandType == Instruction.OperandType.SSA_VAL && swapPairs.ContainsKey(currInstr.secondOperandSSAVal)) {
                    // need to swap out the opearnds
                    second = ssaValToResult[currInstr.secondOperandSSAVal];
                }


                if (currInstr.opCode == Token.BECOMES) {
                    string variableName = currInstr.secondOperand;
                    string value = currInstr.firstOperand;
                    int ssaval = Int32.Parse(value);


                    // update variableValues
                    int bootedResult;
                    if (variableValues.TryGetValue(variableName, out bootedResult)) {
                        // successfully got value 
                        // remove all pairs containing bootedResult
                        foreach (KeyValuePair<int, int> pair in swapPairs) {
                            if (pair.Value == bootedResult || pair.Key == bootedResult)
                                swapPairs.Remove(pair.Key);
                        }
                    } else {
                        // map this variable to the instruction associated with the value
                        if (currInstr.firstOperandType == Instruction.OperandType.SSA_VAL) {
                            variableValues.Add(variableName, ssaval);

                        } else if (currInstr.firstOperandType == Instruction.OperandType.CONSTANT) {
                            // Add a constant instruction ... ? 

                        } else {
                            Console.WriteLine("Got an assignment instruction, but don't know how to parse it");
                            Console.WriteLine(currInstr);
                            Console.ReadLine();
                        }
                    }

                    Result resA, resB;
                    resA = first == null ? null : first;
                    resB = second == null ? null : second;
//                    ReconstructResult(currInstr, out resA, out resB);

                  
                }
                currInstr = currInstr.next;
                currInstrNum++;
            }

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
            return newblock;


        }

        private static void PutInstruction(Token opcode, Result resA, Result resB, int lineNumber, ref InstructionManager im) {
            // Add updated instruction to newblock
            switch (opcode) {
                case Token.TIMES:
                case Token.DIV:
                case Token.PLUS:
                case Token.MINUS:
                    im.PutBasicInstruction(opcode, resA, resB, lineNumber);
                    break;
                case Token.LOAD:
                case Token.BECOMES:
                    im.PutLoadInstruction(resA, lineNumber);
                    break;
                case Token.EQL:
                case Token.NEQ:
                case Token.LSS:
                case Token.GEQ:
                case Token.LEQ:
                case Token.GTR:
                case Token.BRANCH:
                    if (KindToOperandType(resA) == Instruction.OperandType.BRANCH)
                        im.PutUnconditionalBranch(opcode, Int32.Parse(resA.GetValue()), lineNumber);
                    else if (KindToOperandType(resB) == Instruction.OperandType.BRANCH)
                        im.PutConditionalBranch(opcode, resA, resB.GetValue(), lineNumber);
                    else
                        im.PutCompare(opcode, resA, resB, lineNumber);
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
                case Instruction.OperandType.REG:
                    return Kind.REG;
                case Instruction.OperandType.VAR:
                    return Kind.VAR;
                case Instruction.OperandType.CONSTANT:
                    return Kind.CONST;
                case Instruction.OperandType.BRANCH:
                    return Kind.BRA;
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
                    return Instruction.OperandType.VAR;
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
