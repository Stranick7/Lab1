using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetUI
{
    internal class TableSaveData
    {
        public int RowCount { get; set; }
        public int ColCount { get; set; }
        public List<CellDTO> Cells { get; set; } = new List<CellDTO>();
        public TableSaveData(int row, int col) {
            RowCount = row;
            ColCount = col;
        }
        public TableSaveData() { }
    }
}
