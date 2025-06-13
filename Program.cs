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

                // Инициализация
                var reader = new InputReader("program.pas"); // Ваш вариант InputReader
                var lexer = new Lexer(reader, null); // Lexer с передачей парсера
                var parser = new Parser(lexer); // Ваш вариант Parser
                lexer.SetParser(parser); // Ваш метод для обратной связи

                // Парсинг
                parser.ParseProgram();

                // Сбор ВСЕХ ошибок (лексические + синтаксические/семантические)
                var allErrors = parser.AllErrors.Distinct().ToList(); // Удаляем повторы

                // Вывод
                if (allErrors.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n=== ВСЕ ОШИБКИ ===");
                    foreach (var error in allErrors) Console.WriteLine(error);
                    Console.WriteLine($"\nВсего: {allErrors.Count} ошибок");
                    Console.ResetColor();
                
                ShowError("В программе ошибки!!!");
                    Console.WriteLine("\n=== Ключевые слова компилятора ===");
                    PrintKeyWords();

                    var parser2 = new Parser(lexer);
                    parser2.ParseProgram();
                    PrintTokens(sourceCode,parser2);
                }
                else
                {
                    ShowSuccess("Программа корректна!");
                    Console.WriteLine("\n=== Ключевые слова компилятора ===");
                    PrintKeyWords();

                    var parser3 = new Parser(lexer);
                    parser3.ParseProgram();
                    PrintTokens(sourceCode,parser3);
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


        private static void PrintTokens(string code, Parser parser)
        {
            Console.WriteLine("\nТокены:");
            var lexer = new Lexer(new InputReader(code),parser);
            Token token;
            do
            {
                token = lexer.NextToken();
                Console.WriteLine($"{token.Type}: {token.Value}");
            } while (token.Type != TokenType.EOF);
        }
    }
}
