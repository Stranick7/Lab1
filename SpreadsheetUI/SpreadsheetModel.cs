using Antlr4.Runtime;
using Lab1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace SpreadsheetUI
{
    internal class SpreadsheetModel
    {
        private Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
        private int rowCount;
        private int colCount;
        private CalculatorVisitor analyzer = new CalculatorVisitor();
        public SpreadsheetModel() {
            rowCount = 5;
            colCount = 10;
            for(int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    String v = ((char)('A' + j)).ToString() + (i+1);
                    cells.Add(v, new Cell(v));
                }
            }
        }
        public void AddRow()
        {
            rowCount++;
            for(int i = 0;i < colCount; i++)
            {
                String v = ((char)('A' + i)).ToString() + rowCount;
                cells.Add(v, new Cell(v));
            }
        }
        public void AddCol()
        {
            colCount++;
            for (int i = 0; i < rowCount; i++)
            {
                String v = ((char)('A' + colCount - 1)).ToString() + (i+1);
                cells.Add(v, new Cell(v));
            }
        }
        public void ClearAllCells()
        {
            foreach (Cell cell in cells.Values)
            {
                cell.expressionDelete();
                cell.computedValueZero();
            }
        }
        public bool SaveData(string filePath)
        {
            try
            {
                TableSaveData data = new TableSaveData(rowCount, colCount);
                foreach(Cell cell in cells.Values)
                {
                    data.Cells.Add(new CellDTO(cell.getName(), cell.getExpression()));
                }
                File.WriteAllText(filePath, JsonSerializer.Serialize(data));
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool LoadData(string filePath)
        {
            try
            {
                TableSaveData data = JsonSerializer.Deserialize<TableSaveData>(File.ReadAllText(filePath));
                rowCount = data.RowCount;
                colCount = data.ColCount;
                cells.Clear(); 
                foreach (CellDTO cell in data.Cells)
                {
                    Cell c = new Cell(cell.Name);
                    cells.Add(cell.Name, c);
                    c.SetExpression(cell.Expression);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void ProcessInput(int row, int col, string input)
        {
            String v = ((char)('A' + col)).ToString() + (row + 1);
            Cell cur = cells[v];
            cur.SetExpression(input);
            cur.setHasError(false);
            if (string.IsNullOrEmpty(input)) {
                cur.computedValueZero();
                return; }
            try
            {
                AntlrInputStream ant = new AntlrInputStream(input);
                CalculatorLexer lex = new CalculatorLexer(ant);
                CommonTokenStream com = new CommonTokenStream(lex);
                CalculatorParser parser = new CalculatorParser(com);
                var tree = parser.compileUnit();
                BigInteger result = analyzer.Visit(tree);
                cur.setComputedValue(result);
            }
            catch (Exception e)
            {
                cur.setHasError(true);
            }
        }
        public int getRowCount() { return rowCount; }
        public int getColCount() { return colCount; }
        public Cell getCell(int row, int col)
        {
            String v = ((char)('A' + col)).ToString() + (row +  1) ;
            return cells[v];
        }
    }
   
}
