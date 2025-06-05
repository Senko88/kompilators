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
        public List<string> Errors { get; } = new List<string>(); // Новое поле для ошибок
        private Dictionary<string, string> _symbols = new Dictionary<string, string>();

        // Проверка диапазона для целых чисел (возвращает false, если вне диапазона)

        // Проверка диапазона для вещественных чисел

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
                Console.WriteLine($"Ошибка 102: Число {value.ToString("G17", CultureInfo.InvariantCulture)} вне диапазона real [{RealMin.ToString("G17", CultureInfo.InvariantCulture)}, {RealMax.ToString("G17", CultureInfo.InvariantCulture)}]");
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
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Предупреждение: число {value} вне диапазона integer [{IntegerMin}, {IntegerMax}]");
                Console.ResetColor();
                return false;
            }
            return true;
        }

        // Добавление переменной в таблицу символов
        public void AddVariable(string id, string type)
        {
            _symbols[id] = type;
        }

        // Проверка типа при присваивании
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
    }
}
