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
        private readonly InputReader _reader;
        private readonly SemanticAnalyzer _semanticAnalyzer; // Добавляем поле

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
        }
        private Token ParseNumber()
        {
            string numStr = "";
            bool hasDot = false;
            bool hasExponent = false;

            // Собираем все цифры, точки и знаки экспоненты
            while (char.IsDigit(_reader.Peek()) ||
                   _reader.Peek() == '.' ||
                   char.ToLower(_reader.Peek()) == 'e')
            {
                char current = _reader.Peek();

                if (current == '.')
                {
                    if (hasDot) throw new Exception("Две точки в числе");
                    hasDot = true;
                }
                else if (char.ToLower(current) == 'e')
                {
                    if (hasExponent) throw new Exception("Две экспоненты в числе");
                    hasExponent = true;
                    numStr += _reader.NextChar();

                    // Обрабатываем знак после экспоненты
                    if (_reader.Peek() == '+' || _reader.Peek() == '-')
                    {
                        numStr += _reader.NextChar();
                    }
                    continue;
                }

                numStr += _reader.NextChar();
            }

            try
            {
                if (hasDot || hasExponent)
                {
                    double value = double.Parse(numStr, CultureInfo.InvariantCulture);
                    _semanticAnalyzer.CheckRealRange(value);
                    return new Token(TokenType.NUMBER, numStr, TokenCodes["floatc"]);
                }
                else
                {
                    int value = int.Parse(numStr);
                    _semanticAnalyzer.CheckIntegerRange(value);
                    return new Token(TokenType.NUMBER, numStr, TokenCodes["intc"]);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ОШИБКА в строке {_reader.CurrentLine}: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
                return new Token(TokenType.UNKNOWN, numStr, -1);
            }
        }
        public Token NextToken()
        {

            while (char.IsWhiteSpace(_reader.Peek()))
                _reader.NextChar();

            char c = _reader.Peek();
            if (c == '\0') return new Token(TokenType.EOF, "", 0);

            // 1. Если буква — это IDENT или KEYWORD
            if (char.IsLetter(c))
            {
                string word = "";
                while (char.IsLetterOrDigit(_reader.Peek()))
                    word += _reader.NextChar();

                int code = GetTokenCode(word);
                return new Token(code == 2 ? TokenType.IDENT : TokenType.KEYWORD, word, code);
            }

            // 2. Если цифра — это NUMBER
            if (char.IsDigit(c))
            {
                string numStr = "";
                while (char.IsDigit(_reader.Peek()))
                    numStr += _reader.NextChar();

                try
                {
                    int value = int.Parse(numStr);
                    _semanticAnalyzer.CheckIntegerRange(value);
                    return new Token(TokenType.NUMBER, numStr, TokenCodes["intc"]);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ОШИБКА в строке {_reader.CurrentLine}: {ex.Message}");
                    Console.ResetColor();
                    // Можно либо продолжить работу, либо завершить программу
                    Environment.Exit(1);
                }
            }

            // 3. Если символ — OPERATOR/PUNCT
            string op = _reader.NextChar().ToString();
            // Проверка многосимвольных операторов (:=, <= и т.д.)
            if (TokenCodes.ContainsKey(op + _reader.Peek()))
            {
                op += _reader.NextChar();
            }
            return new Token(TokenType.OPERATOR, op, GetTokenCode(op));
        

   
        }
    }
    }




