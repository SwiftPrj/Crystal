﻿using Crystal.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Functions
{
    public class FunctionTable
    {
        private readonly Dictionary<string, Function> functions = new Dictionary<string, Function>();

        public void AddFunction(string name, List<(string, string)> parameters, List<Token> body)
        {
            if (functions.ContainsKey(name))
            {
                throw new Exception($"function '{name}' is already defined.");
            }
            functions[name] = new Function(name, parameters, body);
        }

        public Function GetFunction(string name)
        {
            if (functions.ContainsKey(name))
            {
                return functions[name];
            }
            throw new Exception($"function '{name}' is not defined.");
        }
    }
}
