using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization; 
using kompilator; 
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

                var lexer = new Lexer(new InputReader(sourceCode));
                var parser = new Parser(lexer); // Передаём в парсер
                parser.ParseProgram();

                // Собираем ошибки из всех источников
                var allErrors = new List<string>();
                allErrors.AddRange(lexer.Errors);
                allErrors.AddRange(parser.Errors);

                // Вывод ошибок
                if (allErrors.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n=== НАЙДЕНЫ ОШИБКИ ===");
                        Console.WriteLine($"\nВсего ошибок: {allErrors.Count}");
                    Console.ResetColor();
                
                ShowError("В программе ошибки!!!");
                    Console.WriteLine("\n=== Ключевые слова компилятора ===");
                    PrintKeyWords();


                    PrintTokens(sourceCode);
                }
                else
                {
                    ShowSuccess("Программа корректна!");
                    Console.WriteLine("\n=== Ключевые слова компилятора ===");
                    PrintKeyWords();

                    PrintTokens(sourceCode);
                }

                
 
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
            Console.WriteLine(Lexer.GetAllTokenCodes());
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
                Console.WriteLine($"[Строка {lexer._currentLine}] {token.Type}: {token.Value}");
            } while (token.Type != TokenType.EOF);
        }
    }
}
