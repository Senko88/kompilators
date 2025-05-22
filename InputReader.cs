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
    private int _line = 1, _column = 1;
    private char _currentChar;

    public InputReader(string sourceCode)
    {
        _reader = new StringReader(sourceCode);
        _currentChar = (char)_reader.Read();
    }

    public char NextChar()
    {
        char c = _currentChar;
        if (_reader.Peek() == -1)
        {
            _currentChar = '\0';
        }
        else
        {
            _currentChar = (char)_reader.Read();
            _column++;
            if (c == '\n')
            {
                _line++;
                _column = 1;
            }
        }
        return c;
    }

    public char Peek() => _currentChar;

    public void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
