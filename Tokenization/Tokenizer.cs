using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystal.Tokenization
{
    public class Tokenizer
    {
        private readonly string source;
        private int currentIndex;

        private static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "func", "var", "int", "double", "string", "return"
        };

        private static readonly HashSet<string> Operators = new HashSet<string>
        {
            ":", "->", "*", "/", "=", "(", ")", "{", "}"
        };

        public Tokenizer(string source)
        {
            this.source = source;
            currentIndex = 0;
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new();

            while (currentIndex < source.Length)
            {
                char currentChar = source[currentIndex];

                if (char.IsWhiteSpace(currentChar))
                {
                    tokens.Add(new Token(TokenType.Whitespace, ReadWhile(char.IsWhiteSpace)));
                }
                else if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    string identifier = ReadWhile(c => char.IsLetterOrDigit(c) || c == '_');
                    if (Keywords.Contains(identifier))
                    {
                        tokens.Add(new Token(TokenType.Keyword, identifier));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Identifier, identifier));
                    }
                }
                else if (char.IsDigit(currentChar))
                {
                    tokens.Add(new Token(TokenType.Number, ReadWhile(char.IsDigit)));
                }
                else if (currentChar == '"')
                {
                    tokens.Add(new Token(TokenType.String, ReadString()));
                } else
                {
                    string op = TryReadOperator();
                    if (!string.IsNullOrEmpty(op))
                    {
                        tokens.Add(new Token(TokenType.Operator, op));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Unknown, ReadNext().ToString()));
                    }
                }
            }

            return tokens;
        }

        private string ReadWhile(Func<char, bool> condition)
        {
            var result = new StringBuilder();

            while (currentIndex < source.Length && condition(source[currentIndex]))
            {
                result.Append(source[currentIndex]);
                currentIndex++;
            }

            return result.ToString();
        }

        private char ReadNext()
        {
            return source[currentIndex++];
        }

        private string ReadString()
        {
            var result = new StringBuilder();
            currentIndex++;

            while (currentIndex < source.Length)
            {
                char currentChar = source[currentIndex];

                // escaped characters
                if (currentChar == '\\')
                {
                    currentIndex++;
                    if (currentIndex < source.Length)
                    {
                        char escapedChar = source[currentIndex];

                        result.Append(escapedChar switch
                        {
                            'n' => '\n',
                            't' => '\t',
                            '"' => '"',
                            '\\' => '\\',
                            _ => escapedChar // handle unrecognized escapes as literal characters
                        });
                    }
                }
                else if (currentChar == '"')
                {
                    currentIndex++;
                    break;
                }
                else
                {
                    result.Append(currentChar);
                }

                currentIndex++;
            }

            return result.ToString();
        }

        private string TryReadOperator()
        {
            foreach (var op in Operators)
            {
                if (currentIndex + op.Length <= source.Length &&
                    source.Substring(currentIndex, op.Length) == op)
                {
                    currentIndex += op.Length;
                    return op;
                }
            }

            return string.Empty;
        }
    }
}
