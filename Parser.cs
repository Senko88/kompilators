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
        public List<string> Errors { get; } = new List<string>(); // Новое поле для ошибок
        private readonly Random _random = new Random();
        private readonly Lexer _lexer;
        private Token _currentToken;
        private readonly SemanticAnalyzer _semanticAnalyzer;
        private int _currentLine = 1;
        private readonly Dictionary<string, string> _symbols = new Dictionary<string, string>();
        // Список возможных ошибок (код → сообщение)
        public static readonly Dictionary<int, string> ErrorCodes = new Dictionary<int, string>
    {
 {100, "Отсутствует ;"},
        {101, "Отсутствует . после end"},
        {102, "Неверный тип данных"},
        {103, "Неизвестный символ: {0}"},
        {104, "Ожидалось {0}, но получено {1}"},
        {105, "Неизвестный оператор"},
        {106, "Переменная '{0}' не объявлена"},
        {107, "Отсутствует begin"},
        {108, "Отсутствует end"},
        {109, "Отсутствует :="},
        {110, "Отсутствует '('"},
        {111, "Отсутствует ')'"},
        {112, "Несоответствие типов"},
        {113, "Дублирование идентификатора '{0}'"},
        {114, "Выход за границы массива"},
        {115, "Неверное количество параметров"},
        {116, "Незакрытая скобка: {0}"},
        {117, "Незакрытая кавычка"},
        {118, "Недопустимый символ: {0}"},
        {119, "Целое число {0} вне допустимого диапазона [-32768, 32767]"},
        {120, "Вещественное число {0} вне допустимого диапазона"}
        // ... добавь остальные из твоего списка
    };
        private void ThrowError(int code, params object[] args)
        {
            string message = $"Строка {_lexer._currentLine}: Ошибка {code}: {string.Format(ErrorCodes[code], args)}";
            Errors.Add(message);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public Parser(Lexer lexer, SemanticAnalyzer semanticAnalyzer)
        {
            _lexer = lexer;
            _currentToken = _lexer.NextToken();
            _semanticAnalyzer = semanticAnalyzer; // Принимаем извне
        }
        private int GetApproximateLine()
        {
            // Простая логика для демонстрации
            if (_currentToken.Value == "begin") return 1;
            if (_currentToken.Value == "end") return 3;
            return 2; // По умолчанию
        }
        private void Eat(TokenType expectedType, string expectedValue = null)
        {
            // Проверка соответствия токена
            if (_currentToken.Type != expectedType ||
    (expectedValue != null && _currentToken.Value != expectedValue))
            {
                throw new Exception(
                    $"Ожидалось {(expectedValue ?? expectedType.ToString())}, " +
                    $"но получено {_currentToken.Value} " +
                    $"на строке {GetApproximateLine()}"
                );
            }
        }
        // Метод для вставки случайной ошибки
        private void MaybeInjectError()
        {
            // 20% вероятность ошибки на каждом шаге
            if (_random.Next(0, 100) < 20)
            {
                int errorCode = ErrorCodes.Keys.ElementAt(_random.Next(ErrorCodes.Count));
                string errorMsg = ErrorCodes[errorCode];

                // Подставляем значения в сообщение (если есть {0})
                if (errorMsg.Contains("{0}"))
                    errorMsg = string.Format(errorMsg, _currentToken.Value);

                throw new Exception($"Ошибка {errorCode}: {errorMsg}");
            }
        }

        private void ParseBlock()
        {
            _currentToken = _lexer.NextToken(); // Пропускаем 'begin'

            while (_currentToken.Value != "end")
            {
                ParseStatements();
            }

            _currentToken = _lexer.NextToken(); // Пропускаем 'end'
            if (_currentToken.Value != ".")
            {
                ThrowError(101); // "Отсутствует ."
            }
        }
        public void ParseProgram()
        {
            try
            {
                // 1. Проверяем заголовок программы
                if (_currentToken.Value != "program")
                {
                    ThrowError(104, "program", _currentToken.Value);
                }
                _currentToken = _lexer.NextToken(); // Пропускаем 'program'

                if (_currentToken.Type != TokenType.IDENT)
                {
                    ThrowError(100, "идентификатор программы");
                }
                _currentToken = _lexer.NextToken(); // Пропускаем имя программы

                if (_currentToken.Value != ";")
                {
                    ThrowError(100, ";");
                }
                _currentToken = _lexer.NextToken(); // Пропускаем ;

                // 2. Обрабатываем секцию var (если есть)
                if (_currentToken.Value == "var")
                {
                    _currentToken = _lexer.NextToken(); // Пропускаем 'var'

                    while (_currentToken.Type == TokenType.IDENT)
                    {
                        // Обрабатываем объявление переменной
                        string varName = _currentToken.Value;
                        _currentToken = _lexer.NextToken(); // Пропускаем имя переменной

                        if (_currentToken.Value != ":")
                        {
                            ThrowError(104, ":", _currentToken.Value);
                        }
                        _currentToken = _lexer.NextToken(); // Пропускаем :

                        if (_currentToken.Value != "integer" && _currentToken.Value != "real")
                        {
                            ThrowError(102); // Неверный тип данных
                        }
                        _currentToken = _lexer.NextToken(); // Пропускаем 'integer'


                        if (_currentToken.Value != ";")
                        {
                            ThrowError(100);
                        }
                        _currentToken = _lexer.NextToken(); // Пропускаем ;
                    }
                }

                // 3. Проверяем основной блок
                if (_currentToken.Value != "begin")
                {
                    ThrowError(107); // Отсутствует begin
                }
                _currentToken = _lexer.NextToken(); // Пропускаем 'begin'

                // Обрабатываем содержимое блока
                while (_currentToken.Value != "end")
                {
                    // Здесь должна быть обработка операторов
                    _currentToken = _lexer.NextToken();
                }

                // Проверяем завершение программы
                _currentToken = _lexer.NextToken(); // Пропускаем 'end'
                if (_currentToken.Value != ".")
                {
                    ThrowError(101); // Отсутствует .
                }
                // Проверка незакрытых скобок в конце
                if (_lexer._bracketStack.Count > 0)
                {
                    var (unclosed, line) = _lexer._bracketStack.Pop();
                    ThrowError(116, $"Незакрытая скобка: {unclosed} (начало на строке {line})");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОШИБКА: {ex}");
                Console.ResetColor();
            }
        }

        private void ParseVarSection()
        {
            if (_currentToken.Value == "var")
            {
                Eat(TokenType.KEYWORD, "var");

                while (_currentToken.Type == TokenType.IDENT)
                {
                    string varName = _currentToken.Value;
                    Eat(TokenType.IDENT); // x
                    Eat(TokenType.OPERATOR, ":"); // :

                    // Поддержка integer и real
                    if (_currentToken.Value != "integer" && _currentToken.Value != "real")
                    {
                        ThrowError(102); // Неверный тип данных
                    }
                    string type = _currentToken.Value;
                    Eat(TokenType.KEYWORD); // integer/real

                    // Добавляем переменную в таблицу символов
                    _semanticAnalyzer.AddVariable(varName, type);
                    _symbols[varName] = type; // Дублируем для удобства

                    Eat(TokenType.OPERATOR, ";");
                }
            }
        }
        private void ParseVarDeclaration()
        {
            Eat(TokenType.KEYWORD, "var");

            while (_currentToken.Type == TokenType.IDENT)
            {
                string varName = _currentToken.Value;
                Eat(TokenType.IDENT);
                Eat(TokenType.OPERATOR, ":");

                string typeName = _currentToken.Value;
                Eat(TokenType.KEYWORD); // "integer" или "real"

                // Добавляем переменную в таблицу символов
                _symbols.Add(varName, typeName.ToLower());

                Eat(TokenType.OPERATOR, ";");
            }
        }
        private void ParseAssignment()
        {
            string varName = _currentToken.Value;
            Eat(TokenType.IDENT);
            Eat(TokenType.OPERATOR, ":=");

            // Проверка типа теперь происходит в лексере
            if (_currentToken.Type != TokenType.NUMBER)
            {
                ThrowError(102, "Ожидалось число");
            }

            // Просто пропускаем число (лексер уже проверил диапазон)
            Eat(TokenType.NUMBER);
            Eat(TokenType.OPERATOR, ";");
        }

        private void ParseStatements()
        {
            while (_currentToken.Value != "end")
            {
                MaybeInjectError();

                // Необязательная точка с запятой перед end
                if (_currentToken.Value == ";")
                    Eat(TokenType.OPERATOR, ";");
            }
        }
    }
}
