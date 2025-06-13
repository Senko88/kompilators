using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace kompilator
{
    // Lexer.cs
    public class Lexer
    {
        public List<string> Errors { get; } = new List<string>(); // Новое поле для ошибок
        private readonly InputReader _reader;
        private Parser _parser;
        private int _currentLine = 1; // Добавляем отслеживание строки
        private void ThrowError(int code, params object[] args)
        {
            // Формируем сообщение
            string message = $"Строка {_currentLine}: Ошибка {code}: {string.Format(ErrorCodes[code], args)}";

            // Вывод в консоль
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();

            // Завершаем программу с кодом ошибки
            Environment.Exit(code);
        }
        public static readonly Dictionary<int, string> ErrorCodes = new Dictionary<int, string>
        {
            {100, "Отсутствует ;"},
            {101, "Отсутствует . после end"},
            {102, "Неверный тип данных"},
            {103, "Неизвестный символ: {0}"},
            {104, "Ожидалось {0}, но получено {1}"},
            {105, "Неизвестный оператор"},
            {106, "Переменная '{0}' не объявлена"},
            {107, "Отсутствует begin"},
            {108, "Отсутствует end"},
            {109, "Отсутствует :="},
            {110, "Отсутствует '('"},
            {111, "Отсутствует ')'"},
            {112, "Несоответствие типов"},
            {113, "Дублирование идентификатора '{0}'"},
            {114, "Выход за границы массива"},
            {115, "Неверное количество параметров"}
            // ... добавь остальные из твоего списка
        };
        public static readonly Dictionary<string, int> TokenCodes = new()
        {
            // Специальные символы и операторы
            { "*", 21 },
            { "/", 60 },
            { "=", 16 },
            { ",", 20 },
            { ";", 14 },
            { ":", 5 },
            { ".", 61 },
            { "^", 62 },
            { "(", 9 },
            { ")", 4 },
            { "[", 11 },
            { "]", 12 },
            { "{", 63 },
            { "}", 64 },
            { "<", 65 },
            { ">", 66 },
            { "<=", 67 },
            { ">=", 68 },
            { "<>", 69 },
            { "+", 70 },
            { "-", 71 },
            { "(*", 72 },
            { "*)", 73 },
            { ":=", 51 },
            { "..", 74 },

            // Ключевые слова (в нижнем регистре)
            { "case", 31 },
            { "else", 32 },
            { "file", 57 },
            { "goto", 33 },
            { "then", 52 },
            { "type", 34 },
            { "until", 53 },
            { "do", 54 },
            { "with", 37 },
            { "if", 56 },
            { "in", 100 },
            { "of", 101 },
            { "or", 102 },
            { "to", 103 },
            { "end", 104 },
            { "var", 105 },
            { "div", 106 },
            { "and", 107 },
            { "not", 108 },
            { "for", 109 },
            { "mod", 110 },
            { "nil", 111 },
            { "set", 112 },
            { "begin", 113 },
            { "while", 114 },
            { "array", 115 },
            { "const", 116 },
            { "label", 117 },
            { "downto", 118 },
            { "packed", 119 },
            { "record", 120 },
            { "repeat", 121 },
            { "program", 122 },
            { "function", 123 },
            { "procedure", 124 },

            // Константы (псевдотокены)
            { "ident", 2 },
            { "floatc", 82 },
            { "intc", 15 }
        };
        public static string GetAllTokenCodes()
        {
            return string.Join(" ", TokenCodes.Values.OrderBy(code => code));
        }
        public int GetTokenCode(string lexeme)
        {
            return TokenCodes.TryGetValue(lexeme.ToLower(), out int code)
                ? code
                : TokenCodes["ident"]; // По умолчанию идентификатор (код 2)
        }


        public Lexer(InputReader reader, Parser parser = null)
        {
            _reader = reader;
            _parser = parser;
        }

        // Метод для позднего связывания
        public void SetParser(Parser parser)
        {
            _parser = parser;
        }


        public Token NextToken()
        {
            while (char.IsWhiteSpace(_reader.Peek()))
                _reader.NextChar();

            char c = _reader.Peek();
            if (c == '\0') return new Token(TokenType.EOF, "", 0);
            if (c == '.' && !char.IsDigit(_reader.NextChar()))
            {
                _reader.NextChar(); // Пропускаем точку
                return new Token(TokenType.OPERATOR, ".", TokenCodes["."]);
            }
            // 1. Если буква — IDENT или KEYWORD
            if (char.IsLetter(c))
            {
                string word = "";
                while (char.IsLetterOrDigit(_reader.Peek()))
                    word += _reader.NextChar();

                int code = GetTokenCode(word);
                return new Token(code == 2 ? TokenType.IDENT : TokenType.KEYWORD, word, code);
            }

            // 2. Если цифра или точка (NUMBER, в том числе real)
            if (char.IsDigit(c) || c == '.')
            {
                string numStr = "";
                bool hasDot = false;
                bool hasExponent = false;

                // Читаем цифры и точку
                while (char.IsDigit(_reader.Peek()) || _reader.Peek() == '.')
                {
                    char nextChar = _reader.Peek();
                    if (nextChar == '.')
                    {
                        if (hasDot) break; // Уже была точка — выходим
                        hasDot = true;
                    }
                    numStr += _reader.NextChar();
                }

                // Проверяем экспоненту (e/E)
                if (_reader.Peek() == 'e' || _reader.Peek() == 'E')
                {
                    numStr += _reader.NextChar(); // Добавляем 'e'
                    hasExponent = true;

                    // Обрабатываем знак (+/-)
                    if (_reader.Peek() == '+' || _reader.Peek() == '-')
                    {
                        numStr += _reader.NextChar();
                    }

                    // Читаем цифры экспоненты
                    while (char.IsDigit(_reader.Peek()))
                    {
                        numStr += _reader.NextChar();
                    }
                }

                // Проверяем, что это число (а не просто точка или 'e')
                if (numStr.Length > 0 && (char.IsDigit(numStr[0]) || numStr.StartsWith(".") && numStr.Length > 1))
                {
                    if (hasDot || hasExponent)
                    {
                        if (!double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double realValue))
                        {
                            ThrowError(102, $"Некорректное вещественное число: {numStr}");
                        }

                        // Проверяем диапазон для вещественных чисел
                        _parser.CheckRealRange(realValue);

                        return new Token(TokenType.NUMBER, numStr, TokenCodes["floatc"]);
                    }
                    else
                    {
                        if (!int.TryParse(numStr, out int intValue))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"⚠ Предупреждение: '{numStr}' — слишком большое целое число");
                            Console.ResetColor();
                        }
                        else
                        {
                            _parser.CheckIntegerRange(intValue);
                        }
                        return new Token(TokenType.NUMBER, numStr, TokenCodes["intc"]);
                    }
                }
            }

            // 3. Если символ — OPERATOR/PUNCT
            string op = _reader.NextChar().ToString();
            if (TokenCodes.ContainsKey(op + _reader.Peek()))
            {
                op += _reader.NextChar();
            }
            return new Token(TokenType.OPERATOR, op, GetTokenCode(op));
        }
    }
}
