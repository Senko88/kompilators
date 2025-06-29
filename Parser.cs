using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
namespace kompilator
{
    public class Parser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;
        private readonly SemanticAnalyzer _semanticAnalyzer;
        public List<string> Errors { get; } = new List<string>();

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.NextToken();
            _semanticAnalyzer = new SemanticAnalyzer();
        }

        public void ParseProgram()
        {
            try
            {
                // Пропускаем заголовок программы (он не обязателен)
                if (CurrentTokenIs("program"))
                {
                    _currentToken = _lexer.NextToken(); // 'program'
                    if (_currentToken.Type == TokenType.IDENT)
                    {
                        _currentToken = _lexer.NextToken(); // имя программы
                    }
                    if (CurrentTokenIs(";"))
                    {
                        _currentToken = _lexer.NextToken(); // ';'
                    }
                }

                // Раздел объявлений
                ParseDeclarations();

                // Основной блок
                if (!CurrentTokenIs("begin"))
                {
                    AddError("Ожидалось начало основного блока 'begin'");
                    return;
                }

                _currentToken = _lexer.NextToken(); // 'begin'

                // Пропускаем все до 'end'
                while (!CurrentTokenIs("end") && _currentToken.Type != TokenType.EOF)
                {
                    _currentToken = _lexer.NextToken();
                }

                if (!CurrentTokenIs("end"))
                {
                    AddError("Ожидалось завершение блока 'end'");
                    return;
                }

                _currentToken = _lexer.NextToken(); // 'end'

                // Проверяем точку в конце программы
                if (!CurrentTokenIs("."))
                {
                    AddError("Ожидалось '.' в конце программы");
                }
            }
            catch (Exception ex)
            {
                AddError($"Критическая ошибка: {ex.Message}");
            }
        }

        private void ParseDeclarations()
        {
            // Раздел переменных
            if (CurrentTokenIs("var"))
            {
                _currentToken = _lexer.NextToken(); // 'var'

                while (_currentToken.Type == TokenType.IDENT)
                {
                    string varName = _currentToken.Value;
                    _currentToken = _lexer.NextToken(); // имя

                    if (CurrentTokenIs(":"))
                    {
                        _currentToken = _lexer.NextToken(); // ':'
                        string varType = _currentToken.Value;
                        _semanticAnalyzer.AddVariable(varName, varType);
                        _currentToken = _lexer.NextToken(); // тип
                    }

                    if (CurrentTokenIs(";"))
                    {
                        _currentToken = _lexer.NextToken(); // ';'
                    }
                }
            }

            // Раздел функций
            while (CurrentTokenIs("function") || CurrentTokenIs("procedure"))
            {
                ParseFunction();
            }
        }

        private void ParseFunction()
        {
            _currentToken = _lexer.NextToken(); // 'function/procedure'
            if (_currentToken.Type == TokenType.IDENT)
            {
                _currentToken = _lexer.NextToken(); // имя функции
            }

            // Параметры
            if (CurrentTokenIs("("))
            {
                _currentToken = _lexer.NextToken(); // '('
                while (!CurrentTokenIs(")") && _currentToken.Type != TokenType.EOF)
                {
                    _currentToken = _lexer.NextToken();
                }
                if (CurrentTokenIs(")"))
                {
                    _currentToken = _lexer.NextToken(); // ')'
                }
            }

            // Возвращаемый тип
            if (CurrentTokenIs(":"))
            {
                _currentToken = _lexer.NextToken(); // ':'
                _currentToken = _lexer.NextToken(); // тип
            }

            if (CurrentTokenIs(";"))
            {
                _currentToken = _lexer.NextToken(); // ';'
            }

            // Тело функции
            if (CurrentTokenIs("begin"))
            {
                _currentToken = _lexer.NextToken(); // 'begin'
                while (!CurrentTokenIs("end") && _currentToken.Type != TokenType.EOF)
                {
                    _currentToken = _lexer.NextToken();
                }
                if (CurrentTokenIs("end"))
                {
                    _currentToken = _lexer.NextToken(); // 'end'
                }
            }

            if (CurrentTokenIs(";"))
            {
                _currentToken = _lexer.NextToken(); // ';'
            }
        }

        private bool CurrentTokenIs(string value)
            => _currentToken.Value?.Equals(value, StringComparison.OrdinalIgnoreCase) ?? false;

        private void AddError(string message)
        {
            string error = $"Строка {_lexer._currentLine}: {message}";
            if (!Errors.Contains(error)) // избегаем дублирования ошибок
            {
                Errors.Add(error);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ResetColor();
            }
        }
    }
}

