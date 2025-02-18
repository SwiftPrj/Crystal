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
        public List<Token> Body { get; }

        public Function(string name, List<Token> body)
        {
            Name = name;
            Body = body;
        }
    }
}
