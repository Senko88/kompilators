using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

// InputReader.cs
public class InputReader
{
    public int CurrentLine { get; private set; } = 1;
    public int CurrentColumn { get; private set; } = 1;

    private readonly StringReader _reader;
    private char _currentChar;

    public InputReader(string sourceCode)
    {
        _reader = new StringReader(sourceCode);
        _currentChar = (char)_reader.Read(); // Инициализируем первый символ
    }

    public char NextChar()
    {
        char c = _currentChar;

        // Читаем следующий символ
        int next = _reader.Read();
        _currentChar = next == -1 ? '\0' : (char)next;

        // Обновляем позицию
        if (c == '\n')
        {
            CurrentLine++;
            CurrentColumn = 1;
        }
        else
        {
            CurrentColumn++;
        }

        return c;
    }

    public char Peek()
    {
        return _currentChar;
    }

    public char PeekNext()
    {
        int next = _reader.Peek();
        return next == -1 ? '\0' : (char)next;
    }
    public void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
