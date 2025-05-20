using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace kompilator
{
    public class InputReader
    {
        private StreamReader _file;
        private int _line = 1, _column = 1;
        private char _currentChar;

        public InputReader(string filePath)
        {
            _file = new StreamReader(filePath);
            _currentChar = (char)_file.Read(); // Первый символ
        }

        // 🎀 nextch — читает следующий символ (двигает "каретку")
        public char NextChar()
        {
            char c = _currentChar;
            if (_file.EndOfStream)
            {
                _currentChar = '\0'; // Конец файла
            }
            else
            {
                _currentChar = (char)_file.Read();
                _column++;
                if (c == '\n')
                {
                    _line++;
                    _column = 1;
                }
            }
            return c;
        }

        // 🍯 Peek — подглядывает символ (не двигая позицию)
        public char Peek()
        {
            return _currentChar;
        }

        // 🧧 Запись ошибки (для таблицы ошибок из задания 0)
        public void LogError(string message)
        {
            Console.WriteLine($"Ошибка в строке {_line}, столбец {_column}: {message}");
        }
    }
}
