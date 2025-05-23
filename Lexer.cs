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
        public readonly Dictionary<string, int> _keywords = new()
        {
            star 21, // *
            slash = 60, // /
            equal = 16, // =
            comma = 20, // ,
            semicolon = 14, // ;
            colon = 5, // :
            point = 61, // .
            arrow = 62, // ^
            leftpar = 9, // (
            rightpar = 4, // )
            lbracket = 11, // [
            rbracket = 12, // ]
            flpar = 63, // {
            frpar = 64, // }
            later = 65, // <
            greater = 66, // >
            laterequal = 67, //  <=
            greaterequal = 68, //  >=
            latergreater = 69, //  <>
            plus = 70, // +
            minus = 71, // –
            lcomment = 72, //  (*
            rcomment = 73, //  *)
            assign = 51, //  :=
            twopoints = 74, //  ..
            ident = 2, // идентификатор
            floatc = 82, // вещественная константа
            intc = 15, // целая константа
            casesy = 31,
            elsesy = 32,
            filesy = 57,
            gotosy = 33,
            thensy = 52,
            typesy = 34,
            untilsy = 53,
            dosy = 54,
            withsy = 37,
            ifsy = 56,
            insy = 100,
            ofsy = 101,
            orsy = 102,
            tosy = 103,
            endsy = 104,
            varsy = 105,
            divsy = 106,
            andsy = 107,
            notsy = 108,
            forsy = 109,
            modsy = 110,
            nilsy = 111,
            setsy = 112,
            beginsy = 113,
            whilesy = 114,
            arraysy = 115,
            constsy = 116,
            labelsy = 117,
            downtosy = 118,
            packedsy = 119,
            recordsy = 120,
            repeatsy = 121,
            programsy = 122,
            functionsy = 123,
            procedurensy = 124;
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


