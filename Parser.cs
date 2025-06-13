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
        private int _currentLine = 1;
        private readonly Dictionary<string, string> _symbols = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _variables = new();

        public List<string> AllErrors { get; } = new(); // Все ошибки парсера и семантики

        // Список возможных ошибок (код → сообщение)
        public static readonly Dictionary<int, string> ErrorCodes = new Dictionary<int, string>
        {
            { 100, "Отсутствует ;" },
            { 101, "Отсутствует . после end" },
            { 102, "Неверный тип данных" },
            { 103, "Неизвестный символ: {0}" },
            { 104, "Ожидалось {0}, но получено {1}" },
            { 105, "Неизвестный оператор" },
            { 106, "Переменная '{0}' не объявлена" },
            { 107, "Отсутствует begin" },
            { 108, "Отсутствует end" },
            { 109, "Отсутствует :=" },
            { 110, "Отсутствует '('" },
            { 111, "Отсутствует ')'" },
            { 112, "Несоответствие типов" },
            { 113, "Дублирование идентификатора '{0}'" },
            { 114, "Выход за границы массива" },
            { 115, "Неверное количество параметров" },
            { 116, "Выход за диапазон" }
            // ... добавь остальные из твоего списка
        };

        private void AddVariable(string name, string type)
        {
            _variables[name] = type;
        }

        public bool CheckRealRange(double value)
        {
            const double RealMin = -1.7976931348623157E+308;
            const double RealMax = 1.7976931348623157E+308;

            if (double.IsInfinity(value))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка 102: Число слишком большое (Infinity)");
                Console.ResetColor();
                return false;
            }

            if (value < RealMin || value > RealMax)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(
                    $"Ошибка 102: Число {value.ToString("G17", CultureInfo.InvariantCulture)} вне диапазона real [{RealMin.ToString("G17", CultureInfo.InvariantCulture)}, {RealMax.ToString("G17", CultureInfo.InvariantCulture)}]");
                Console.ResetColor();
                return false;
            }

            return true;
        }


        public bool CheckIntegerRange(long value)
        {
            const int IntegerMin = -32768;
            const int IntegerMax = 32767;

            if (value < IntegerMin || value > IntegerMax)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка: число {value} вне диапазона integer [{IntegerMin}, {IntegerMax}]");
                Console.ResetColor();
                return false;
            }

            return true;
        }

        public bool CheckAssignment(string id, string valueStr)
        {
            if (!_symbols.ContainsKey(id))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка: переменная '{id}' не объявлена");
                Console.ResetColor();
                return false;
            }

            string type = _symbols[id];
            if (type == "integer")
            {
                // Исправлено: TryParse вместо IryParse
                if (!long.TryParse(valueStr, out long intValue))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка: '{valueStr}' не является целым числом");
                    Console.ResetColor();
                    return false;
                }

                return CheckIntegerRange(intValue);
            }
            else if (type == "real")
            {
                // Исправлено: TryParse и realValue вместо realValve
                if (!double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double realValue))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка: '{valueStr}' не является вещественным числом");
                    Console.ResetColor();
                    return false;
                }

                return CheckRealRange(realValue);
            }

            return true;
        }

        private void ThrowError(int code, params object[] args)
        {
            string message = $"Строка {_currentLine}: Ошибка {code}: {string.Format(ErrorCodes[code], args)}";
            Errors.Add(message); // Добавляем ошибку в список
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message); // Выводим сразу (опционально)
            Console.ResetColor();
        }

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _currentToken = _lexer.NextToken();

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
                    errorMsg = string.Format(errorMsg, _currentToken?.Value ?? "null");

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
                    ParseStatements();
                }

                // Проверяем завершение программы
                _currentToken = _lexer.NextToken(); // Пропускаем 'end'
                if (_currentToken.Value != ".")
                {
                    ThrowError(101); // Отсутствует .
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
                if (!_symbols.ContainsKey(varName))
                {
                    ThrowError(106, varName); // Переменная не объявлена
                    return;
                }

                string varType = _symbols[varName];
                Eat(TokenType.IDENT);
                Eat(TokenType.OPERATOR, ":=");

                if (_currentToken.Type == TokenType.NUMBER)
                {
                    string value = _currentToken.Value;

                    if (varType == "integer")
                    {
                        if (!long.TryParse(value, out long intValue))
                        {
                            ThrowError(102, $"Некорректный формат целого числа: {value}");
                        }
                        else if (!CheckIntegerRange(intValue))
                        {
                            ThrowError(116, $"Число {value} вне диапазона integer");
                        }
                    }
                    else if (varType == "real")
                    {
                        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture,
                                out double realValue))
                        {
                            ThrowError(102, $"Некорректный формат вещественного числа: {value}");
                        }
                        else if (!CheckRealRange(realValue))
                        {
                            ThrowError(116, $"Число {value} вне диапазона real");
                        }
                    }

                    Eat(TokenType.NUMBER);
                }
                else
                {
                    ThrowError(102, "Ожидалось число");
                }

                Eat(TokenType.OPERATOR, ";");
            }

        private void ParseStatements()
        {
            while (_currentToken != null && _currentToken.Value != "end")
            {
                MaybeInjectError();

                if (_currentToken.Type == TokenType.IDENT)
                {
                    ParseAssignment(); // Внутри этого метода токены продвигаются
                }
                else if (_currentToken.Value == ";")
                {
                    _currentToken = _lexer.NextToken(); // Пропускаем ;
                }
                else
                {
                    ThrowError(105, _currentToken.Value); // "Неизвестный оператор"
                    _currentToken = _lexer.NextToken(); // Пропускаем ошибочный токен
                }
            }
        }
        }
    }
