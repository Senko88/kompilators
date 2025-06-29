using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace kompilator
{
    public class SemanticAnalyzer
    {
        public List<string> Errors { get; } = new List<string>();
        private readonly Lexer _lexer;
        // Таблица символов: имя → (тип, является ли функцией)
        private Dictionary<string, (string type, bool isFunction)> _symbols = new Dictionary<string, (string, bool)>();

        // Таблица функций: имя → (возвращаемый тип, список параметров)
        private Dictionary<string, (string returnType, List<(string name, string type)> parameters)> _functions =
            new Dictionary<string, (string, List<(string, string)>)>();

        // Текущий контекст (для проверки внутри функций)
        private string _currentFunction = null;

        /// <summary>
        /// Добавляет переменную в таблицу символов
        /// </summary>
        public void AddVariable(string id, string type)
        {
            if (_symbols.ContainsKey(id))
            {
                Errors.Add($"Строка {_lexer._currentLine}: Переменная '{id}' уже объявлена");
                return;
            }
            _symbols[id] = (type, false);
        }

        /// <summary>
        /// Добавляет функцию в таблицу символов
        /// </summary>
        public void AddFunction(string id, string returnType, List<(string name, string type)> parameters)
        {
            if (_symbols.ContainsKey(id))
            {
                Errors.Add($"Строка {_lexer._currentLine}: Идентификатор '{id}' уже используется");
                return;
            }

            _symbols[id] = (returnType, true);
            _functions[id] = (returnType, parameters);
        }

        /// <summary>
        /// Проверяет правильность присваивания
        /// </summary>
        public void ValidateAssignment(string id, string expressionType)
        {
            if (!_symbols.TryGetValue(id, out var symbol))
            {
                Errors.Add($"Строка {_lexer._currentLine}: Переменная '{id}' не объявлена");
                return;
            }

            if (symbol.isFunction)
            {
                Errors.Add($"Строка {_lexer._currentLine}: '{id}' является функцией, а не переменной");
                return;
            }

            // Проверка совместимости типов
            if (symbol.type != expressionType && !(symbol.type == "real" && expressionType == "integer"))
            {
                Errors.Add($"Строка {_lexer._currentLine}: Несоответствие типов. Нельзя присвоить {expressionType} переменной типа {symbol.type}");
            }
        }

        /// <summary>
        /// Проверяет вызов функции
        /// </summary>
        public string ValidateFunctionCall(string id, List<string> argumentTypes)
        {
            if (!_functions.TryGetValue(id, out var function))
            {
                Errors.Add($"Строка {_lexer._currentLine}: Функция '{id}' не объявлена");
                return "unknown";
            }

            if (function.parameters.Count != argumentTypes.Count)
            {
                Errors.Add($"Строка {_lexer._currentLine}: Неверное количество аргументов. Ожидалось {function.parameters.Count}, получено {argumentTypes.Count}");
                return function.returnType;
            }

            // Проверка типов аргументов
            for (int i = 0; i < function.parameters.Count; i++)
            {
                var param = function.parameters[i];
                var argType = argumentTypes[i];

                if (param.type != argType && !(param.type == "real" && argType == "integer"))
                {
                    Errors.Add($"Строка {_lexer._currentLine}: Несоответствие типа аргумента {i + 1}. Ожидался {param.type}, получен {argType}");
                }
            }

            return function.returnType;
        }

        /// <summary>
        /// Проверяет тип выражения
        /// </summary>
        public string ValidateExpression(Token op, string leftType, string rightType)
        {
            // Для арифметических операторов
            if (new[] { "+", "-", "*", "/" }.Contains(op.Value))
            {
                if (leftType == "integer" && rightType == "integer")
                    return "integer";

                if ((leftType == "real" || rightType == "real") &&
                    (leftType != "unknown" && rightType != "unknown"))
                    return "real";

                Errors.Add($"Строка {_lexer._currentLine}: Несовместимые типы для оператора {op.Value}: {leftType} и {rightType}");
                return "unknown";
            }

            // Для операторов сравнения
            if (new[] { "<", ">", "<=", ">=", "=", "<>" }.Contains(op.Value))
            {
                if ((leftType == rightType) ||
                    (leftType == "integer" && rightType == "real") ||
                    (leftType == "real" && rightType == "integer"))
                    return "boolean";

                Errors.Add($"Строка {_lexer._currentLine}: Нельзя сравнивать {leftType} и {rightType}");
                return "unknown";
            }

            return "unknown";
        }

        /// <summary>
        /// Проверяет, существует ли идентификатор
        /// </summary>
        public bool IdentifierExists(string id)
        {
            return _symbols.ContainsKey(id);
        }

        /// <summary>
        /// Получает тип идентификатора
        /// </summary>
        public string GetIdentifierType(string id)
        {
            return _symbols.TryGetValue(id, out var symbol) ? symbol.type : "unknown";
        }

        /// <summary>
        /// Входит в контекст функции
        /// </summary>
        public void EnterFunctionContext(string functionName)
        {
            _currentFunction = functionName;
        }

        /// <summary>
        /// Выходит из контекста функции
        /// </summary>
        public void ExitFunctionContext()
        {
            _currentFunction = null;
        }
    }
}
