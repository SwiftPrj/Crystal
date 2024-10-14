using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crystal
{
    public class Parser
    {
        private Memory memory;
        public string path;

        public Parser(string path) => this.path = path;

        public void parse()
        {
            memory = new Memory();
            string[] code = File.ReadAllLines(path);
            string result = string.Join("", code);
            executeCode(result);
        }

        public List<Action> parseCode(string input, string context)
        {
            List<Action> actions = new();

            string[] lines = input.Split(";");
            foreach (string code in lines)
            {
                // sysin

                int sysinIndex = 0;
                while ((sysinIndex = code.IndexOf("sysin", sysinIndex)) != -1)
                {
                    string count = code.Substring(sysinIndex + "sysin".Length).Trim();
                    string arrow = count.Substring(0, 2);
                    if (arrow == "->")
                    {
                        string varName = count.Substring(2).Trim();
                        actions.Add(() =>
                        {
                            string inputValue = Console.ReadLine();
                            var type = memory.GetVariable(varName, context).GetType();
                            switch (type)
                            {
                                case Type.FLOAT:
                                    float floatVal = float.Parse(inputValue);
                                    memory.GetVariable(varName, context).ChangeValue(floatVal);
                                    break;
                                case Type.STRING:
                                    memory.GetVariable(varName, context).ChangeValue(inputValue);
                                    break;
                                case Type.INT:
                                    int value = int.Parse(inputValue);
                                    memory.GetVariable(varName, context).ChangeValue(value);
                                    break;
                                default:
                                    throwSyntaxError(0);
                                    break;
                            }
                        });
                    }
                    else
                    {
                        throwSyntaxError(0);
                    }
                    sysinIndex += "sysin".Length;
                }

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
                            memory.CreateVariable(name, context, Type.INT, int.Parse(value));
                            break;
                        case "float":
                            memory.CreateVariable(name, context, Type.FLOAT, float.Parse(value));
                            break;
                        case "string":
                            memory.CreateVariable(name, context, Type.STRING, value.Replace("\"", String.Empty));
                            break;
                        default:
                            throw new Exception("Invalid return type at variable " + name);
                    }
                    varIndex += "var".Length;
                }

                // sysout
                int sysoutIndex = 0;
                while ((sysoutIndex = code.IndexOf("sysout", sysoutIndex)) != -1)
                {
                    string count = code.Substring(sysoutIndex + "sysout".Length).Trim();
                    string arrow = count.Substring(0, 2);

                    if (arrow == "->")
                    {
                        string expression = count.Substring(2).Trim().TrimEnd(';');
                        actions.Add(() =>
                        {
                            string result = EvaluateExpression(expression, context);
                            Console.WriteLine(result);
                        });
                    }
                    else
                    {
                        throwSyntaxError(0);
                    }
                    sysoutIndex += "sysout".Length;
                }


                foreach (Function func in memory.functions)
                {
                    string f_name = func.Name;

                    int fnIndex = 0;
                    while ((fnIndex = code.IndexOf(f_name, fnIndex)) != -1)
                    {
                        string args = code.Substring(fnIndex + f_name.Length).Split("(")[1].Split(")")[0];
                        string count = code.Substring(fnIndex + f_name.Length, args.Length + 2);

                        foreach (Action act in func.Actions)
                        {
                            actions.Add(act);
                        }
                        fnIndex += f_name.Length;
                    }
                }
            }

            return actions;
        }

        public bool EvaluateCondition(string condition, string context)
        {
            string[] parts;
            if (condition.Contains("=="))
            {
                parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
                return GetVariableValue(parts[0].Trim(), context) == GetVariableValue(parts[1].Trim(), context);
            }
            else if (condition.Contains("!="))
            {
                parts = condition.Split(new[] { "!=" }, StringSplitOptions.None);
                return GetVariableValue(parts[0].Trim(), context) != GetVariableValue(parts[1].Trim(), context);
            }
            else if (condition.Contains("<"))
            {
                parts = condition.Split(new[] { "<" }, StringSplitOptions.None);
                return GetVariableValue(parts[0].Trim(), context) < GetVariableValue(parts[1].Trim(), context);
            }
            else if (condition.Contains(">"))
            {
                parts = condition.Split(new[] { ">" }, StringSplitOptions.None);
                return GetVariableValue(parts[0].Trim(), context) > GetVariableValue(parts[1].Trim(), context);
            }
            else
            {
                throw new Exception("Invalid condition: " + condition);
            }
        }

        private dynamic GetVariableValue(string varName, string context)
        {
            Variable var = memory.GetVariable(varName, context);
            if (var != null)
            {
                return var.Value;
            }
            throw new Exception("Variable " + varName + " does not exist in context " + context);
        }

        public string EvaluateExpression(string expression, string context)
        {
            StringBuilder result = new StringBuilder();
            string[] parts = expression.Split('+');

            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();

                if (trimmedPart.StartsWith("\"") && trimmedPart.EndsWith("\""))
                {
                    result.Append(trimmedPart.Trim('"')); 
                }
                else
                {
                    if (memory.VarExists(trimmedPart, context))
                    {
                        Variable var = memory.GetVariable(trimmedPart, context);
                        if (var != null)
                        {
                            result.Append(var.Value);
                        }
                        else
                        {
                            throw new Exception("Variable " + trimmedPart + " does not exist in function " + context);
                        }
                    } else // if the var doesn't exist, try passing it as a function instead
                    {
                        int index = trimmedPart.IndexOf("(");
                        Function var = memory.GetFunction(trimmedPart.Substring(0, index));
                        if (var != null)
                        {
                            result.Append(var.Return);
                        }
                        else
                        {
                            throw new Exception("Function " + trimmedPart + " does not exist");
                        }
                    }
                }
            }

            return result.ToString();
        }

        public void executeCode(string input)
        {
            string code = RemoveSpacesOutsideQuotes(input);



            // functions
            int funcIndex = 0;

            while ((funcIndex = code.IndexOf("func", funcIndex)) != -1)
            {
                string count = code.Substring(funcIndex + "func".Length);
                string name = count.Split(":")[0];
                string type = count.Split(":")[1].Split("->")[0];
                if (memory.FuncExists(name))
                {
                    throw new Exception("Function named " + name + " already exists in memory!");
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
                switch (type)
                {
                    case "int":
                        memory.CreateFunction(name, Type.INT);
                        break;
                    case "void":
                        memory.CreateFunction(name, Type.VOID);
                        break;
                    case "string":
                        memory.CreateFunction(name, Type.STRING);
                        break;
                    default:
                        throw new Exception("Invalid return type at function " + name);
                }

                // store the code of the function
                memory.GetFunction(name).Actions = parseCode(codeInFunction, name);

                if (codeInFunction.Contains("return"))
                {
                    if (memory.GetFunction(name).Type != Type.VOID)
                    {
                        int returnPos = codeInFunction.IndexOf("return");

                        var returnVal = codeInFunction.Substring(returnPos + 1).Split("n")[1].Replace(";", "");
                        if (name == "Main")
                        {
                            Environment.ExitCode = int.Parse(codeInFunction.Substring(returnPos + 1).Split("n")[1].Replace(";", "")); 
                        }
                        else
                        {
                            if (memory.VarExists(returnVal, name))
                            {

                                memory.GetFunction(name).Return = memory.GetVariable(returnVal, name).Value;
              

                            } else if (memory.GetFunction(name).Type == Type.STRING)
                            {
                                Console.WriteLine(returnVal);
                                if (memory.VarExists(returnVal, name))
                                {

                                    memory.GetFunction(name).Return = returnVal.Replace("\"", String.Empty);
                                } else
                                {
                                    throw new Exception("Variable " + name + " does not exist!");
                                }
                            } else if (!memory.VarExists(returnVal, name))
                            {
                                memory.GetFunction(name).Return = returnVal;
                            }
                                
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot return a value while the function type is of type void");
                    }

                }
                else if (memory.GetFunction(name).Type != Type.VOID)
                {
                    throw new Exception("Function " + name + " must return a value (type != void)");
                }
                funcIndex += "func".Length;
            }

            if (!memory.FuncExists("Main"))
            {
                throw new Exception("Could not find entry point for program");
            }
            else
            {
                if (memory.GetFunction("Main").Type != Type.INT)
                {
                    throw new Exception("Entry point function does not have the correct type (expected integer)");
                }

                memory.GetFunction("Main").Run();
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