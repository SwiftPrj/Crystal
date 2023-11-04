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
    enum Types
    {
        INT,
        DOUBLE,
        FLOAT,
        STRING,
        ARRAY
    }

    public class Parser
    {
        public string path;
        public bool isInForLoop = false;
        public Parser(string path) { this.path = path; }

        public Dictionary<string, object> variables = new Dictionary<string, object>();
        public Dictionary<string, object> values = new Dictionary<string, object>();
        public Dictionary<string, object> functions = new Dictionary<string, object>();
        public ArrayList memory = new ArrayList(); // this is the entire memory pool, everything related to the programming envoirnment (along with keywords!) is stored here 

        public void parse()
        {
            var code = File.ReadAllLines(path);

            // init memory header 
            memory.Add("print");
            memory.Add("idle");
            memory.Add("var");
            memory.Add("func");
/*            string[] tare = new string[] { "idle();" };
            executeString(tare, 0);*/

            for (int i = 0; i < code.Count(); i++) 
            {
                
                executeString(code, i);
                memory.Add(variables);
                memory.Add(values);
            }
            

        }

        public void executeForLoop(string[] lines, int i)
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
                    var condition = expression[1].Replace(";", String.Empty).Replace("=", String.Empty).Replace(type, String.Empty).Replace(value, String.Empty).Replace("-", String.Empty).Replace("increment", String.Empty).Trim();
                    var valueInt = int.Parse(value);
                    var symbol = String.Empty;
                    if (condition.Contains("<"))
                    {
                        symbol = "<";
                    }
                    if (condition.Contains(">"))
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
                        // hard code 
                        switch (symbol)
                        {
                            case "<":
                                Console.WriteLine(symbol);
                                for (int j = int.Parse(value); j < int.Parse(evalValue); j++)
                                {
                                    // TODO: loop the for loop until you find a } and then exit. 
                                    // issue right now is i can reach the } but cant execute the code in the for loop 

                                    foreach (var bracket in lines)
                                    {
                                        if (bracket.Contains("}"))
                                        {
                                            if (j == int.Parse(evalValue) - 1)
                                            {
                                                isInForLoop = false;
                                            }
                                        } 
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    else
                    {
                        throw new Exception("Could not find variable named " + varName);
                    }


                }
            }
        }

        public void executeString(string[] lines, int i) 
        {
            
            var line = lines[i].Trim();
            if (line.Trim() == "}" && isInForLoop)
            {
                // breaking out of the for loop 
                isInForLoop = false;
                Console.WriteLine(isInForLoop);
            }
            executeForLoop(lines, i);
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
                if (!functions.ContainsKey(func))
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
                    string variable = declaration.Replace("var", String.Empty).Trim();
                    string type = line.Split(":")[1].Trim();
                    string varType = type.Split("=")[0].Trim();
                    string value = type.Split("=")[1].Replace(";", "").Trim();
                    switch (varType) // var type 
                    {
                        case "int":
                            variables.Add(variable, Types.INT);
                            values.Add(variable, int.Parse(value));
                            break;
                        case "float":
                            variables.Add(variable, Types.FLOAT);
                            values.Add(variable, float.Parse(value));
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
                            var parse = line.Replace(";", "").Trim();
                            var argument = parse.Replace("print", "").Replace("(", "").Replace(")", "").Replace("\"", "").Trim();
                            Console.WriteLine(argument);
                        }
                        else
                        {
                            throwSyntaxError(i + 1);
                        }
                    }
                    else
                    {
                        var parse = line.Replace(";", "").Trim();
                        var argument = parse.Replace("print", "").Replace("(", "").Replace(")", "").Trim();
                        if (values.ContainsKey(argument))
                        {
                            Console.WriteLine(values.GetValueOrDefault(argument));
                        }
                        else
                        {
                            throw new Exception("Could not find variable named " + argument);
                        }
                    }
                }
                else
                {
                    throwSyntaxError(i + 1);
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
