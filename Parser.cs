﻿

using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crystal
{
    public class Parser
    {
        private Memory memory;
        public string path;

        public Parser(string path)
        {
            this.path = path;
        }

        public void parse()
        {
            memory = new Memory();
            string[] code = File.ReadAllLines(path);
            string result = string.Join("", code);
            executeCode(result);
        }
        public bool isInForLoop = false;

        // TODO: isolate code from functions that is not in the Main func
        // for example: if you have a func called "real" and it has a sysout, that sysout shouldn't be executed
        // as long as the func hasn't been invoked in the main entry point 

        public void executeCode(string input)
        {
            string code = RemoveSpacesOutsideQuotes(input);

            // variables
            int varIndex = 0;

            while ((varIndex = code.IndexOf("var", varIndex)) != -1)
            {
                string count = code.Substring(varIndex + "var".Length);
                string name = count.Split(":")[0];
                string value = count.Split(":")[1].Split("=")[1].Split(";")[0];
                string type = count.Split(":")[1].Split("=")[0];
                switch (type)
                {
                    case "int":
                        memory.CreateVar<int>(name, Type.INT, int.Parse(value));
                        break;
                    case "float":
                        memory.CreateVar<float>(name, Type.FLOAT, float.Parse(value));
                        break;
                    case "string":
                        memory.CreateVar<string>(name, Type.STRING, value);
                        break;
                    default:
                        throw new Exception("Invalid return type at variable " + name);
                }
                varIndex += "var".Length;
            }


            // functions
            int funcIndex = 0;

            while ((funcIndex = code.IndexOf("func", funcIndex)) != -1)
            {
                string count = code.Substring(funcIndex + "func".Length);
                string name = count.Split(":")[0];
                string type = count.Split(":")[1].Split("->")[0];
                if (memory.functions.ContainsKey(name))
                {
                    throw new Exception("Function named " + name + " already exists in memory!");
                }
                switch (type)
                {
                    case "int":
                        memory.functions.Add(name, Type.INT);
                        break;
                    case "void":
                        memory.functions.Add(name, Type.VOID);
                        break;
                    default:
                        throw new Exception("Invalid return type at function " + name);
                }
                if (!count.Contains("{"))
                {
                    throwSyntaxError(0);
                }
                if (!count.Contains("}"))
                {
                    throwSyntaxError(0);
                }
                string codeInFunction = count.Split("{")[1].Split("}")[0];
                // store the code of the function 
                if (name != "Main")
                {
                    memory.functionCode.Add(name, codeInFunction);
                }

                if (codeInFunction.Contains("return"))
                {
                    if (memory.GetFuncType(name) != Type.VOID)
                    {
                        int returnPos = codeInFunction.IndexOf("return");
                        int returnVal = int.Parse(codeInFunction.Substring(returnPos + 1).Split("n")[1].Replace(";", ""));
                        if (name == "Main")
                        {
                            Environment.ExitCode = returnVal;
                        }
                        else
                        {
                            // TODO: implement returnization in program memory  
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot return a value while the function type is of type void");
                    }

                } else if (memory.GetFuncType(name) != Type.VOID)
                {
                    throw new Exception("Function " + name + " must return a value (type != void)");
                }
                funcIndex += "func".Length;
            }

            if (!memory.functions.ContainsKey("Main"))
            {
                throw new Exception("Could not find entry point for program");
            }
            else
            {
                if (memory.functions.GetValueOrDefault("Main") != Type.INT)
                {
                    throw new Exception("Entry point function does not have the correct type (expected integer)");
                }
            }

            // sysout
            int sysoutIndex = 0;

            while ((sysoutIndex = code.IndexOf("sysout", sysoutIndex)) != -1)
            {
                string count = code.Substring(sysoutIndex + "sysout".Length);
                string arrow = count.Substring(0, 2);
                if (arrow == "->")
                {
                    if (count.Contains("\""))
                    {
                        if (count.Contains(";"))
                        {
                            Console.WriteLine(count.Split("\"")[1]);
                        }
                        else { throwSyntaxError(0); }
                    } else
                    {
                        if (count.Contains(";")) 
                        {
                            Console.WriteLine(memory.GetValue(count.Split(";")[0].Split("->")[1]));
                        }
                    }

                }
                else { throwSyntaxError(0); }
                sysoutIndex += "sysout".Length;
            }
        }
        
        public bool HasValidSyntax(string result)
        {
            return !HasDuplicateCharacters(result, "\"") && !HasDuplicateCharacters(result, ";");
        }

        public bool HasDuplicateCharacters(string input, string charToCheck)
        {
            int charCount = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].ToString() == charToCheck)
                {
                    charCount++;
                    
                    if (charCount > 1)
                    {
                        return true; 
                    }
                }
            }

            return false; 
        }

        public int GetDuplicateCharacters(string input, string charToCheck)
        {
            int charCount = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].ToString() == charToCheck)
                {
                    charCount++;
                    if (charCount >= 1)
                    {
                        return charCount;
                    }
                }
            }

            return 0;
        }

        public void throwSyntaxError(int line)
        {
            throw new Exception("Invalid syntax at line " + line);
        }

        public string RemoveSpacesOutsideQuotes(string input)
        {
            return Regex.Replace(input, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1");
        }

        public int CountCharsUsingIndex(string source, string toFind)
        {
            int count = 0;
            int n = 0;
            while ((n = source.IndexOf(toFind, n) + 1) != 0)
            {
                n++;
                count++;
            }
            return count;
        }

    }
}
