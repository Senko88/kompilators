using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace kompilator
{
    public class Lexer
    {
        private InputReader _reader;

        private Dictionary<string, int> _keywords = new Dictionary<string, int>
{
    {"program", 1}, {"var", 2}, {"begin", 3}, {"end", 4},
    {"integer", 5},  // ← Вот он!
    {":=", 6}, {";", 7}, {"(", 8}, {")", 9},{"writeln", 10},{".", 11}
};


        public Lexer(InputReader reader)
        {
            _reader = reader;
        }

        public Token NextToken()
        {
            // Пропускаем пробелы
            while (char.IsWhiteSpace(_reader.Peek()))
                _reader.NextChar();

            char c = _reader.Peek();
            if (c == '\0')
                return new Token(TokenType.EOF, "");

            // Идентификаторы или ключевые слова
            if (char.IsLetter(c))
            {
                string word = "";
                while (char.IsLetterOrDigit(_reader.Peek()))
                    word += _reader.NextChar();

                // Жёсткая проверка регистра
                string lowerWord = word.ToLower();
                if (_keywords.ContainsKey(lowerWord))
                    return new Token(TokenType.KEYWORD, word, _keywords[lowerWord]);
                else
                    return new Token(TokenType.ID, word);
            }

            // Числа
            if (char.IsDigit(c))
            {
                string num = "";
                while (char.IsDigit(_reader.Peek()))
                    num += _reader.NextChar();

                return new Token(TokenType.NUMBER, num);
            }

            // Операторы и разделители
            switch (c)
            {

                case '.':
                    _reader.NextChar();
                    return new Token(TokenType.OPERATOR, ".");  // Было EOF, стало OPERATOR
                case '(':
                    _reader.NextChar();
                    return new Token(TokenType.OPERATOR, "(", _keywords["("]);
                case ')':
                    _reader.NextChar();
                    return new Token(TokenType.OPERATOR, ")", _keywords[")"]);
                case ':':
                    _reader.NextChar();
                    if (_reader.Peek() == '=')
                    {
                        _reader.NextChar();
                        return new Token(TokenType.OPERATOR, ":=", _keywords[":="]);
                    }
                    return new Token(TokenType.COLON, ":");
                case ';':
                    _reader.NextChar();
                    return new Token(TokenType.OPERATOR, ";", _keywords[";"]);
                default:
                    _reader.LogError($"Неизвестный символ: {c}");
                    _reader.NextChar();
                    return new Token(TokenType.UNKNOWN, c.ToString());
            }
        }
    }
}
