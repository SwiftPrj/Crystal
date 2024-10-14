using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal
{
    public enum Type
    {
        GENERIC,
        INT,
        DOUBLE,
        FLOAT,
        STRING,
        ARRAYINT,
        ARRAYSTRING,
        ARRAYGENERIC,
        VOID,
        None
    }

    public class Variable
    {
        public string Name;
        public Type Type;
        public object Value;

        public Variable(string name, Type type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public Type GetType()
        {
            return Type;
        }
        public void ChangeValue(object value)
        {
            Value = value;
        }
    }

    public class Function
    {
        public string Name;
        public Type Type;
        public List<Variable> Variables;
        public List<Action> Actions;
        public object Return; // return value

        public Function(string name, Type type, List<Action> actions)
        {
            Name = name;
            Type = type;
            Variables = new();
            Actions = actions;
        }

        public object GetReturn() { return Return; }

        public void Run()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Action action = Actions[i];
                action.Invoke();
            }
        }
    }

    public class Memory
    {
        public readonly string[] keywords = { "sysout", "return", "var", "func", "int", "double", "array", "string" };
        public readonly List<Function> functions;

        public Memory()
        {
            functions = new();
        }

        public bool FuncExists(string funcName)
        {
            foreach (Function func in functions)
            {
                if (func.Name == funcName)
                {
                    return true;
                }
            }

            return false;
        }

        public void CreateFunction(string name, Type type, List<Action> actions)
        {
            if (keywords.Contains(name))
                throw new Exception("A function cannot have the same name as a keyword");

            functions.Add(new Function(name, type, actions));
        }

        public void CreateFunction(string name, Type type)
        {
            if (keywords.Contains(name))
                throw new Exception("A function cannot have the same name as a keyword");

            functions.Add(new Function(name, type, new()));
        }

        public Function GetFunction(string funcName)
        {
            foreach (Function func in functions)
            {
                if (func.Name == funcName)
                {
                    return func;
                }
            }

            return null;
        }

        public void CreateVariable(string name, string context, Type type, object value)
        {
            if (keywords.Contains(name))
                throw new Exception("A function cannot have the same name as a keyword");

            Variable var = new Variable(name, type, value);
            foreach (Function func in functions)
            {
                if (func.Name == context)
                {
                    func.Variables.Add(var);
                }
            }
        }

        public Variable GetVariable(string varName, string context)
        {
            foreach (Function func in functions)
            {
                if (func.Name == context)
                {
                    foreach (Variable v in func.Variables)
                    {
                        if (v.Name == varName)
                        {
                            return v;
                        }
                    }
                }
            }

            return null;
        }

        public bool VarExists(string varName, string context)
        {
            foreach (Function func in functions)
            {
                if (func.Name == context)
                {
                    foreach (Variable v in func.Variables)
                    {  
                        if (v.Name == varName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsTypeOf(string varName, string context, Type typeCompare)
        {
            Type type = GetVariable(varName, context).Type;

            return GenericTypeCompare(type, typeCompare);
        }

        private bool GenericTypeCompare(Type type1, Type type2)
        {
            if (type1.Equals(type2))
            {
                return true;
            }

            if ((type1.Equals(Type.ARRAYINT) || type1.Equals(Type.ARRAYSTRING))
                && type2.Equals(Type.ARRAYGENERIC))
            {
                return true;
            }

            if ((type2.Equals(Type.ARRAYINT) || type2.Equals(Type.ARRAYSTRING))
                && type1.Equals(Type.ARRAYGENERIC))
            {
                return true;
            }

            return false;
        }
    }
}