using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace kompilator
{
    public class Parser
    {
        private Lexer _lexer;
        private Token _currentToken;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.NextToken();
        }

        // 🎀 "Съедает" токен, если он совпадает с ожидаемым
        private void Eat(TokenType expectedType, string expectedValue = null)
        {
            if (_currentToken.Type == expectedType &&
                (expectedValue == null || _currentToken.Value == expectedValue))
            {
                _currentToken = _lexer.NextToken();
            }
            else
            {
                throw new Exception($"Ожидалось {expectedType}, но получено {_currentToken.Value}");
            }
        }
        private void ParseExpression()
        {
            if (_currentToken.Value == "(")  // Скобки
            {
                Eat(TokenType.OPERATOR, "(");
                ParseExpression();          // Рекурсия для выражения внутри скобок
                Eat(TokenType.OPERATOR, ")");
            }
            else  // Числа, переменные или ошибка
            {
                if (_currentToken.Type == TokenType.ID)        // Переменная (например, x)
                {
                    Eat(TokenType.ID);
                }
                else if (_currentToken.Type == TokenType.NUMBER)  // Число (например, 42)
                {
                    Eat(TokenType.NUMBER);
                }
                else
                {
                    throw new Exception($"Ожидалось число или переменная, а получено: {_currentToken.Value}");
                }
            }
        }
        private void ParseStatement()
        {
            if (_currentToken.Type == TokenType.KEYWORD &&
        _currentToken.Value.Equals("end", StringComparison.OrdinalIgnoreCase))
                return;
            if (_currentToken.Value == "writeln") // Если это вызов writeln
            {
                Eat(TokenType.KEYWORD, "writeln");
                Eat(TokenType.OPERATOR, "(");  // Скобка открывается
                ParseExpression();             // Разбираем аргумент
                Eat(TokenType.OPERATOR, ")");  // Скобка закрывается
                Eat(TokenType.OPERATOR, ";");
            }
            // Случай 1: Присваивание (x := 5;)
            if (_currentToken.Type == TokenType.ID)
            {
                Eat(TokenType.ID);        // Переменная
                Eat(TokenType.OPERATOR, ":="); // Оператор присваивания
                ParseExpression();        // Разбор выражения (число, сложение и т.д.)
            }
            // Случай 2: Вызов процедуры (writeln(x);)
            else if (_currentToken.Value == "writeln")
            {
                Eat(TokenType.KEYWORD, "writeln");
                Eat(TokenType.OPERATOR, "(");
                ParseExpression();
                Eat(TokenType.OPERATOR, ")");
            }
            else
            {
                throw new Exception($"Неизвестный оператор: {_currentToken.Value}");
            }
        }

        private void ParseBlock()
        {
            Eat(TokenType.KEYWORD, "begin");

            while (true)
            {
                // Жёсткая проверка с выводом
                Console.WriteLine($"Проверка end. Токен: {_currentToken.Type}='{_currentToken.Value}'");
                if (_currentToken.Type == TokenType.KEYWORD &&
                    _currentToken.Value.Equals("end", StringComparison.OrdinalIgnoreCase))
                    break;

                ParseStatement();

                if (!(_currentToken.Type == TokenType.KEYWORD &&
                      _currentToken.Value.Equals("end", StringComparison.OrdinalIgnoreCase)))
                {
                    if (_currentToken.Value == ";") Eat(TokenType.OPERATOR, ";");
                }
            }

            // Финальная проверка
            if (_currentToken.Type != TokenType.KEYWORD ||
                !_currentToken.Value.Equals("end", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Критическая ошибка: Токен end имеет тип {_currentToken.Type}");
            }
            Eat(TokenType.KEYWORD, "end");
        }
        // 🍵 Разбор программы
        public void ParseProgram()
        {
            Eat(TokenType.KEYWORD, "program");
            Eat(TokenType.ID);          // HelloWorld
            Eat(TokenType.OPERATOR, ";");
            ParseVariables();
            ParseBlock();
            Eat(TokenType.OPERATOR, ".");  // Было EOF, стало OPERATOR
        }

        private void ParseVariables()
        {
            if (_currentToken.Value == "var")
            {
                Eat(TokenType.KEYWORD, "var");
                while (_currentToken.Type == TokenType.ID)
                {
                    Eat(TokenType.ID); // Имя переменной
                    Eat(TokenType.COLON);
                    Eat(TokenType.KEYWORD, "integer");
                    Eat(TokenType.OPERATOR, ";");
                }
            }
        }
    }
}
