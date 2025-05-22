using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace kompilator
{
    // Lexer.cs
    public class Lexer
    {
        private readonly InputReader _reader;
        private readonly Dictionary<string, int> _keywords = new()
        {
            { "program", 1 },
            { "var", 2 },
            { "begin", 3 },
            { "end", 4 },
            { "integer", 5 },
            { "writeln", 6 }
        };

        public Lexer(InputReader reader)
        {
            _reader = reader;
        }

        public Token NextToken()
        {
            // Пропускаем пробелы и управляющие символы
            while (char.IsWhiteSpace(_reader.Peek()))
            {
                _reader.NextChar();
            }

            char c = _reader.Peek();
            if (c == '\0')
            {
                return new Token(TokenType.EOF, "");
            }
            if (c == '\n')
            {
                _reader.NextChar();
                return new Token(TokenType.NEWLINE, "\n");
            }
            // Идентификаторы или ключевые слова
            if (char.IsLetter(c))
            {
                string word = "";
                while (char.IsLetterOrDigit(_reader.Peek()))
                {
                    word += _reader.NextChar();
                }

                if (_keywords.TryGetValue(word.ToLower(), out int code))
                {
                    return new Token(TokenType.KEYWORD, word, code);
                }
                return new Token(TokenType.ID, word);
            }

            // Числа
            if (char.IsDigit(c))
            {
                string num = "";
                while (char.IsDigit(_reader.Peek()))
                {
                    num += _reader.NextChar();
                }
                return new Token(TokenType.NUMBER, num);
            }

            // Операторы и разделители
            switch (c)
            {
                case ';':
                    _reader.NextChar();
                    return new Token(TokenType.OPERATOR, ";");
                case '.':
                    _reader.NextChar();
                    return new Token(TokenType.OPERATOR, ".");
                case ':':
                    _reader.NextChar();
                    if (_reader.Peek() == '=')
                    {
                        _reader.NextChar();
                        return new Token(TokenType.OPERATOR, ":=");
                    }
                    return new Token(TokenType.COLON, ":");
                default:
                    _reader.NextChar();
                    _reader.LogError($"Неизвестный символ: {c}");
                    return new Token(TokenType.UNKNOWN, c.ToString());
            }
        }
    }
}

