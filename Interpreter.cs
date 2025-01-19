using Crystal.Tokenization;
using Crystal.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crystal
{
    public class Interpreter
    {
        private readonly SymbolTable symbolTable = new SymbolTable();

        public void Interpret(List<Token> tokens)
        {
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

                        symbolTable.AddVariable(varName, varType, tokens[i].Type switch
                        {
                            TokenType.Number =>
                                varType == "int" ?
                                int.Parse(value) : throw error,
                            TokenType.String =>
                                varType == "string" ?
                                value : throw error,
                            TokenType.Identifier =>
                                symbolTable.GetVariable(value).Type == varType ?
                                symbolTable.GetVariable(value).Value : throw error,
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
                        symbolTable.UpdateVariable(varName, tokens[i].Type switch
                        {
                            TokenType.Number =>
                                symbolTable.GetVariable(varName).Type == "int" ?
                                int.Parse(value) : throw error,
                            TokenType.String =>
                                symbolTable.GetVariable(varName).Type == "string" ?
                                value : throw error,
                            TokenType.Identifier =>
                                symbolTable.GetVariable(value).Type == symbolTable.GetVariable(varName).Type ?
                                symbolTable.GetVariable(value).Value : throw error,
                            _ => error
                        });

                        i = SkipWhitespace(tokens, i); // TODO concatenation + math parser
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

            symbolTable.PrintAllVariables();
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
