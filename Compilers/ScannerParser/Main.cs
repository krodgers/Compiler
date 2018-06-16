using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser {
    class MainClass {
        static void Main(string[] args) {

             //Parser p = new Parser("testFiles/test001.txt");
            //Parser p = new Parser("testFiles/factorial.txt");
             //Parser p = new Parser("testFiles/FunctionTester.txt");
            // Parser p = new Parser("testFiles/test003.txt");
            // Parser p = new Parser("testFiles/test007.txt");
             Parser p = new Parser("testFiles/test006.txt");
             //Parser p = new Parser(@"testFiles/test017.txt");


            // parse
            BasicBlock start = null;
            p.StartFirstPass(ref start);
            List<Symbol> symtable = p.ExportSymbolTable();
            int numLnes;

            // order blocks
            Queue<BasicBlock> res = Utilities.TraverseCFG(ref start);

            Console.WriteLine("\n\nOriginal Program: ");
            foreach (BasicBlock bl in res)
                SSAWriter.WriteBlock(bl);
            Console.WriteLine("\n\n");
            int lastLineNo;

            // copy propagation
            Console.WriteLine("Copy Propagation");
            Queue<BasicBlock> prpped = CodifierPrep.PerformCopyPropagation(start, null, out lastLineNo);


            // Codify
            Console.WriteLine("\n\nOrdered Blocks: ");
            Codifier coder = new Codifier(@"../../assem_17.txt", lastLineNo);
            foreach (BasicBlock bl in prpped) {
                SSAWriter.WriteBlock(bl);

            }

            Console.WriteLine("\n\nCodifying");
            foreach (BasicBlock bl in prpped) {
                coder.CodifyBlock(bl);
            }
            coder.CloseFiles();
            SSAWriter.sw.Dispose();
            Console.ReadLine();




           Console.ReadLine();
        }
    }

}




           