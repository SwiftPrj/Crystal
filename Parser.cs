using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Crystal
{
    public class Parser
    {
        private Memory memory;
        public string path;

        public Parser(string path) { 
            this.path = path;
        }

        public void parse()
        {
            memory = new Memory();
            List<string> code = File.ReadAllLines(path).ToList();
            executeCode(code);
        }

        public bool isInForLoop = false;

        public void handleForLoop(List<string> lines, int i)
        {
            var line = lines[i].Trim();
            if (line.Contains("for("))
            {
                if (line.Contains("->"))
                {
                    var loopLine = i;
                    isInForLoop = true;
                    string loop = line.Replace("for(", String.Empty).Replace("->", String.Empty).Replace(")", String.Empty).Replace("{", String.Empty).Trim();
                    
                    var expression = loop.Split(":");
                    var varName = expression[0].Replace(":", String.Empty).Trim();
                    var type = expression[1].Split("=")[0].Trim();
                    
                    var value = expression[1].Split(";")[0].Split("=")[1].Trim();
                    var valueInt = int.Parse(value);
                    
                    var condition = expression[1].Replace(";", String.Empty).Replace("=", String.Empty).Replace(type, String.Empty).Replace(value, String.Empty).Replace("-", String.Empty).Replace("increment", String.Empty).Trim();
                    var symbol = String.Empty;
                    
                    // create temp var 
                    memory.CreateVar(varName, Type.INT, valueInt);

                    if (condition.Contains('<'))
                    {
                        symbol = "<";
                    }
                    if (condition.Contains('>'))
                    {
                        symbol = ">";
                    }
                    if (condition.Contains("<="))
                    {
                        symbol = "<=";
                    }
                    if (condition.Contains(">="))
                    {
                        symbol = ">=";
                    }
                    var evalValue = expression[1].Replace(";", String.Empty).Replace("=", String.Empty).Replace(type, String.Empty).Replace(value, String.Empty).Replace("-", String.Empty).Replace("increment", String.Empty).Replace(varName, String.Empty).Replace(symbol, String.Empty).Trim();
                    if (condition.Contains(varName))
                    {
                        if (symbol.Equals("<"))
                        {
                            List<string> code = new List<string>();

                            bool endBracket = false;
                            foreach (var bracket in lines)
                            {
                                if (bracket.Contains('}'))
                                {
                                    if (i < lines.IndexOf(bracket))
                                    {
                                        for(int j = i + 1; j < lines.IndexOf(bracket); ++j)
                                        {
                                            code.Add(lines[j]);

                                            if (j == lines.IndexOf(bracket) - 1)
                                            {
                                                endBracket = true;
                                                isInForLoop = false;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }

                            for (int j = int.Parse(value); j < int.Parse(evalValue); j++)
                            {
                                if (j == int.Parse(value)) continue;
                                executeCode(code);

                                foreach (var bracket in lines)
                                {
                                    if (bracket.Contains('}'))
                                    {
                                        if (j == int.Parse(evalValue) - 1)
                                        {
                                            isInForLoop = false;

                                            memory.DeleteVar(varName);
                                            break;
                                        }
                                    }
                                }

                                memory.ChangeValue(varName, j);
                            }
                        }
                    }
                    else 
                    {
                        throw new Exception("Could not find variable named " + varName);
                    }
                }
            }
        }

        public void executeCode(List<string> lines) 
        {
            for (int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i].Trim();
                if (line.Trim() == "}" && isInForLoop)
                {
                    // breaking out of the for loop 
                    isInForLoop = false;
                }
                handleForLoop(lines, i);

                if (line.Contains("func") && line.Contains("->") && line.Contains("(") && line.Contains(")"))
                {
                    int lineNum = i + 1;

                    string declaration = line.Split(":")[0].Trim();
                    string func = declaration.Replace("func", String.Empty).Trim();
                    string type = line.Split(":")[1].Replace("->", String.Empty).Trim();
                    string finalType = String.Empty;
                    if (type.Contains("()")) // empty function 
                    {
                        finalType = type.Replace("()", String.Empty).Trim();
                    }
                    if (!memory.functions.ContainsKey(func))
                    {
                        Console.WriteLine(func);
                    }
                }
                if (line.Contains("var"))
                {
                    int lineNum = i + 1;
                    if (!isInForLoop)
                    {
                        string declaration = line.Split(":")[0].Trim();
                        string variable = declaration.Replace("var", string.Empty).Trim();

                        string type = line.Split(":")[1].Trim();
                        string varType = type.Split("=")[0].Trim();

                        // array logic
                        string explicitType = String.Empty;
                        string typeLiteral = varType;
                        try
                        {
                            explicitType = varType.Split("<")[1].Split(">")[0];
                            typeLiteral = varType.Replace("<" + explicitType + ">", string.Empty);
                        }
                        catch (Exception) { }

                        string value = type.Split("=")[1].Replace(";", string.Empty).Trim();

                        switch (typeLiteral) // var type 
                        {
                            case "int":
                                memory.CreateVar(variable, Type.INT, value);
                                break;
                            case "float":
                                memory.CreateVar(variable, Type.FLOAT, value);
                                break;
                            case "array":
                                string content = value.Replace(" ", string.Empty).Split("{")[1].Split("}")[0];

                                var list = content.Split(",");
                                if (explicitType.Equals("int"))
                                {
                                    int[] array = new int[list.Count()];

                                    for (int uy = 0; uy < list.Count(); ++uy)
                                    {
                                        string s = list[uy];
                                        try
                                        {
                                            array[uy] = int.Parse(s);
                                        }
                                        catch (Exception)
                                        {
                                            /*Console.WriteLine(e.StackTrace);*/
                                            array[uy] = int.Parse(memory.GetValue(s).ToString());
                                        }
                                    }

                                    memory.CreateVar(variable, Type.ARRAYINT, array);
                                }
                                break;
                            default:
                                throw new Exception("Invalid variable type at line " + lineNum);
                        }
                    }
                }

                // functions 
                if (line.Contains("idle"))
                {
                    if (syntaxCheck(line))
                    {
                        Console.ReadKey();
                    }
                    else
                    {
                        throwSyntaxError(i + 1);
                    }
                }
                if (line.Contains("print"))
                {
                    if (syntaxCheck(line))
                    {
                        // check if a variable has been passed 
                        if (line.Contains("\""))
                        {
                            if (CountCharsUsingIndex(line, "\"") <= 2)
                            {
                                var parse = line.Replace(";", "").Replace("print", "").Replace("(", "").Replace(")", "").Replace("\"", "").Trim();
                                Console.WriteLine(parse);
                            }
                            else
                            {
                                throwSyntaxError(i + 1);
                            }
                        }
                        else
                        {
                            var parse = line.Replace(";", string.Empty).Trim();
                            var argument = parse.Replace("print", string.Empty).Split("(")[1].Split(")")[0].Trim();
                            object value = memory.GetValue(argument.Split("[")[0]);

                            if (argument == null)
                            {
                                throw new Exception("Could not find variable named " + argument);
                            }

                            if (memory.IsTypeOf(argument.Split("[")[0], Type.ARRAYINT))
                            {
                                if (argument.Contains("[") && argument.Contains("]"))
                                {
                                    var index_arg = argument.Split("[")[1].Split("]")[0];
                                    int index;
                                    try
                                    {
                                        index = int.Parse(index_arg);
                                    }
                                    catch (Exception)
                                    {
                                        try
                                        {
                                            index = int.Parse(memory.GetValue(index_arg).ToString());
                                        }
                                        catch (Exception) { return; }
                                    }
                                    Console.WriteLine(((int[])value).GetValue(index));
                                }
                            }
                            else
                            {
                                Console.WriteLine(value);
                            }
                        }
                    }
                    else
                    {
                        throwSyntaxError(i + 1);
                    }
                }
            }
        }

        public void throwSyntaxError(int line)
        {
            throw new Exception("Invalid syntax at line " + line);
        }

        public bool syntaxCheck(string line) 
        {
            return line.Contains("(") && line.Contains(")");
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
