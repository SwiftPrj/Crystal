﻿using Crystal.Functions;
using Crystal.Tokenization;
using Crystal.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal
{
    public class Interpreter
    {
        private readonly FunctionTable functionTable = new FunctionTable();
        private readonly Stack<SymbolTable> scopeStack = new Stack<SymbolTable>();

        public void Interpret(List<Token> tokens)
        {
            for (int i = 0; i < tokens.Count;)
            {
                var token = tokens[i];

                if (token.Type == TokenType.Keyword && token.Value == "func")
                {
                    i = SkipWhitespace(tokens, i); // move past 'func'
                    string functionName = tokens[i].Value;
                    i = SkipWhitespace(tokens, i); // move past function name
                    i = SkipWhitespace(tokens, i); // move past '('

                    var parameters = new List<(string, string)>();
                    while (tokens[i].Type != TokenType.RightPr)
                    {
                        string paramName = tokens[i].Value;
                        i = SkipWhitespace(tokens, i); // move past parameter name
                        i = SkipWhitespace(tokens, i); // move past ':'
                        string paramType = tokens[i].Value;
                        parameters.Add((paramName, paramType));
                        i = SkipWhitespace(tokens, i); // move past parameter type

                        if (tokens[i].Value == ",")
                        {
                            i = SkipWhitespace(tokens, i); // move past ','
                        }
                    }

                    i = SkipWhitespace(tokens, i); // move past ')'
                    i = SkipWhitespace(tokens, i); // move past '{'

                    var functionBody = new List<Token>();
                    while (tokens[i].Type != TokenType.RightBr)
                    {
                        functionBody.Add(tokens[i]);
                        i = SkipWhitespace(tokens, i);
                    }

                    functionTable.AddFunction(functionName, parameters, functionBody);
                    i = SkipWhitespace(tokens, i); // move past '}'
                }
                else
                {
                    i = SkipWhitespace(tokens, i);
                }
            }

            // execute main function if found
            if (functionTable.GetFunction("main") != null)
            {
                ExecuteFunction("main", new List<object>());
            }
            else
            {
                throw new Exception("No main function found!");
            }
        }

        private void ExecuteFunction(string functionName, List<object> arguments)
        {
            var function = functionTable.GetFunction(functionName);
            Console.WriteLine($"Executing function: {functionName}");

            // push a new scope for the function
            var currentScope = new SymbolTable();
            scopeStack.Push(currentScope);

            // add function arguments to the scope
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var (paramName, paramType) = function.Parameters[i];
                var argumentValue = arguments[i] is string argStr && scopeStack.Any() && scopeStack.Peek().HasVariable(argStr)
                    ? scopeStack.Peek().GetVariable(argStr).Value
                    : arguments[i];
                currentScope.AddVariable(paramName, paramType, argumentValue);
            }

            List<Token> tokens = function.Body;

            for (int i = 0; i < tokens.Count;)
            {
                var token = tokens[i];

                //Console.WriteLine(token);
                if (token.Type == TokenType.Keyword && token.Value == "var")
                {
                    // variable name
                    i = SkipWhitespace(tokens, i);

                    string varName = tokens[i].Value;

                    // move to next token, should be ':'
                    i = SkipWhitespace(tokens, i);
                    if (tokens[i].Value == ":")
                    {
                        i = SkipWhitespace(tokens, i); // next token is var type
                    }
                    else
                    {
                        throw new Exception("expected ':' after variable declaration");
                    }

                    // variable type
                    string varType = tokens[i].Value;

                    // move to next token, should be '='
                    i = SkipWhitespace(tokens, i);
                    if (tokens[i].Value == "=")
                    {
                        i = SkipWhitespace(tokens, i); // next token is value
                        string value = tokens[i].Value;
                        var error = new Exception("invalid int value in declaration");

                        scopeStack.Peek().AddVariable(varName, varType, tokens[i].Type switch
                        {
                            TokenType.Number =>
                                varType == "int" ?
                                int.Parse(value) : throw error,
                            TokenType.String =>
                                varType == "string" ?
                                value : throw error,
                            TokenType.Identifier =>
                                scopeStack.Peek().GetVariable(value).Type == varType ?
                                scopeStack.Peek().GetVariable(value).Value : throw error,
                            _ => error
                        });

                        // semicolon
                        i = SkipWhitespace(tokens, i); // TODO concatenation + math parser
                    }
                    else
                    {
                        throw new Exception("expected '=' after variable declaration");
                    }

                    if (tokens[i].Value == ";")
                    {
                        i = SkipWhitespace(tokens, i); // move after semicolon
                    }
                    else
                    {
                        throw new Exception("expected ';' at the end of the statement");
                    }
                }
                else if (token.Type == TokenType.Identifier)
                {
                    string varName = token.Value;

                    // move to next token, should be '='
                    i = SkipWhitespace(tokens, i);

                    if (tokens[i].Value == "=")
                    {
                        i = SkipWhitespace(tokens, i); // next token is the value

                        var value = ParseValue(tokens, ref i, scopeStack.Peek().GetVariable(varName).Type);

                        scopeStack.Peek().UpdateVariable(varName, value);

                        if (tokens[i].Value == ";")
                        {
                            i = SkipWhitespace(tokens, i);
                        }
                        else
                        {
                            throw new Exception("expected ';' at the end of the statement");
                        }
                    }
                    else if(tokens[i].Value == "(")
                    {
                        var functionArgs = new List<object>();
                        i = SkipWhitespace(tokens, i); // move past '('
                        while (tokens[i].Type != TokenType.RightPr)
                        {
                            string? argValue = tokens[i].Type == TokenType.Identifier && scopeStack.Any() && scopeStack.Peek().HasVariable(tokens[i].Value)
                               ? scopeStack.Peek().GetVariable(tokens[i].Value).Value.ToString()
                               : tokens[i].Value;
                            functionArgs.Add(argValue); // handle different types as needed
                            i = SkipWhitespace(tokens, i);
                            if (tokens[i].Value == ",")
                            {
                                i = SkipWhitespace(tokens, i); // move past ','
                            }
                        }
                        i = SkipWhitespace(tokens, i); // move past ')'

                        if (functionTable.GetFunction(varName) != null)
                        {
                            ExecuteFunction(varName, functionArgs);
                        }
                    }
                    else
                    {
                        throw new Exception("expected '=' in assignment");
                    }
                }
                else
                {
                    i++;
                }
            }

            Console.WriteLine("Start scope: " + functionName);
            scopeStack.Peek().PrintAllVariables();
            Console.WriteLine("Stop scope");

            scopeStack.Pop();
        }

        private object ParseValue(List<Token> tokens, ref int i, string expectedType)
        {
            var token = tokens[i];
            object value = null;

            if (token.Type == TokenType.String)
            {
                value = token.Value;
                i = SkipWhitespace(tokens, i);
            }
            else if (token.Type == TokenType.Number)
            {
                if (expectedType == "int")
                {
                    value = int.Parse(token.Value);
                }
                else
                {
                    throw new Exception("invalid int value in declaration");
                }
                i = SkipWhitespace(tokens, i);
            }
            else if (token.Type == TokenType.Identifier)
            {
                var varValue = scopeStack.Peek().GetVariable(token.Value).Value;
                if (expectedType == "string" && varValue is string)
                {
                    value = varValue;
                }
                else if (expectedType == "int" && varValue is int)
                {
                    value = varValue;
                }
                else
                {
                    throw new Exception("type mismatch in variable assignment");
                }
                i = SkipWhitespace(tokens, i);
            }

            if (tokens[i].Value == "+")
            {
                i = SkipWhitespace(tokens, i);
                var rightValue = ParseValue(tokens, ref i, expectedType);

                if (expectedType == "string")
                {
                    value = (string)value + (string)rightValue;
                }
                else
                {
                    throw new Exception("concatenation operator is only supported for strings");
                }
            }

            return value;
        }


        private int SkipWhitespace(List<Token> tokens, int originalIndex)
        {
            int i = originalIndex + 1;
            if (i >= tokens.Count) {
                return tokens.Count;
            }

            Token token = tokens[i];
            while (token.Type == TokenType.Whitespace) {
                if (i >= tokens.Count) {
                    return tokens.Count;
                }

                i++;
                token = tokens[i];
            }

            return i;
        }
    }
}
