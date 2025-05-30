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
        private const int IntegerMin = -32768;  // Минимальное значение для integer (32-бит)
        private const int IntegerMax = 32767;   // Максимальное значение для integer
        private const double RealMin = -1.7e308;
        private const double RealMax = 1.7e308;
        private Dictionary<string, string> _symbols = new Dictionary<string, string>();

        public void CheckAssignment(string id, string type, string valueStr)
        {
            if (!_symbols.ContainsKey(id))
                throw new Exception($"Переменная '{id}' не объявлена");

            bool isReal = _symbols[id] == "real";
            bool isInteger = _symbols[id] == "integer";

            try
            {
                if (isReal)
                {
                    double value = double.Parse(valueStr, CultureInfo.InvariantCulture);
                    CheckRealRange(value); // Проверка диапазона
                }
                else if (isInteger)
                {
                    int value = int.Parse(valueStr);
                    CheckIntegerRange(value);
                }
            }
            catch (FormatException)
            {
                throw new Exception($"Неверный формат числа: {valueStr}");
            }
        }

        public void AddVariable(string id, string type)
        {
            _symbols[id] = type;
        }
        public void CheckIntegerRange(int value)
        {
            if (value < IntegerMin || value > IntegerMax)
            {
                throw new Exception($"Ошибка 102: {value} вне диапазона [{IntegerMin}, {IntegerMax}]");
            }
        }

        public void CheckRealRange(double value)
        {
            
            // Стандартная проверка (оставьте на будущее)
            if (value < RealMin || value > RealMax)
            {
                throw new Exception($"Вещественное число {value} вне диапазона IEEE 754 double [{RealMin}, {RealMax}]");
            }
        }
    }
}
