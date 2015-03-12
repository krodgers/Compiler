using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    // Collection of functions to get code ready to be codified
    class CodifierPrep {
        ///////////////////////////////////////////////////////////
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
            Dictionary<int, Result> ssaValToResult = new Dictionary<int, Result>(); // maps ssa values --> results
            im.setCurrentBlock(newblock);
          


            Instruction currInstr = curBlock.firstInstruction;
            int currInstrNum = 1;
            Result first, second;

            while (currInstr != null) {
                // check swapPairs
                if (currInstr.firstOperandType == Instruction.OperandType.SSA_VAL && swapPairs.ContainsKey(currInstr.firstOperandSSAVal)) {
                    // need to swap out the operands
                }
                if (currInstr.secondOperandType == Instruction.OperandType.SSA_VAL && swapPairs.ContainsKey(currInstr.secondOperandSSAVal)) {
                    // need to swap out the opearnds
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
                    ReconstructResult(currInstr, out resA, out resB);

                    // Add updated instruction to newblock
                    switch (currInstr.opCode) {
                        case Token.TIMES:
                        case Token.DIV:
                        case Token.PLUS:
                        case Token.MINUS:
                            im.PutBasicInstruction(currInstr.opCode, resA, resB, currInstrNum);
                            break;
                        case Token.LOAD:
                        case Token.BECOMES:
                            im.PutLoadInstruction(resA, currInstrNum);
                            break;
                        case Token.EQL:
                        case Token.NEQ:
                        case Token.LSS:
                        case Token.GEQ:
                        case Token.LEQ:
                        case Token.GTR:
                        case Token.BRANCH:
                            if (currInstr.firstOperandType == Instruction.OperandType.BRANCH)
                                im.PutUnconditionalBranch(currInstr.opCode, Int32.Parse(currInstr.firstOperand), currInstrNum);
                            else if (currInstr.secondOperandType == Instruction.OperandType.BRANCH)
                                im.PutConditionalBranch(currInstr.opCode, resA, resB.GetValue(), currInstrNum);
                            else
                                im.PutCompare(currInstr.opCode, resA, resB, currInstrNum);
                            break;
                        case Token.END:
                        case Token.RETURN:
                            if (currInstr.firstOperandType != null)
                                im.PutFunctionReturn(resA, currInstrNum);
                            else
                                im.PutProcedureReturn(currInstrNum);
                            break;
                        case Token.PHI:
                        case Token.STORE:
                        case Token.OUTPUTNUM:
                        case Token.INPUTNUM:
                        case Token.OUTPUTNEWLINE:
                        default:
                            im.PutBasicInstruction(currInstr.opCode, resA, resB, currInstrNum);
                            break;
                    }
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
        private static void ReconstructResult(Instruction instr, out Result first, out Result second) {
            first = null; second = null;

            Kind myKind = OperandTypeToKind(instr.firstOperandType);
            switch (myKind) {
                case Kind.REG:
                case Kind.VAR:
                case Kind.BRA:
                    first = new Result(myKind, instr.firstOperand);
                    break;
                case Kind.CONST:
                    first = new Result(myKind, Double.Parse(instr.firstOperand));
                    break;
                default:
                    Console.WriteLine("ERROR: unable to reconstruct result");
                    Console.ReadKey();
                    break;
            }
            myKind = OperandTypeToKind(instr.secondOperandType);
            switch (myKind) {
                case Kind.REG:
                case Kind.VAR:
                case Kind.BRA:
                    second = new Result(myKind, instr.secondOperand);
                    break;
                case Kind.CONST:
                    second = new Result(myKind, Double.Parse(instr.secondOperand));
                    break;
                default:
                    Console.WriteLine("ERROR: unable to reconstruct result");
                    Console.ReadKey();
                    break;
            }

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
