using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kompilator
{
    public enum TokenType
    {
        EOF,        // Конец файла
        KEYWORD,    // Ключевое слово (program, var)
        ID,         // Идентификатор (x, y)
        NUMBER,     // Число (42, 3.14)
        OPERATOR,   // Оператор (+, -, :=)
        COLON,      // Двоеточие (:)
        SEMICOLON,  // Точка с запятой (;)
        UNKNOWN ,    // Неизвестный символ
        NEWLINE
    }
}
