using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Variables
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Variable> vars = new Dictionary<string, Variable>();

        public void AddVariable(string name, string type, object value)
        {
            if (vars.ContainsKey(name))
            {
                throw new Exception($"variable '{name}' is already defined");
            }
            vars[name] = new Variable(name, type, value);
        }

        public Variable GetVariable(string name)
        {
            if (vars.ContainsKey(name))
            {
                return vars[name];
            }
            throw new Exception($"variable '{name}' is not defined");
        }

        public void UpdateVariable(string name, object value)
        {
            if (vars.ContainsKey(name))
            {
                vars[name].Value = value;
            }
            else
            {
                throw new Exception($"variable '{name}' is not defined");
            }
        }

        public void PrintAllVariables()
        {
            foreach (var variable in vars.Values)
            {
                Console.WriteLine(variable);
            }
        }
    }
}
