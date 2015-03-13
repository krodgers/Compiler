using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    public class InstructionManager {
        private Dictionary<int, Instruction> instructionDictionary;
        private BasicBlock curBasicBlock;
        private List<Symbol> symbolTable;
        public Dictionary<int, BasicBlock> joinMatches;

        public InstructionManager(List<Symbol> symbolTable) {
            instructionDictionary = new Dictionary<int, Instruction>();
            this.symbolTable = symbolTable;
        }

        public void setCurrentBlock(BasicBlock current) {
            curBasicBlock = current;
        }

        public BasicBlock getCurrentBlock() {
            return curBasicBlock;
        }
        public Instruction GetInstruction(int instructionNum) {
            Instruction result = null;
            instructionDictionary.TryGetValue(instructionNum, out result);
            return result;
        }

        // NOTE:  Token.ARR means adda
        public void PutBasicInstruction(Token opCode, Result a, Result b, int lineNumber) {

            // Initialize all of the non-pointer fields for the instruction
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = opCode;

            tmp.firstOperand = String.Format("{0}", a.GetValue());
            if (a.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(a.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(a);
            }

            tmp.secondOperand = String.Format("{0}", b.GetValue());
            if (b.GetValue().Contains("(")) {
                tmp.secondOperandSSAVal = GetNumFromSSAReg(b.GetValue());
                tmp.secondOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.secondOperandType = KindToOperandType(b);
            }

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);

        }

        public void PutLoadInstruction(Result itemToLoad, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.LOAD;

            tmp.firstOperand = itemToLoad.GetValue();
            if (itemToLoad.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(itemToLoad.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(itemToLoad);
            }


            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutProcedureReturn(int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.RETURN;

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutFunctionReturn(Result res, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.RETURN;

            tmp.firstOperand = res.GetValue();
            if (res.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(res.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(res);
            }

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutFunctionArgument(Result argument, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.MINUS;

            tmp.firstOperand = "4";
            tmp.firstOperandType = Instruction.OperandType.CONSTANT;

            tmp.secondOperand = "$SP";
            tmp.secondOperandType = Instruction.OperandType.REG;



            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);

            PutBasicInstruction(Token.STORE, argument, new Result(Kind.REG, String.Format("({0})", lineNumber)), lineNumber + 1);

        }

        public void PutFunctionEntry(Result function, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.BRANCH;

            tmp.firstOperand = function.GetValue().ToUpper();
            tmp.firstOperandType = KindToOperandType(function);

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutFunctionLeave(int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = Token.BRANCH;

            tmp.firstOperand = "$RA";
            tmp.firstOperandType = Instruction.OperandType.REG;

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        // branches to branchlocation unconditionally
        // i.e. branch TRUE
        public void PutUnconditionalBranch(Token opCode, int branchLocation, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = opCode;

            tmp.firstOperand = branchLocation.ToString();
            tmp.firstOperandType = Instruction.OperandType.BRANCH;

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }
        // cmp A B
        public void PutCompare(Token opCode, Result operandA, Result operandB, int lineNumber) {
            PutBasicInstruction(opCode, operandA, operandB, lineNumber);

        }

        // Put a phi instruction
        //public void PutPhiInstruction(int lineNumber, BasicBlock curJoin, int originalVarVal, int symTableId)
        //{
        //    PhiInstruction phi = new PhiInstruction(lineNumber, curJoin, originalVarVal);
        //    phi.symTableID = symTableId;
        //    phi.opCode = Token.PHI;
        //}

        // Branch on condition
        // condResult -- the compare result this branch is dependent on
        // tempLabel -- the place to branch to ---- complete unused
        public void PutConditionalBranch(Token opCode, Result condResult, string tempLabel, int lineNumber) {
            Instruction tmp = new Instruction(lineNumber, curBasicBlock);
            tmp.opCode = opCode;

            tmp.firstOperand = condResult.GetValue();
            if (condResult.GetValue().Contains("(")) {
                tmp.firstOperandSSAVal = GetNumFromSSAReg(condResult.GetValue());
                tmp.firstOperandType = Instruction.OperandType.SSA_VAL;
            }
            else {
                tmp.firstOperandType = KindToOperandType(condResult);
            }

            tmp.secondOperand = tempLabel;
            tmp.secondOperandType = Instruction.OperandType.BRANCH;

            // Add the new instruction to the dictionary of all instructions
            instructionDictionary.Add(lineNumber, tmp);

            // Initialize all of the pointer fields for the instruction
            InsertAndLink(tmp);
        }

        public void PutLoadArray(Result array, int[] dims, Result[] indices, int lineNum) {
            Result currentResult;
            int lineNumber = PutArrayAddress(array, dims, indices, lineNum, out currentResult);

            // add FP + Base
            Result arrayBase = new Result(Kind.REG, String.Format("({0})", lineNumber));
            PutBasicInstruction(Token.PLUS, new Result(Kind.REG, "FP"), array, lineNumber++);
            // adda/load
            PutBasicInstruction(Token.ARR, currentResult, arrayBase, lineNumber);
            lineNumber++;
            Result finalResult = new Result(Kind.REG, String.Format("({0})", lineNumber));
            finalResult.lineNumber = lineNumber;
            PutLoadInstruction(finalResult, lineNumber);
        }

        public void PutStoreArray(Result array, Result thingToStore, int[] dims, Result[] indices, int lineNum) {
            Result currentResult;
            int lineNumber = PutArrayAddress(array, dims, indices, lineNum, out currentResult);

            // add FP + Base
            Result arrayBase = new Result(Kind.REG, String.Format("({0})", lineNumber));
            PutBasicInstruction(Token.PLUS, new Result(Kind.REG, "FP"), array, lineNumber++);
            // adda/load
            PutBasicInstruction(Token.ARR, currentResult, arrayBase, lineNumber);
            lineNumber++;
            Result finalResult = new Result(Kind.REG, String.Format("({0})", lineNumber));
            finalResult.lineNumber = lineNumber;

            PutBasicInstruction(Token.STORE, thingToStore, finalResult, lineNumber);
        }


        // Returns the last lineNumber
        private int PutArrayAddress(Result array, int[] dims, Result[] indices, int lineNumber, out Result currentResult) {
            Result[] inds = array.arrIndices;
            int addr = 0;
            int constantAccum = 1;
            currentResult = null;
            List<Result> termsToAdd = new List<Result>();

            for (int i = 0; i < indices.Length - 1; i++) {
                for (int d = i + 1; d < dims.Length; d++) {
                    constantAccum *= dims[d];
                }
                if (indices[i].type == Kind.CONST) {
                    // Continue accumulating a constant address
                    addr += constantAccum * Int32.Parse(indices[i].GetValue());
                }
                else {
                    currentResult = new Result(Kind.REG, String.Format("({0})", lineNumber));
                    PutBasicInstruction(Token.TIMES, new Result(Kind.CONST, constantAccum), indices[i], lineNumber);
                    termsToAdd.Add(currentResult);
                }
                constantAccum = 1;
            }
            // all other terms have been pushed to termsToAdd
            currentResult = indices[indices.Length - 1];
            // check for constant address parts
            if (addr != 0) {
                if (currentResult.type == Kind.CONST)
                    currentResult = new Result(Kind.CONST, addr + Int32.Parse(currentResult.GetValue()));
                else {
                    // add address part and last index
                    PutBasicInstruction(Token.PLUS, currentResult, new Result(Kind.CONST, addr), lineNumber);
                    currentResult = new Result(Kind.REG, String.Format("({0})", lineNumber));
                    lineNumber++;
                }
            }
            // If there are no constant address parts, then currentResult sure to be initialized
            Debug.Assert(currentResult != null, "LoadArray has null result");
            foreach (Result r in termsToAdd) {
                // Add all of the terms
                PutBasicInstruction(Token.PLUS, currentResult, r, lineNumber);
                currentResult = new Result(Kind.REG, String.Format("({0})", lineNumber));
                lineNumber++;
            }
            // Multiply address by 4
            if (currentResult.type == Kind.CONST)
                currentResult = new Result(Kind.CONST, Int32.Parse(currentResult.GetValue()) * 4);
            else {

                PutBasicInstruction(Token.TIMES, new Result(Kind.CONST, 4), currentResult, lineNumber++);
                currentResult = new Result(Kind.REG, String.Format("({0})", lineNumber));

            }

            return lineNumber;
        }

        public void PutPhiInstruction(int lineNumber, BasicBlock curJoinBlock, int symTableID, Result oldVarVal,
            string symbolName) {

            Instruction tmp;

            Result preBranchVal = oldVarVal;
            PhiInstruction phi = new PhiInstruction(lineNumber, curJoinBlock, preBranchVal, symbolName);
            phi.symTableID = symTableID;
            phi.opCode = Token.PHI;

            // todo, need to check curJoinBlock type, not the cur basic block type. Also
            // need to make sure that each type of cur basic block will be updated with a phi
            // properly. There needs to be a phi for EVERY assignment, regardless of what type
            // of block it is currently

            // todo, the above is solved for loops, but still need to figure out how
            // to decide which side of the phi instruction to place the new assignments on for ifs,
            // as we won't know what nodes connect to the join blocks yet

            switch (curJoinBlock.blockType) {
                case BasicBlock.BlockType.LOOP_HEADER:
                    SetFirstOperand(phi, preBranchVal);

                    // Link the phi instruction to the move that created this value and back
                    phi.neededInstr[0] = instructionDictionary[phi.firstOperandSSAVal];
                    instructionDictionary[phi.firstOperandSSAVal].referencesToThisValue.Add(phi);

                    SetSecondOperand(phi);

                    // Link the phi instruction to the move that created this value
                    tmp = curBasicBlock.firstInstruction;
                    while (tmp.next != null)
                        tmp = tmp.next;
                    phi.neededInstr[1] = tmp;
                    tmp.referencesToThisValue.Add(phi);

                    // Insert the phis at the front for a loop
                    if (curJoinBlock.firstInstruction != null) {
                        phi.next = curJoinBlock.firstInstruction;
                        curJoinBlock.firstInstruction = phi;
                    }
                    else {
                        curJoinBlock.firstInstruction = phi;
                    }
                    break;
                case BasicBlock.BlockType.JOIN:
                    int curSide = WhichSide(curBasicBlock, joinMatches[curJoinBlock.blockNum]);

                    switch (curSide) {
                        case 0:
                            // Fill in the left operand with the value
                            SetFirstOperand(phi);

                            // Link the phi instruction to the move that created this value
                            tmp = curBasicBlock.firstInstruction;
                            while (tmp.next != null)
                                tmp = tmp.next;
                            phi.neededInstr[0] = tmp;
                            tmp.referencesToThisValue.Add(phi);

                            // Second operand is the initial value
                            SetSecondOperand(phi, preBranchVal);

                            // Link the phi instruction to the move that created this value and back
                            if (preBranchVal.type != Kind.CONST)
                            {
                                phi.neededInstr[1] = instructionDictionary[phi.secondOperandSSAVal];
                                instructionDictionary[phi.secondOperandSSAVal].referencesToThisValue.Add(phi);
                            }
                            break;
                        case 1:
                            // Fill in the right operand with the value
                            SetFirstOperand(phi, preBranchVal);

                            // Link the phi instruction to the move that created this value and back
                            if (preBranchVal.type != Kind.CONST) {
                                phi.neededInstr[0] = instructionDictionary[phi.firstOperandSSAVal];
                                instructionDictionary[phi.firstOperandSSAVal].referencesToThisValue.Add(phi);
                            }

                            SetSecondOperand(phi);

                            // Link the phi instruction to the move that created this value
                            tmp = curBasicBlock.firstInstruction;
                            while (tmp.next != null)
                                tmp = tmp.next;
                            phi.neededInstr[1] = tmp;
                            tmp.referencesToThisValue.Add(phi);
                            break;
                        case -1:
                            // We should never get here
                            break;
                    }
                    // Insert it at the end of the join block's instruction list

                    if (curJoinBlock.firstInstruction != null) {
                        Instruction tmpInstr = curJoinBlock.firstInstruction;
                        while (tmpInstr.next != null) {
                            tmpInstr = tmpInstr.next;
                        }
                        tmpInstr.next = phi;
                        phi.prev = tmpInstr;
                    }
                    else {
                        curJoinBlock.firstInstruction = phi;
                    }
                    break;
            }



            curJoinBlock.phiInstructions[symTableID] = phi;
            curJoinBlock.instructionCount++;

            instructionDictionary.Add(lineNumber, phi);
        }

        public void UpdatePhiInstruction(int symbolID, BasicBlock curJoinBlock) {
            // TODO!!!!, change anything with .lineNumber because that isn't set

            Instruction tmp;
            int ifIndex;
            PhiInstruction phi = curJoinBlock.phiInstructions[symbolID];


            switch (curJoinBlock.blockType) {
                case BasicBlock.BlockType.LOOP_HEADER:
                    phi.secondOperand = String.Format("{0}", symbolTable[phi.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
                    phi.secondOperandSSAVal = GetNumFromSSAReg(phi.secondOperand);

                    // Link the phi instruction to the move that created this value
                    tmp = curBasicBlock.firstInstruction;
                    while (tmp.next != null)
                        tmp = tmp.next;
                    if (phi.neededInstr[1] != null)
                        phi.neededInstr[1].referencesToThisValue.Remove(phi);
                    phi.neededInstr[1] = tmp;
                    tmp.referencesToThisValue.Add(phi);
                    break;
                case BasicBlock.BlockType.JOIN:
                    int side = WhichSide(curBasicBlock, joinMatches[curJoinBlock.blockNum]);

                    switch (side) {
                        case 0:
                            // Update the first operand
                            phi.firstOperand = String.Format("{0}", symbolTable[phi.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
                            phi.firstOperandSSAVal = GetNumFromSSAReg(phi.firstOperand);
                            break;
                        case 1:
                            // Update the second operand
                            phi.secondOperand = String.Format("{0}", symbolTable[phi.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
                            phi.secondOperandSSAVal = GetNumFromSSAReg(phi.secondOperand);
                            break;
                        case -1:
                            // Shouldn't ever get here
                            break;
                    }
                    // Link the phi instruction to the move that created this value
                    tmp = curBasicBlock.firstInstruction;
                    while (tmp.next != null)
                        tmp = tmp.next;
                    if (phi.neededInstr[side] != null)
                        phi.neededInstr[side].referencesToThisValue.Remove(phi);
                    phi.neededInstr[side] = tmp;
                    tmp.referencesToThisValue.Add(phi);
                    break;
            }
        }

        public Dictionary<int, PhiInstruction> CommitOuterPhi(ref int lineNumber, BasicBlock curJoinBlock, BasicBlock outerJoinBlock) {
            Dictionary<int, PhiInstruction> commitIDs = new Dictionary<int, PhiInstruction>();

            foreach (KeyValuePair<int, PhiInstruction> phi in curJoinBlock.phiInstructions) {

                int phiKey = phi.Key;
                PhiInstruction curPhi = phi.Value;

                commitIDs.Add(curPhi.symTableID, curPhi);

                if (!outerJoinBlock.phiInstructions.ContainsKey(phiKey)) {
                    Result preBranchVal = new Result(Kind.REG, String.Format("{0}", curPhi.originalVarVal.GetValue()));
                    PhiInstruction outerPhi = new PhiInstruction(lineNumber, curJoinBlock, preBranchVal, curPhi.targetVar);
                    outerPhi.symTableID = curPhi.symTableID;
                    outerPhi.opCode = Token.PHI;

                    switch (outerJoinBlock.blockType) {
                        case BasicBlock.BlockType.LOOP_HEADER:
                            outerPhi.firstOperand = String.Format("({0})", curPhi.instructionNum);
                            outerPhi.firstOperandType = Instruction.OperandType.PHI_OPERAND;
                            outerPhi.firstOperandSSAVal = GetNumFromSSAReg(outerPhi.firstOperand);

                            outerPhi.neededInstr[0] = curPhi;
                            curPhi.referencesToThisValue.Add(outerPhi);

                            SetSecondOperand(outerPhi, preBranchVal);

                            if (preBranchVal.type != Kind.CONST) {
                                outerPhi.neededInstr[1] = instructionDictionary[outerPhi.secondOperandSSAVal];
                                instructionDictionary[outerPhi.secondOperandSSAVal].referencesToThisValue.Add(outerPhi);
                            }

                            // Insert the phis at the front for a loop
                            if (outerJoinBlock.firstInstruction != null) {
                                outerPhi.next = outerJoinBlock.firstInstruction;
                                outerJoinBlock.firstInstruction = outerPhi;
                            }
                            else {
                                outerJoinBlock.firstInstruction = outerPhi;
                            }
                            break;
                        case BasicBlock.BlockType.JOIN:
                            int side = WhichSide(curJoinBlock, joinMatches[outerJoinBlock.blockNum]);
                            switch (side) {
                                case 0:
                                    outerPhi.firstOperand = String.Format("({0})", curPhi.instructionNum);
                                    outerPhi.firstOperandType = Instruction.OperandType.PHI_OPERAND;
                                    outerPhi.firstOperandSSAVal = GetNumFromSSAReg(outerPhi.firstOperand);

                                    outerPhi.neededInstr[0] = curPhi;
                                    curPhi.referencesToThisValue.Add(outerPhi);

                                    SetSecondOperand(outerPhi, preBranchVal);

                                    if (preBranchVal.type != Kind.CONST) {
                                        outerPhi.neededInstr[1] = instructionDictionary[outerPhi.secondOperandSSAVal];
                                        instructionDictionary[outerPhi.secondOperandSSAVal].referencesToThisValue.Add(outerPhi);
                                    }
                                    break;
                                case 1:

                                    SetFirstOperand(outerPhi, preBranchVal);

                                    if (preBranchVal.type != Kind.CONST) {
                                        outerPhi.neededInstr[0] = instructionDictionary[outerPhi.firstOperandSSAVal];
                                        instructionDictionary[outerPhi.firstOperandSSAVal].referencesToThisValue.Add(outerPhi);
                                    }

                                    outerPhi.secondOperand = String.Format("({0})", curPhi.instructionNum);
                                    outerPhi.secondOperandType = Instruction.OperandType.PHI_OPERAND;
                                    outerPhi.secondOperandSSAVal = GetNumFromSSAReg(outerPhi.firstOperand);

                                    outerPhi.neededInstr[1] = curPhi;
                                    curPhi.referencesToThisValue.Add(outerPhi);
                                    break;
                                case -1:
                                    break;
                            }
                            // Insert it at the front of the join block's instruction list
                            if (curJoinBlock.firstInstruction != null) {
                                Instruction tmpInstr = outerJoinBlock.firstInstruction;
                                while (tmpInstr.next != null) {
                                    tmpInstr = tmpInstr.next;
                                }
                                tmpInstr.next = outerPhi;
                                outerPhi.prev = tmpInstr;
                            }
                            else {
                                curJoinBlock.firstInstruction = outerPhi;
                            }
                            break;
                    }



                    outerJoinBlock.phiInstructions[curPhi.symTableID] = outerPhi;
                    outerJoinBlock.instructionCount++;

                    instructionDictionary.Add(lineNumber++, outerPhi);
                }
                else {
                    Instruction outerVersion = outerJoinBlock.phiInstructions[phiKey];
                    switch (outerJoinBlock.blockType) {
                        case BasicBlock.BlockType.LOOP_HEADER:
                            outerVersion.firstOperand = String.Format("({0})", curPhi.instructionNum);
                            outerVersion.firstOperandType = Instruction.OperandType.PHI_OPERAND;
                            outerVersion.firstOperandSSAVal = GetNumFromSSAReg(outerVersion.firstOperand);

                            outerVersion.neededInstr[1] = curPhi;
                            curPhi.referencesToThisValue.Add(outerVersion);
                            break;
                        case BasicBlock.BlockType.JOIN:
                            int side = WhichSide(curJoinBlock, joinMatches[outerJoinBlock.blockNum]);
                            switch (side) {
                                case 0:
                                    outerVersion.firstOperand = String.Format("({0})", curPhi.instructionNum);
                                    outerVersion.firstOperandType = Instruction.OperandType.PHI_OPERAND;
                                    outerVersion.firstOperandSSAVal = GetNumFromSSAReg(outerVersion.firstOperand);

                                    outerVersion.neededInstr[0] = curPhi;
                                    curPhi.referencesToThisValue.Add(outerVersion);
                                    break;
                                case 1:
                                    outerVersion.secondOperand = String.Format("({0})", curPhi.instructionNum);
                                    outerVersion.secondOperandType = Instruction.OperandType.PHI_OPERAND;
                                    outerVersion.secondOperandSSAVal = GetNumFromSSAReg(outerVersion.secondOperand);

                                    outerVersion.neededInstr[1] = curPhi;
                                    curPhi.referencesToThisValue.Add(outerVersion);
                                    break;
                                case -1:
                                    break;
                            }
                            break;
                    }
                }
            }
            return commitIDs;
        }

        public int WhichSide(BasicBlock curBasicBlock, BasicBlock targetBlock) {
            BasicBlock tmp = curBasicBlock;
            while (tmp.dominatingBlock != targetBlock) {
                tmp = tmp.dominatingBlock;
            }

            if (tmp.blockType == BasicBlock.BlockType.TRUE)
                return 0;
            if (tmp.blockType == BasicBlock.BlockType.FALSE)
                return 1;
            return -1;
        }

        private void SetFirstOperand(PhiInstruction phiToSet) {
            phiToSet.firstOperand = String.Format("{0}",
                        symbolTable[phiToSet.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
            phiToSet.firstOperandType = Instruction.OperandType.PHI_OPERAND;
            phiToSet.firstOperandSSAVal = GetNumFromSSAReg(phiToSet.firstOperand);
        }

        private void SetFirstOperand(PhiInstruction phiToSet, Result fromPrior) {
            switch (fromPrior.type) {
                case Kind.REG:
                    phiToSet.firstOperand = fromPrior.GetValue();
                    phiToSet.firstOperandType = Instruction.OperandType.SSA_VAL;
                    phiToSet.firstOperandSSAVal = GetNumFromSSAReg(phiToSet.firstOperand);
                    break;
                case Kind.CONST:
                    phiToSet.firstOperand = fromPrior.GetValue();
                    phiToSet.firstOperandType = Instruction.OperandType.CONSTANT;
                    break;
            }
        }

        private void SetSecondOperand(PhiInstruction phiToSet) {
            phiToSet.secondOperand = String.Format("{0}",
                        symbolTable[phiToSet.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
            phiToSet.secondOperandType = Instruction.OperandType.PHI_OPERAND;
            phiToSet.secondOperandSSAVal = GetNumFromSSAReg(phiToSet.secondOperand);
        }

        private void SetSecondOperand(PhiInstruction phiToSet, Result fromPrior) {
            switch (fromPrior.type) {
                case Kind.REG:
                    phiToSet.secondOperand = fromPrior.GetValue();
                    phiToSet.secondOperandType = Instruction.OperandType.SSA_VAL;
                    phiToSet.secondOperandSSAVal = GetNumFromSSAReg(phiToSet.secondOperand);
                    break;
                case Kind.CONST:
                    phiToSet.secondOperand = fromPrior.GetValue();
                    phiToSet.secondOperandType = Instruction.OperandType.CONSTANT;
                    break;
            }
        }


        public void PropagateHeaderPhis(BasicBlock loopHeader) {
            foreach (KeyValuePair<int, PhiInstruction> pair in loopHeader.phiInstructions) {
                PhiInstruction phi = pair.Value;
                Instruction oldVal = instructionDictionary[GetNumFromSSAReg(phi.originalVarVal.GetValue())];

                foreach (Instruction reference in oldVal.referencesToThisValue) {
                    if (reference != phi) {
                        if (reference.firstOperandSSAVal == oldVal.instructionNum) {
                            reference.firstOperand = String.Format("({0})", phi.instructionNum);
                            reference.firstOperandType = Instruction.OperandType.SSA_VAL;
                            reference.firstOperandSSAVal = GetNumFromSSAReg(reference.firstOperand);

                            reference.neededInstr[0] = phi;
                            phi.referencesToThisValue.Add(reference);
                        }
                        else {
                            reference.secondOperand = String.Format("({0})", phi.instructionNum);
                            reference.secondOperandType = Instruction.OperandType.SSA_VAL;
                            reference.secondOperandSSAVal = GetNumFromSSAReg(reference.firstOperand);

                            reference.neededInstr[1] = phi;
                            phi.referencesToThisValue.Add(reference);
                        }
                    }
                }


            }
        }

        private int GetNumFromSSAReg(string p) {
            return (int)Decimal.Parse(p, NumberStyles.AllowParentheses) * -1;
        }

        private Instruction.OperandType KindToOperandType(Result res) {
            switch (res.type) {
                case Kind.REG:
                    return Instruction.OperandType.REG;
                case Kind.VAR:
                    return Instruction.OperandType.VAR;
                case Kind.CONST:
                    return Instruction.OperandType.CONSTANT;
                case Kind.BRA:
                    return Instruction.OperandType.BRANCH;
                default:
                    Debug.WriteLine("Should not have gotten here");
                    return Instruction.OperandType.ERROR;
            }
        }

        private void InsertAndLink(Instruction curInstruction) {
            if (curBasicBlock.firstInstruction == null) {
                curBasicBlock.firstInstruction = curInstruction;
                curBasicBlock.instructionCount = 1;
            }
            else {
                Instruction tmp = curBasicBlock.firstInstruction;
                while (tmp.next != null)
                    tmp = tmp.next;
                tmp.next = curInstruction;
                curInstruction.prev = tmp;
                curBasicBlock.instructionCount++;
            }

            if (curInstruction.firstOperandType == Instruction.OperandType.SSA_VAL ||
                curInstruction.firstOperandType == Instruction.OperandType.PHI_OPERAND) {
                curInstruction.neededInstr[0] = instructionDictionary[curInstruction.firstOperandSSAVal];
                instructionDictionary[curInstruction.firstOperandSSAVal].referencesToThisValue.Add(curInstruction);
            }

            if (curInstruction.secondOperandType == Instruction.OperandType.SSA_VAL ||
                curInstruction.firstOperandType == Instruction.OperandType.PHI_OPERAND) {
                curInstruction.neededInstr[1] = instructionDictionary[curInstruction.secondOperandSSAVal];
                instructionDictionary[curInstruction.secondOperandSSAVal].referencesToThisValue.Add(curInstruction);
            }
        }
    }
}
