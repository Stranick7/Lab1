using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetUI
{
    internal class CellDTO
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public CellDTO(string name, string expression)
        {
            Name = name;
            Expression = expression;
        }
    }
}
