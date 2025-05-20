using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kompilator
{
    public class SemanticAnalyzer
    {
        private Dictionary<string, string> _symbols = new Dictionary<string, string>();

        public void CheckAssignment(string id, string type)
        {
            if (!_symbols.ContainsKey(id))
                throw new Exception($"Переменная {id} не объявлена!");
            if (_symbols[id] != type)
                throw new Exception($"Нельзя присвоить {type} к {_symbols[id]}");
        }

        public void AddVariable(string id, string type)
        {
            _symbols[id] = type;
        }
    }
}
