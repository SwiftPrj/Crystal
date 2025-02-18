using Crystal.Functions;
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
                    i = SkipWhitespace(tokens, i); // Move past 'func'
                    string functionName = tokens[i].Value;
                    i = SkipWhitespace(tokens, i); // Move past function name
                    i = SkipWhitespace(tokens, i); // Move past '('
                    i = SkipWhitespace(tokens, i); // Move past ')'
                    i = SkipWhitespace(tokens, i); // Move past '{'

                    var functionBody = new List<Token>();
                    while (tokens[i].Type != TokenType.RightBr)
                    {
                        functionBody.Add(tokens[i]);
                        i = SkipWhitespace(tokens, i);
                    }

                    functionTable.AddFunction(functionName, functionBody);
                    i = SkipWhitespace(tokens, i); // Move past '}'
                }
                else
                {
                    i = SkipWhitespace(tokens, i);
                }
            }

            // Execute main function if found
            if (functionTable.GetFunction("main") != null)
            {
                ExecuteFunction("main");
            }
            else
            {
                throw new Exception("No main function found!");
            }
        }

        private void ExecuteFunction(string functionName)
        {
            var function = functionTable.GetFunction(functionName);
            Console.WriteLine($"Executing function: {functionName}");

            // Push a new scope for the function
            scopeStack.Push(new SymbolTable());

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

                        string value = tokens[i].Value;
                        var error = new Exception("invalid int value in assignment");
                        scopeStack.Peek().UpdateVariable(varName, tokens[i].Type switch
                        {
                            TokenType.Number =>
                                scopeStack.Peek().GetVariable(varName).Type == "int" ?
                                int.Parse(value) : throw error,
                            TokenType.String =>
                                scopeStack.Peek().GetVariable(varName).Type == "string" ?
                                value : throw error,
                            TokenType.Identifier =>
                                scopeStack.Peek().GetVariable(value).Type == scopeStack.Peek().GetVariable(varName).Type ?
                                scopeStack.Peek().GetVariable(value).Value : throw error,
                            _ => error
                        });

                        i = SkipWhitespace(tokens, i); // TODO concatenation + math parser
                    }
                    else if(tokens[i].Value == "(")
                    {
                        i = SkipWhitespace(tokens, i); // next token is )
                        if (functionTable.GetFunction(varName) != null)
                        {
                            ExecuteFunction(varName);
                        }
                        i = SkipWhitespace(tokens, i);
                    }
                    else
                    {
                        throw new Exception("expected '=' in assignment");
                    }

                    if (tokens[i].Value == ";")
                    {
                        i = SkipWhitespace(tokens, i);
                    }
                    else
                    {
                        throw new Exception("expected ';' at the end of the statement");
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
