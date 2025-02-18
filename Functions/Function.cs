using Crystal.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Functions
{
    public class Function
    {
        public string Name { get; }
        public List<(string, string)> Parameters { get; }
        public List<Token> Body { get; }

        public Function(string name, List<(string, string)> parameters, List<Token> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
}
