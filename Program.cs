using System;
using System.IO;

namespace kompilator
{
    class Program
    {
        static void Main()
        {
            try
            {
                string filePath = "program.pas";

                if (!File.Exists(filePath))
                {
                    ShowError($"Файл не найден: {filePath}");
                    Console.WriteLine("Создайте файл program.pas в папке:");
                    Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
                    return;
                }

                string sourceCode = File.ReadAllText(filePath);
                Console.WriteLine("=== Исходный код ===");
                Console.WriteLine(sourceCode);

                var parser = new Parser(new Lexer(new InputReader(sourceCode)));
                parser.ParseProgram();

                ShowSuccess("Программа корректна!");
                Console.WriteLine("\n=== Ключевые слова компилятора ===");
                PrintKeyWords();

                var parser2 = new Parser(new Lexer(new InputReader(sourceCode)));
                parser2.ParseProgram();
                PrintTokens(sourceCode);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private static void PrintKeyWords()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nСловарь лексера:");
            foreach (var kw in new Lexer(null)._keywords)
            {
                Console.WriteLine($"{kw.Value}");
            }
            Console.ResetColor();
        }
        // Вспомогательные методы
        private static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nОШИБКА: {message}");
            Console.ResetColor();
        }

        private static void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
        }
        
        
        private static void PrintTokens(string code)
        {
            Console.WriteLine("\nТокены:");
            var lexer = new Lexer(new InputReader(code));
            Token token;
            do
            {
                token = lexer.NextToken();
                Console.WriteLine($"{token.Type}: {token.Value}");
            } while (token.Type != TokenType.EOF);
        }
    }
}
