

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
        public List<string> Errors { get; } = new List<string>();
        private readonly InputReader _reader;
        private readonly SemanticAnalyzer _semanticAnalyzer; // Добавляем поле
        public int _currentLine = 1; // Добавляем отслеживание строки
        public readonly Stack<(char, int)> _bracketStack = new Stack<(char, int)>();
        private void ThrowError(int code, params object[] args)
        {
            string message = $"Строка {_currentLine}: Ошибка {code}: {string.Format(ErrorCodes[code], args)}";
            Errors.Add(message); // Добавляем ошибку в список
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message); // Выводим сразу (опционально)
            Console.ResetColor();
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
            {115, "Неверное количество параметров"},
            {116, "Незакрытая скобка: {0}"},
            {117, "Незакрытая кавычка"},
            {118, "Недопустимый символ: {0}"},
            {119, "Целое число {0} вне допустимого диапазона [-32768, 32767]"},
            {120, "Вещественное число {0} вне допустимого диапазона"}
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


        public Lexer(InputReader reader)
        {
            _reader = reader;
            _semanticAnalyzer = new SemanticAnalyzer(); // Инициализируем анализатор
            _bracketStack = new Stack<(char, int)>(); // Инициализация стека
        }


        public Token NextToken()
        {
            while (char.IsWhiteSpace(_reader.Peek()))
            {
                if (_reader.NextChar() == '\n') _currentLine++;
                
            }

            char c = _reader.Peek();
            if (c == '\0') return new Token(TokenType.EOF, "", 0);

            // Проверка на недопустимые символы
            if (c == '@' || c == '$' || c == '&' || c == '?')
            {
                _reader.NextChar();
                ThrowError(118, c.ToString());
                return new Token(TokenType.UNKNOWN, c.ToString());
            }

            // Обработка скобок и кавычек
            if (c == '(' || c == '[' || c == '{')
            {
                _bracketStack.Push((c, _currentLine));
            }
            else if (c == ')' || c == ']' || c == '}')
            {
                if (_bracketStack.Count == 0)
                {
                    ThrowError(116, $"Лишняя закрывающая скобка: {c}");
                }
                else
                {
                    var (opening, line) = _bracketStack.Pop();
                    if ((opening == '(' && c != ')') ||
                        (opening == '[' && c != ']') ||
                        (opening == '{' && c != '}'))
                    {
                        ThrowError(116, $"Несоответствие скобок: ожидалось {GetClosingBracket(opening)}, но получено {c}");
                    }
                }
            }
            else if (c == '\'')
            {
                // Обработка кавычек
                _reader.NextChar(); // Пропускаем открывающую кавычку
                bool closed = false;
                while (_reader.Peek() != '\0')
                {
                    if (_reader.Peek() == '\'')
                    {
                        _reader.NextChar();
                        closed = true;
                        break;
                    }
                    _reader.NextChar();
                }
                if (!closed)
                {
                    ThrowError(117);
                }
                return NextToken(); // Пропускаем содержимое кавычек
            }

            // Пропускаем пробельные символы и считаем строки
            while (char.IsWhiteSpace(_reader.Peek()))
            {
                if (_reader.NextChar() == '\n')
                {
                    _currentLine++;
                }
            }
            while (char.IsWhiteSpace(_reader.Peek()))
                _reader.NextChar();


            if (c == '\0') return new Token(TokenType.EOF, "", 0);
            if (c == '.' && !char.IsDigit(_reader.PeekNext()))
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




            // Обработка чисел
            if (char.IsDigit(c) || c == '.')
            {
                string numStr = ReadNumber();

                if (numStr.Contains('.') || numStr.Contains('e') || numStr.Contains('E'))
                {
                    if (!double.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double realValue))
                    {
                        ThrowError(102, $"Некорректное вещественное число: {numStr}");
                    }
                    else if (!IsRealInRange(realValue))
                    {
                        ThrowError(120, numStr);
                    }
                    return new Token(TokenType.NUMBER, numStr, TokenCodes["floatc"]);
                }
                else
                {
                    if (!long.TryParse(numStr, out long intValue))
                    {
                        ThrowError(102, $"Слишком большое целое число: {numStr}");
                    }
                    if (!IsIntegerInRange(intValue))
                    {
                        ThrowError(119, numStr); // Используем новый код ошибки
                    }
                    return new Token(TokenType.NUMBER, numStr, TokenCodes["intc"]);
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
        private string ReadNumber()
        {
            string number = "";
            char c = _reader.Peek();

            // Читаем целую часть
            while (char.IsDigit(c))
            {
                number += _reader.NextChar();
                c = _reader.Peek();
            }

            // Проверяем на вещественное число
            if (c == '.')
            {
                number += _reader.NextChar(); // Добавляем точку
                c = _reader.Peek();

                // Читаем дробную часть
                while (char.IsDigit(c))
                {
                    number += _reader.NextChar();
                    c = _reader.Peek();
                }
            }

            // Проверяем на экспоненциальную запись
            if (c == 'e' || c == 'E')
            {
                number += _reader.NextChar(); // Добавляем 'e' или 'E'
                c = _reader.Peek();

                // Проверяем на знак
                if (c == '+' || c == '-')
                {
                    number += _reader.NextChar();
                    c = _reader.Peek();
                }

                // Читаем экспоненту
                while (char.IsDigit(c))
                {
                    number += _reader.NextChar();
                    c = _reader.Peek();
                }
            }

            return number;
        }
        private char GetClosingBracket(char opening)
        {
            return opening switch
            {
                '(' => ')',
                '[' => ']',
                '{' => '}',
                _ => throw new ArgumentException("Неизвестная скобка")
            };
        }
        private bool IsIntegerInRange(long value)
        {
            return value >= -32768 && value <= 32767;
        }

        private bool IsRealInRange(double value)
        {
            return !double.IsInfinity(value);
        }
    }
}
    }
}
