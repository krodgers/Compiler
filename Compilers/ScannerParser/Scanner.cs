using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScannerParser
{

    // TODO:: built  in function tokens?
    public class Scanner
    {
        // current character on input
        private char? inputSym; //nullable
        private StreamReader input;
        private List<String> identifiers;

        public int PC { get; private set; } // current line number
        public double number; // the last nmber encountered
        public int id; // last identifier encountered


        // Opens file and scans first letter
        public Scanner(String fileName)
        {
            try
            {
                input = new StreamReader(fileName);
            }
            catch (Exception e)
            {
                Error("Failed to open source file.\n" + e.Message);
                System.Environment.Exit(e.HResult);

            }
            inputSym = (char)NextChar();
            identifiers = new List<String>();
            number = Int32.MinValue;
            id = -1;
            PC = 1;
        }

        // Returns current token and advances to next token
        public Token GetSym()
        {
            Token sym = ParseNextToken();
            return sym;
        }

        private Token ParseNextToken()
        {
            if (inputSym == null)
                return Token.EOF;
            Token res = Token.ERROR;
            char currChar = (char)inputSym;
            Token currToken;
            try
            {
                switch (currChar)
                {
                    case (char)255:
                        res = Token.EOF;
                        break;
                    case '*':
                        res = Token.TIMES;
                        break;
                    case '/':
                        if (input.Peek() == '/')
                        {
                            input.ReadLine(); // is a comment and the line should be skipped
                            //PC++; // count the comment line
                            inputSym = NextChar();
                            res = ParseNextToken();
                            return res;
                        }
                        else
                            res = Token.DIV;
                        break;
                    case '+':
                        res = Token.PLUS;
                        break;
                    case '-':
                        res = Token.MINUS;
                        break;
                    case '=': // == 
                        if (input.Peek() == '=')
                        {
                            input.Read(); // eat '='
                            res = Token.EQL;
                        }
                        else
                            Error("Unexpected !\n");
                        break;
                    case '!':
                        if (input.Peek() == '=')
                        {
                            input.Read(); // eat =
                            res = Token.NEQ;
                        }
                        else
                            Error("Unexpected !\n");
                        break;
                    case '<':
                        if (input.Peek() == '=')
                        {
                            res = Token.LEQ;
                            input.Read();
                        }
                        else if (input.Peek() == '-')
                        {
                            res = Token.BECOMES;
                            input.Read();
                        }
                        else if (Char.IsNumber((char)input.Peek()) || Char.IsWhiteSpace((char)input.Peek()))
                            res = Token.LSS;
                        else
                            Error("Invalid character following <\n");

                        break;
                    case '>':
                        if (input.Peek() == '=')
                        {
                            res = Token.GEQ;
                            input.Read();
                        }
                        else if (Char.IsNumber((char)input.Peek()) || Char.IsWhiteSpace((char)input.Peek()))
                            res = Token.GTR;
                        else
                            Error("Invalid character following >\n");
                        break;
                    case '.':
                        res = Token.PERIOD;
                        break;
                    case ',':
                        res = Token.COMMA;
                        break;
                    case '[':
                        res = Token.OPENBRACKET;
                        break;
                    case ']':
                        res = Token.CLOSEBRACKET;
                        break;
                    case '}':
                        res = Token.END;
                        break;
                    case ')':
                        res = Token.CLOSEPAREN;
                        break;
                    case '(':
                        res = Token.OPENPAREN;
                        break;
                    case ';':
                        res = Token.SEMI;
                        break;
                    case '{':
                        res = Token.BEGIN;
                        break;
                    default:
                        res = parseWords();
                        break;

                }
            }
            catch (Exception e)
            {
                Error(e.Message);
            }
            inputSym = NextChar(); // put at beginning of next token
            return res;
        }

        // Returns the next non whitespace character in the file
        private char? NextChar()
        {
            char? curr;
            do
            {
                if (input.EndOfStream)
                    return null;
                
                curr = (char?)input.Read();
                if (curr == '\n') {
                    PC++;
                }

            } while (char.IsWhiteSpace((char)curr));

            return curr;

        }
        // Returns the token corresponding to the word/number at current file position
        private Token parseWords()
        {
            string word = "" + inputSym;
            Token res;
            while (!input.EndOfStream && char.IsLetterOrDigit((char)input.Peek()))
            {
                // consume the whole word
                word += ((char)NextChar()).ToString();
            }
            int num;
            if (Int32.TryParse(word, out num))
            {
                number = num;
                res = Token.NUMBER;
            }
            else if (word.Equals("then"))
                res = Token.THEN;
            else if (word.Equals("do"))
                res = Token.DO;
            else if (word.Equals("od"))
                res = Token.OD;
            else if (word.Equals("fi"))
                res = Token.FI;
            else if (word.Equals("else"))
                res = Token.ELSE;
            else if (word.Equals("let"))
                res = Token.LET;
            else if (word.Equals("call"))
                res = Token.CALL;
            else if (word.Equals("if"))
                res = Token.IF;
            else if (word.Equals("while"))
                res = Token.WHILE;
            else if (word.Equals("return"))
                res = Token.RETURN;
            else if (word.Equals("var"))
                res = Token.VAR;
            else if (word.Equals("array"))
                res = Token.ARR;
            else if (word.Equals("function"))
                res = Token.FUNC;
            else if (word.Equals("procedure"))
                res = Token.PROC;
            else if (word.Equals("main"))
                res = Token.MAIN;
            else
            {
                res = Token.IDENT;
                AddIdent(word);
            }
            return res;
        }

        // Add an identifier to the identifiers list
        // Makes sure id refers to the last symbol seen
        private void AddIdent(String newIdent)
        {
            int idx = identifiers.IndexOf(newIdent);
            if (idx == -1)
            {
                identifiers.Add(newIdent);
                id++;
            } else {
                id = idx;
            }
        }

        // Closes the file
        // Prints out error message
        // Force quits the program
        public void Error(String errMsg)
        {
            if (input != null)
                input.Close();

            Console.Error.Write(errMsg);

        }
        // Returns the identifier with id idx
        public String Id2String(int idx)
        {
            if (idx < identifiers.Count)
                return identifiers[idx];

            return String.Empty;

        }

        // Returns the id of the identifier 
        public int String2Id(String identifier)
        {
            return identifiers.IndexOf(identifier);
        }


    }

}
