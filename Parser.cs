using System;
using System.Collections;
using System.Collections.Generic;
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
        ARRAY,


    }

    public class Parser
    {
        public string path;
        public Parser(string path) { this.path = path; }

        public Dictionary<String, Object> variables = new Dictionary<String, Object>();
        public Dictionary<String, Object> values = new Dictionary<String, Object>();
        public ArrayList memory = new ArrayList(); // this is the entire memory pool, everything related to the programming envoirnment (along with keywords!) is stored here 

        public void parse()
        {
            var code = File.ReadAllLines(path);

            // init memory header 
            memory.Add("print");
            memory.Add("idle");
            memory.Add("var");

            for (int i = 0; i < code.Count(); i++) 
            {

                var line = code[i].Trim(); 
                if (line.Contains("var"))
                {
                    int lineNum = i + 1;

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

                // functions 
                if (line.Contains("idle"))
                {
                    if (syntaxCheck(line))
                    {
                        Console.ReadKey();
                    } else
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
                memory.Add(variables);
                memory.Add(values);
            }
            

        }

        public void throwSyntaxError(int line)
        {
            throw new Exception("Invalid syntax at line " + line);
        }

        public bool syntaxCheck(string line) 
        {
            if (line.Contains("(") && line.Contains(")"))
            {
                return true;
            }
            return false;
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
