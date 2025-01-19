using Crystal.Tokenization;
using System;
using System.IO;

namespace Crystal
{
    public class Start
    {
        static void Main(string[] args)
        {
            string path = "Tests/test_vars.cy";

            if (!File.Exists(path))
            {
                Console.WriteLine("test not found");
                return;
            }

            try {
                string code = File.ReadAllText(path);

                Tokenizer tokenizer = new Tokenizer(code);
                List<Token> tokens = tokenizer.Tokenize();

                /*foreach (var token in tokens)
                {
                    if (token.Type != TokenType.Whitespace)
                    {
                        Console.WriteLine(token);
                    }
                }*/

                Interpreter interpreter = new Interpreter();
                interpreter.Interpret(tokens);
            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}