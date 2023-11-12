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

    public class Memory
    {
        public readonly string[] keywords = { "sysout", "return", "var", "func", "int", "double", "array", "string"};
        private readonly Dictionary<string, Type> variables;
        private readonly Dictionary<string, Object> values;
        public readonly Dictionary<string, Type> functions;
        public readonly Dictionary<string, string> functionCode; 

        public Memory() {
            variables = new();
            values = new();
            functions = new();
            functionCode = new();
        }

        public Type GetVarType(string varName)
        {
            return variables.GetValueOrDefault(varName, Type.None);
        }

        public Type GetFuncType(string funcName)
        {
            return functions.GetValueOrDefault(funcName, Type.None);
        }

        public bool IsTypeOf(string varName, Type typeCompare)
        {
            Type type = variables.GetValueOrDefault(varName, Type.None);

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

        public object GetValue(string varName)
        {
            return values.GetValueOrDefault(varName, null);
        }

        public void CreateVar<T>(string varName, Type type, T value)
        {
            variables.Add(varName, type);
            values.Add(varName, value);
        }

        public void CreateVar<T>(string varName, Type type, T[] value)
        {
            variables.Add(varName, type);
            values.Add(varName, value);
        }

        public void DeleteVar(string varName) {
            variables.Remove(varName);
            values.Remove(varName);
        }

        public void ChangeValue(string varName, object newValue)
        {
            values[varName] = newValue;
        }
    }
}
