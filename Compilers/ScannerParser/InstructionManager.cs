﻿using System;
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

        public void PutPhiInstruction(int lineNumber, BasicBlock curJoinBlock, Dictionary<int, BasicBlock> joinParentBlocks, int symTableID, Result oldVarVal, string symbolName) {

            Instruction tmp;

            Result preBranchVal = oldVarVal;
            PhiInstruction phi = new PhiInstruction(lineNumber, curJoinBlock, preBranchVal, symbolName);
            phi.symTableID = symTableID;
            phi.augmentedSymbolID = String.Format("{0}_{1}", symTableID, curBasicBlock.blockNum);
            phi.opCode = Token.PHI;

            // todo, need to check curJoinBlock type, not the cur basic block type. Also
            // need to make sure that each type of cur basic block will be updated with a phi
            // properly. There needs to be a phi for EVERY assignment, regardless of what type
            // of block it is currently

            // todo, the above is solved for loops, but still need to figure out how
            // to decide which side of the phi instruction to place the new assignments on for ifs,
            // as we won't know what nodes connect to the join blocks yet

            phi.firstOperand = preBranchVal.GetValue();
            if (preBranchVal.type == Kind.CONST) {
                phi.firstOperandType = Instruction.OperandType.CONSTANT;
            }
            else {
                phi.firstOperandType = Instruction.OperandType.SSA_VAL;
                phi.firstOperandSSAVal = GetNumFromSSAReg(phi.firstOperand);

                // Link the phi instruction to the move that created this value and back
                phi.neededInstr[0] = instructionDictionary[phi.firstOperandSSAVal];
                instructionDictionary[phi.firstOperandSSAVal].referencesToThisValue.Add(phi);
            }

            phi.secondOperand = String.Format("{0}", symbolTable[phi.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
            phi.secondOperandType = Instruction.OperandType.PHI_OPERAND;
            phi.secondOperandSSAVal = GetNumFromSSAReg(phi.secondOperand);

            // Link the phi instruction to the move that created this value
            tmp = curBasicBlock.firstInstruction;
            while (tmp.next != null)
                tmp = tmp.next;
            phi.neededInstr[1] = tmp;
            tmp.referencesToThisValue.Add(phi);


            // Insert it at the end of the join block's instruction list


            if (curJoinBlock.firstInstruction != null) {
                tmp = curJoinBlock.firstInstruction;
                while (tmp.next != null)
                    tmp = tmp.next;
                tmp.next = phi;
                phi.prev = tmp;

            }
            else {
                curJoinBlock.firstInstruction = phi;
            }

            curJoinBlock.phiInstructions[phi.symTableID] = phi;

            curJoinBlock.instructionCount++;

            instructionDictionary.Add(lineNumber, phi);
        }

        public Dictionary<int, PhiInstruction> ResolvePhis(BasicBlock curJoinBlock, ref int lineNumber)
        {
            var allKeys = curJoinBlock.sideOnePhis.Keys.Union(curJoinBlock.sideTwoPhis.Keys);
            Dictionary<int, PhiInstruction> finalPhis = new Dictionary<int, PhiInstruction>();

            foreach (int key in allKeys)
            {
                if (curJoinBlock.sideOnePhis.ContainsKey(key) && !curJoinBlock.sideTwoPhis.ContainsKey(key))
                {
                    finalPhis[key] = curJoinBlock.sideOnePhis[key];
                }
                else if (!curJoinBlock.sideOnePhis.ContainsKey(key) && curJoinBlock.sideTwoPhis.ContainsKey(key))
                {
                    finalPhis[key] = curJoinBlock.sideTwoPhis[key];
                }
                else if (curJoinBlock.sideOnePhis.ContainsKey(key) && curJoinBlock.sideTwoPhis.ContainsKey(key))
                {

                    PhiInstruction sideOneInstruction = curJoinBlock.sideOnePhis[key];
                    PhiInstruction sideTwoInstruction = curJoinBlock.sideTwoPhis[key];

                    PhiInstruction newPhi = new PhiInstruction(lineNumber, curJoinBlock, sideOneInstruction.originalVarVal,
                        sideOneInstruction.targetVar);

                    newPhi.opCode = Token.PHI;
                    newPhi.symTableID = sideOneInstruction.symTableID;
                    newPhi.augmentedSymbolID = newPhi.symTableID + "_" + curJoinBlock.blockNum;

                    if (sideOneInstruction.firstOperand == sideTwoInstruction.firstOperand)
                    {
                        newPhi.firstOperand = sideOneInstruction.secondOperand;
                        newPhi.firstOperandSSAVal = sideOneInstruction.secondOperandSSAVal;
                        newPhi.firstOperandType = sideOneInstruction.secondOperandType;

                        newPhi.secondOperand = sideTwoInstruction.secondOperand;
                        newPhi.secondOperandSSAVal = sideTwoInstruction.secondOperandSSAVal;
                        newPhi.secondOperandType = sideTwoInstruction.secondOperandType;
                    }
                    else if (sideOneInstruction.firstOperand == sideTwoInstruction.secondOperand)
                    {
                        newPhi.firstOperand = sideOneInstruction.secondOperand;
                        newPhi.firstOperandSSAVal = sideOneInstruction.secondOperandSSAVal;
                        newPhi.firstOperandType = sideOneInstruction.secondOperandType;

                        newPhi.secondOperand = sideTwoInstruction.firstOperand;
                        newPhi.secondOperandSSAVal = sideTwoInstruction.firstOperandSSAVal;
                        newPhi.secondOperandType = sideTwoInstruction.firstOperandType;
                    }
                    else if (sideOneInstruction.secondOperand == sideTwoInstruction.firstOperand)
                    {
                        newPhi.firstOperand = sideOneInstruction.firstOperand;
                        newPhi.firstOperandSSAVal = sideOneInstruction.firstOperandSSAVal;
                        newPhi.firstOperandType = sideOneInstruction.firstOperandType;

                        newPhi.secondOperand = sideTwoInstruction.secondOperand;
                        newPhi.secondOperandSSAVal = sideTwoInstruction.secondOperandSSAVal;
                        newPhi.secondOperandType = sideTwoInstruction.secondOperandType;
                    }
                    else
                    {
                        newPhi.firstOperand = sideOneInstruction.firstOperand;
                        newPhi.firstOperandSSAVal = sideOneInstruction.firstOperandSSAVal;
                        newPhi.firstOperandType = sideOneInstruction.firstOperandType;

                        newPhi.secondOperand = sideTwoInstruction.firstOperand;
                        newPhi.secondOperandSSAVal = sideTwoInstruction.firstOperandSSAVal;
                        newPhi.secondOperandType = sideTwoInstruction.firstOperandType;
                    }

                    instructionDictionary.Add(lineNumber, newPhi);

                    // Link this instruction to the rest
                    InsertAndLink(newPhi);

                    // add the instruction to the returning structure
                    finalPhis.Add(lineNumber, newPhi);

                    lineNumber++;
                }
                else
                {
                    // shouldn't get here
                }
            }

            PhiInstruction phi = (PhiInstruction) curJoinBlock.firstInstruction;
            while (phi.next != null)
            {
                if (!finalPhis.ContainsValue(phi))
                {
                    if (phi.prev == null)
                    {
                        if (phi.next == null)
                        {
                            curJoinBlock.firstInstruction = null;
                        }
                        else
                        {
                            curJoinBlock.firstInstruction = phi.next;
                            phi.next.prev = null;
                        }
                        
                    }
                    else if (phi.next == null)
                    {
                        phi.prev.next = null;
                    }
                    else
                    {
                        phi.prev.next = phi.next;
                        phi.next.prev = phi.prev;
                    }
                }
                curJoinBlock.instructionCount--;
                phi = (PhiInstruction) phi.next;
            }
            return finalPhis;
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
                    switch (curBasicBlock.blockType) {
                        case BasicBlock.BlockType.TRUE:
                        case BasicBlock.BlockType.FALSE:

                            if (curBasicBlock.blockType == BasicBlock.BlockType.TRUE) {
                                phi.firstOperand = String.Format("{0}", symbolTable[phi.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
                                phi.firstOperandSSAVal = GetNumFromSSAReg(phi.firstOperand);
                                ifIndex = 0;
                            }
                            else {
                                phi.secondOperand = String.Format("{0}", symbolTable[phi.symTableID].GetCurrentValue(curBasicBlock.scopeNumber).GetValue());
                                phi.secondOperandSSAVal = GetNumFromSSAReg(phi.secondOperand);
                                ifIndex = 1;
                            }

                            // Link the phi instruction to the move that created this value
                            tmp = curBasicBlock.firstInstruction;
                            while (tmp.next != null)
                                tmp = tmp.next;
                            if (phi.neededInstr[ifIndex] != null)
                                phi.neededInstr[ifIndex].referencesToThisValue.Remove(phi);
                            phi.neededInstr[ifIndex] = tmp;
                            tmp.referencesToThisValue.Add(phi);
                            break;
                    }
                    break;
            }
        }


        public void RemoveUnnecessaryPhis(BasicBlock curJoinBlock, ref int lineNumber) {

            Dictionary<int, Dictionary<string, PhiInstruction>> phiSelections = new Dictionary<int, Dictionary<string, PhiInstruction>>();

            if (curJoinBlock.firstInstruction.opCode == Token.PHI) {

                PhiInstruction curInstruction = (PhiInstruction)curJoinBlock.firstInstruction;

                while (curInstruction != null) {
                    string augmentedSymbolID = curInstruction.augmentedSymbolID;
                    int symbolID = RemoveAugmentation(augmentedSymbolID);


                    if (phiSelections.ContainsKey(symbolID)) {
                        if (phiSelections[symbolID].ContainsKey(augmentedSymbolID)) {

                            PhiInstruction oldInstruction = phiSelections[symbolID][augmentedSymbolID];

                            if (oldInstruction != curJoinBlock.firstInstruction) {
                                oldInstruction.prev.next = oldInstruction.next;
                                oldInstruction.next.prev = oldInstruction.prev;
                            }
                            else {
                                curJoinBlock.firstInstruction = curJoinBlock.firstInstruction.next;
                                curJoinBlock.firstInstruction.prev = null;
                            }

                            phiSelections[symbolID][augmentedSymbolID] = curInstruction;
                        }
                        else {
                            phiSelections[symbolID][augmentedSymbolID] = curInstruction;
                        }
                    }
                    else {
                        phiSelections[symbolID] = new Dictionary<string, PhiInstruction>();
                        phiSelections[symbolID][augmentedSymbolID] = curInstruction;
                    }

                    try {
                        if (curInstruction.next.opCode != Token.PHI)
                            break;
                    }
                    catch {
                        break;
                    }
                    curInstruction = (PhiInstruction)curInstruction.next;
                }


                Dictionary<int, PhiInstruction> finalPhis = new Dictionary<int, PhiInstruction>();
                foreach (KeyValuePair<int, Dictionary<string, PhiInstruction>> val in phiSelections) {
                    int symbolID = val.Key;
                    Dictionary<string, PhiInstruction> chosenPhis = val.Value;

                    List<PhiInstruction> phisList = new List<PhiInstruction>();
                    foreach (KeyValuePair<string, PhiInstruction> curElement in chosenPhis) {
                        phisList.Add(curElement.Value);
                    }

                    if (phisList.Count < 2) {
                        phisList[0].augmentedSymbolID = String.Format("{0}_{1}", symbolID, curJoinBlock.blockNum);
                        instructionDictionary.Remove(phisList[0].instructionNum);
                        phisList[0].instructionNum = lineNumber;
                        finalPhis.Add(lineNumber, phisList[0]);
                        instructionDictionary[lineNumber] = phisList[0];
                        lineNumber++;
                        continue;
                    }

                    PhiInstruction newPhi = new PhiInstruction(lineNumber, curJoinBlock, phisList[0].originalVarVal,
                        phisList[0].targetVar);

                    newPhi.opCode = Token.PHI;
                    newPhi.symTableID = symbolID;


                    if (phisList[0].firstOperandSSAVal == phisList[1].firstOperandSSAVal) {
                        newPhi.firstOperand = phisList[0].secondOperand;
                        newPhi.firstOperandSSAVal = phisList[0].secondOperandSSAVal;
                        newPhi.firstOperandType = phisList[0].secondOperandType;

                        newPhi.secondOperand = phisList[1].secondOperand;
                        newPhi.secondOperandSSAVal = phisList[1].secondOperandSSAVal;
                        newPhi.secondOperandType = phisList[1].secondOperandType;
                    }
                    else if (phisList[0].firstOperandSSAVal == phisList[1].secondOperandSSAVal) {
                        newPhi.firstOperand = phisList[0].secondOperand;
                        newPhi.firstOperandSSAVal = phisList[0].secondOperandSSAVal;
                        newPhi.firstOperandType = phisList[0].secondOperandType;

                        newPhi.secondOperand = phisList[1].firstOperand;
                        newPhi.secondOperandSSAVal = phisList[1].firstOperandSSAVal;
                        newPhi.secondOperandType = phisList[1].firstOperandType;
                    }
                    else if (phisList[0].secondOperandSSAVal == phisList[1].firstOperandSSAVal) {
                        newPhi.firstOperand = phisList[0].firstOperand;
                        newPhi.firstOperandSSAVal = phisList[0].firstOperandSSAVal;
                        newPhi.firstOperandType = phisList[0].firstOperandType;

                        newPhi.secondOperand = phisList[1].secondOperand;
                        newPhi.secondOperandSSAVal = phisList[1].secondOperandSSAVal;
                        newPhi.secondOperandType = phisList[1].secondOperandType;
                    }
                    else {
                        newPhi.firstOperand = phisList[0].firstOperand;
                        newPhi.firstOperandSSAVal = phisList[0].firstOperandSSAVal;
                        newPhi.firstOperandType = phisList[0].firstOperandType;

                        newPhi.secondOperand = phisList[1].firstOperand;
                        newPhi.secondOperandSSAVal = phisList[1].firstOperandSSAVal;
                        newPhi.secondOperandType = phisList[1].firstOperandType;
                    }

                    for (int i = 0; i < phisList.Count; i++) {
                        // Remove the old phi
                        if (phisList[i] != curJoinBlock.firstInstruction) {
                            if (phisList[i].next != null) {
                                phisList[i].prev.next = phisList[i].next;
                                phisList[i].next.prev = phisList[i].prev;
                            }
                            else {
                                phisList[i].prev.next = null;
                            }
                        }
                        else {
                            if (curJoinBlock.firstInstruction.next != null) {
                                curJoinBlock.firstInstruction = curJoinBlock.firstInstruction.next;
                                curJoinBlock.firstInstruction.prev = null;
                            }
                            else {
                                curJoinBlock.firstInstruction = null;
                            }
                        }
                    }



                    newPhi.augmentedSymbolID = String.Format("{0}_{1}", symbolID, curJoinBlock.blockNum);


                    instructionDictionary.Add(lineNumber, newPhi);

                    // Link this instruction to the rest
                    InsertAndLink(newPhi);

                    // add the instruction to the returning structure
                    finalPhis.Add(lineNumber, newPhi);

                    lineNumber++;
                }
                curJoinBlock.phiInstructions = finalPhis;
            }

        }

        public int RemoveAugmentation(string augmentedSymbolID) {
            string symbolID;
            int index = augmentedSymbolID.IndexOf("_");
            if (index > 0)
                symbolID = augmentedSymbolID.Substring(0, index);
            else
                return -1;
            return (int)Double.Parse(symbolID);
        }

        public Dictionary<int, PhiInstruction> CommitOuterPhi(ref int lineNumber, BasicBlock curJoinBlock, BasicBlock outerJoinBlock) {
            Dictionary<int, PhiInstruction> commitIDs = new Dictionary<int, PhiInstruction>();

            Instruction tmp;

            foreach (KeyValuePair<int, PhiInstruction> phi in curJoinBlock.phiInstructions) {

                int phiKey = phi.Key;
                PhiInstruction curPhi = phi.Value;

                commitIDs.Add(curPhi.symTableID, curPhi);

                Result preBranchVal = new Result(Kind.REG, String.Format("{0}", curPhi.originalVarVal.GetValue()));
                PhiInstruction outerPhi = new PhiInstruction(lineNumber, curJoinBlock, preBranchVal, curPhi.targetVar);
                outerPhi.symTableID = curPhi.symTableID;
                outerPhi.opCode = Token.PHI;
                outerPhi.augmentedSymbolID = String.Format("{0}_{1}", curPhi.symTableID, curBasicBlock.blockNum);

                outerPhi.firstOperand = String.Format("({0})", curPhi.instructionNum);
                outerPhi.firstOperandType = Instruction.OperandType.PHI_OPERAND;
                outerPhi.firstOperandSSAVal = GetNumFromSSAReg(outerPhi.firstOperand);

                outerPhi.neededInstr[0] = curPhi;
                curPhi.referencesToThisValue.Add(outerPhi);

                outerPhi.secondOperand = preBranchVal.GetValue();
                if (preBranchVal.type == Kind.CONST)
                {
                    outerPhi.secondOperandType = Instruction.OperandType.CONSTANT;
                }
                else
                {
                    outerPhi.secondOperandType = Instruction.OperandType.SSA_VAL;
                    outerPhi.secondOperandSSAVal = GetNumFromSSAReg(outerPhi.firstOperand);

                    outerPhi.neededInstr[1] = instructionDictionary[outerPhi.secondOperandSSAVal];
                    instructionDictionary[outerPhi.secondOperandSSAVal].referencesToThisValue.Add(outerPhi);
                }

               
                // Insert it at the front of the join block's instruction list
                if (outerJoinBlock.firstInstruction != null) {
                    tmp = outerJoinBlock.firstInstruction;
                    while (tmp.next != null)
                        tmp = tmp.next;
                    tmp.next = outerPhi;
                    outerPhi.prev = tmp;

                }
                else {
                    curJoinBlock.firstInstruction = outerPhi;
                }

                outerJoinBlock.phiInstructions[curPhi.symTableID] = outerPhi;
                outerJoinBlock.instructionCount++;

                instructionDictionary.Add(lineNumber++, outerPhi);

            }
            return commitIDs;
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
