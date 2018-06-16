using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScannerParser;
using System.IO;
using System.Diagnostics;

namespace SSAWriterTests {
[TestClass]
    public class ArrayLoadTests {
        static void Main() {
            //TestArrayLoad_1d();

            //TestArrayLoad_2d();

            TestArrayLoad_3d();
            Console.ReadLine();
        }


        public static bool CheckFile(string filename, string[] expected) {
            StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            string result = sr.ReadToEnd();
            string[] delims = { " ", "\n", "\r\n", "1:", "2:", "3:", "4:", "5:", "6:", "7:", "8:", "9:", "0:" };
            string[] splitted = result.Split(delims, StringSplitOptions.RemoveEmptyEntries);


            string[] splittedString = { " ", " ", " " };
            string[] expectedString = { " ", " ", " " };

            for (int s = 0; s < splitted.Length; s++) {
                // Clear out arrays so only one instruction is in them
                if (s % 3 == 0) {
                    splittedString[0] = splittedString[1] = splittedString[2] = " ";
                    expectedString[0] = expectedString[1] = expectedString[2] = " ";
                }

                splittedString[s % 3] = splitted[s];
                expectedString[s % 3] = expected[s];
                if (!splitted[s].Equals(expected[s])) {
                    Console.WriteLine("Failed:");

                    Console.WriteLine("Got: {0} {1} {2} Wanted: {3} {4} {5}", splittedString[0], splittedString[1], splittedString[2], expectedString[0], expectedString[1], expectedString[2]);
                    sr.Dispose();
                    return false;
                }

            }
            sr.Dispose();
            Console.WriteLine("Passed");
            return true;
        }



        public static FileStream OpenStreams(string filename) {
            FileStream fs = File.Open(filename, FileMode.Create, FileAccess.ReadWrite);
            Debug.Assert(fs != null);
            SSAWriter.sw = new StreamWriter(fs);
            return fs;
        }

        [TestMethod]
        public static void TestArrayLoad_1d() {
            string filename = @"../../output.txt";
            FileStream fs = OpenStreams(filename);

            Result arr = new Result(Kind.ARR, "A_BASE");
            int[] dims = { 2 };

            Result i = new Result(Kind.VAR, "i");
            Result two = new Result(Kind.CONST, 2);
            Result zero = new Result(Kind.CONST, (double)0);

            // Get A[i]
            Console.WriteLine("A[i]: ");
            Result[] inds = { i };

            Result finalLineNumber = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            string expectedString = "mul #4 i add FP A_BASE adda (1) (2) load (3)";
            string[] expected = expectedString.Split();
            SSAWriter.sw.Dispose();
            Assert.IsTrue(CheckFile(filename, expected));
    

//            Console.ReadLine();

            // Get A[2]
            Console.WriteLine("A[2]: ");
            OpenStreams(filename);
            inds[0] = two;
            finalLineNumber = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            expectedString = "mul #4 #2 add FP A_BASE adda (1) (2) load (3)";
            expected = expectedString.Split();
            SSAWriter.sw.Dispose();
            Assert.IsTrue(CheckFile(filename, expected));

      //      Console.ReadLine();

            // Get A[0]
            Console.WriteLine("A[0]: ");
            OpenStreams(filename);
            inds[0] = zero;
            finalLineNumber = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            expectedString = "mul #4 #0 add FP A_BASE adda (1) (2) load (3)";
            expected = expectedString.Split();
            SSAWriter.sw.Dispose();
            Assert.IsTrue(CheckFile(filename, expected));
            Assert.AreEqual(5, finalLineNumber);

//            Console.ReadLine();


        }

        [TestMethod]
        public static void TestArrayLoad_2d() {
            // A = new Array[3][4]
            Result arr = new Result(Kind.ARR, "A_BASE");
            int[] dims = { 3,4 };
            string filename = @"../../output.txt";
            Result i = new Result(Kind.VAR, "i");
            Result j = new Result(Kind.VAR, "j");
            Result four = new Result(Kind.CONST, 4);
            Result zero = new Result(Kind.CONST, (double)0);
            Result three = new Result(Kind.CONST, 3);
            Result one = new Result(Kind.CONST, 1);

            string LastPart = " add FP A_BASE adda";
            string expected;
            Result finalLineNum;

            // A[i][j]
            Console.WriteLine("A[i][[j]");
            Result[] inds = { i, j };
            expected = "mul #4 i add j (1) mul #4 (2)" + LastPart + " (3) (4) load (5)";
            string[] expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();
            
            // A[1][j]
            Console.WriteLine("A[1][[j]");
            inds[0] = one; inds[1] = j;
            expected = "add j #4 mul #4 (1) add FP A_BASE adda (2) (3) load (4)";
            expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();

            // A[i][3]
            Console.WriteLine("A[i][[3]");
            inds[0] = i; inds[1] = three;
            expected = "mul #4 i add #3 (1) mul #4 (2) add FP A_BASE adda (3) (4) load (5)";
            expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();

            // A[0][0]
            Console.WriteLine("A[0][[0]");
            inds[0] = zero; inds[1] = zero;
            expected = "mul #4 #0 add FP A_BASE adda (1) (2) load (3)";
            expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();


            // A[0][j]
            Console.WriteLine("A[0][[j]");
            inds[0] = zero; inds[1] = j;
            expected = "mul #4 j add FP A_BASE adda (1) (2) load (3)";
            expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();

            // A[i][0]
            Console.WriteLine("A[i][[0]");
            inds[0] = i; inds[1] = zero;
            expected = "mul #4 i add #0 (1) mul #4 (2) add FP A_BASE adda (3) (4) load (5)";
            expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();


        }

    [TestMethod]
        public static void TestArrayLoad_3d() {
            // A = new Array[5][4][2]
            Result arr = new Result(Kind.ARR, "A_BASE");
            int[] dims = { 5,4,2 };

            Result i = new Result(Kind.VAR, "i");
            Result j = new Result(Kind.VAR, "j");
            Result k = new Result(Kind.VAR, "k");

            string filename = @"../../output.txt";
            Result four = new Result(Kind.CONST, 4);
            Result zero = new Result(Kind.CONST, (double)0);
            Result three = new Result(Kind.CONST, 3);
            Result two = new Result(Kind.CONST, 2);

            string expected;
            Result finalLineNum;

            // A[i][j][k]
            Console.WriteLine("A[i][[j][k]");
            Result[] inds = { i, j , k};
            expected = "mul #8 i mul #2 j add k (1) add (3) (2) mul #4 (4) add FP A_BASE adda (5) (6) load (7)";
            string[] expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();

            // A[3][j][k]
            Console.WriteLine("A[3][[j][k]");
            inds[0] = three; inds[1] = j; inds[2] = k;
            expected = "mul #2 j add k #24 add (2) (1) mul #4 (3) add FP A_BASE adda (4) (5) load (6)";
            expectedCode = Setup(filename, expected);
            finalLineNum = SSAWriter.LoadArrayElement(arr, dims, inds, 1);
            TearDown();
            Assert.IsTrue(CheckFile(filename, expectedCode));
            //CheckFile(filename, expectedCode);
            //Console.ReadLine();

            // A[i][3][k]


            // A[0][0][0]

















        }

        public static string[] Setup(string filename, string expectedString) {
            OpenStreams(filename);
            return expectedString.Split();
        }
        public static void TearDown() {
            SSAWriter.sw.Dispose();
        }
    }
}
