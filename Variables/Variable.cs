using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Variables
{
    public class Variable
    {
        public string Name { get; }
        public string Type { get; }
        public object Value { get; set; }

        public Variable(string name, string type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"Variable= {Type} {Name} = {Value}";
        }
    }
}
