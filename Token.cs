using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kompilator
{
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Code { get; }  // Для ключевых слов

        public Token(TokenType type, string value, int code = -1)
        {
            Type = type;
            Value = value;
            Code = code;
        }
    }
}
