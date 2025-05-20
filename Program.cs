using System;

namespace kompilator
{
    class Program
    {
        static void Main()
        {
            try
            {
                var reader = new InputReader(@"C:\My_pask\programm.pas");
                var lexer = new Lexer(reader);

                // Вывод всех токенов
                Console.WriteLine("=== Список токенов ===");
                Token token;
                do
                {
                    token = lexer.NextToken();
                    Console.WriteLine($"{token.Type}: '{token.Value}'");
                } while (token.Type != TokenType.EOF);

                // Заново инициализируем лексер и парсер
                reader = new InputReader(@"C:\My_pask\programm.pas");
                lexer = new Lexer(reader);
                var parser = new Parser(lexer);
                parser.ParseProgram();
                Console.WriteLine("✅ Успешная компиляция!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
