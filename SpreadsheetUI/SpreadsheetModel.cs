using Antlr4.Runtime;
using Lab1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace SpreadsheetUI
{
    internal class SpreadsheetModel
    {
        private Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
        private int rowCount;
        private int colCount;

        public SpreadsheetModel()
        {
            rowCount = 20;
            colCount = 20;
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    string v = GetCellName(i, j);
                    cells.Add(v, new Cell(v));
                }
            }
        }

        public static string GetColumnName(int colIndex)
        {
            string name = "";
            while (colIndex >= 0)
            {
                name = (char)('A' + (colIndex % 26)) + name;
                colIndex = (colIndex / 26) - 1;
            }
            return name;
        }

        public static string GetCellName(int row, int col)
        {
            return GetColumnName(col) + (row + 1);
        }

        public void AddRow(int rowIndex = -1)
        {
            if (rowIndex < 0 || rowIndex >= rowCount)
                rowIndex = rowCount;
            for (int r = rowCount - 1; r >= rowIndex; r--)
            {
                for (int c = 0; c < colCount; c++)
                {
                    string oldName = GetCellName(r, c);
                    string newName = GetCellName(r + 1, c);
                    if (cells.ContainsKey(oldName))
                    {
                        Cell cell = cells[oldName];
                        cells.Remove(oldName);
                        cell.setName(newName);
                        cells[newName] = cell;
                    }
                }
            }
            rowCount++;
            for (int j = 0; j < colCount; j++)
            {
                string v = GetCellName(rowIndex, j);
                cells[v] = new Cell(v);
            }
            RecalculateAll();
        }

        public void AddCol(int colIndex = -1)
        {
            if (colIndex < 0 || colIndex >= colCount)
                colIndex = colCount;
            for (int c = colCount - 1; c >= colIndex; c--)
            {
                for (int r = 0; r < rowCount; r++)
                {
                    string oldName = GetCellName(r, c);
                    string newName = GetCellName(r, c + 1);
                    if (cells.ContainsKey(oldName))
                    {
                        Cell cell = cells[oldName];
                        cells.Remove(oldName);
                        cell.setName(newName);
                        cells[newName] = cell;
                    }
                }
            }
            colCount++;

            for (int i = 0; i < rowCount; i++)
            {
                string v = GetCellName(i, colIndex);
                cells[v] = new Cell(v);
            }
            RecalculateAll();
        }

        public void DeleteRow(int rowIndex = -1)
        {
            if (rowCount <= 1) return;
            if (rowIndex < 0 || rowIndex >= rowCount)
                rowIndex = rowCount - 1;

            for (int c = 0; c < colCount; c++)
            {
                string v = GetCellName(rowIndex, c);
                cells.Remove(v);
            }

            for (int r = rowIndex + 1; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    string oldName = GetCellName(r, c);
                    string newName = GetCellName(r - 1, c);
                    if (cells.ContainsKey(oldName))
                    {
                        Cell cell = cells[oldName];
                        cells.Remove(oldName);
                        cell.setName(newName);
                        cells[newName] = cell;
                    }
                }
            }
            rowCount--;
            RecalculateAll();
        }

        public void DeleteCol(int colIndex = -1)
        {
            if (colCount <= 1) return;
            if (colIndex < 0 || colIndex >= colCount)
                colIndex = colCount - 1;

            for (int r = 0; r < rowCount; r++)
            {
                string v = GetCellName(r, colIndex);
                cells.Remove(v);
            }

            for (int c = colIndex + 1; c < colCount; c++)
            {
                for (int r = 0; r < rowCount; r++)
                {
                    string oldName = GetCellName(r, c);
                    string newName = GetCellName(r, c - 1);
                    if (cells.ContainsKey(oldName))
                    {
                        Cell cell = cells[oldName];
                        cells.Remove(oldName);
                        cell.setName(newName);
                        cells[newName] = cell;
                    }
                }
            }
            colCount--;
            RecalculateAll();
        }

        public void ClearAllCells()
        {
            foreach (Cell cell in cells.Values)
            {
                cell.expressionDelete();
                cell.computedValueZero();
                cell.setHasError(false);
            }
        }

        public bool SaveData(string filePath)
        {
            try
            {
                TableSaveData data = new TableSaveData(rowCount, colCount);
                foreach (Cell cell in cells.Values)
                {
                    data.Cells.Add(new CellDTO(cell.getName(), cell.getExpression()));
                }
                File.WriteAllText(filePath, JsonSerializer.Serialize(data));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool LoadData(string filePath)
        {
            try
            {
                TableSaveData data = JsonSerializer.Deserialize<TableSaveData>(File.ReadAllText(filePath));
                if (data == null) return false;
                rowCount = data.RowCount;
                colCount = data.ColCount;
                cells.Clear();
                foreach (CellDTO cell in data.Cells)
                {
                    Cell c = new Cell(cell.Name);
                    c.SetExpression(cell.Expression);
                    cells[cell.Name] = c;
                }
                RecalculateAll();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ProcessInput(int row, int col, string input)
        {
            string v = GetCellName(row, col);
            if (!cells.ContainsKey(v)) return;
            cells[v].SetExpression(input);
            RecalculateAll();
        }

        public void RecalculateAll()
        {
            HashSet<string> evaluating = new HashSet<string>();
            HashSet<string> evaluated = new HashSet<string>();

            foreach (string key in cells.Keys)
            {
                if (!evaluated.Contains(key))
                {
                    try
                    {
                        EvaluateCell(key, evaluating, evaluated);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private BigInteger EvaluateCell(string cellName, HashSet<string> evaluating, HashSet<string> evaluated)
        {
            if (!cells.ContainsKey(cellName))
            {
                throw new ArgumentException($"Клітинка {cellName} не існує");
            }

            Cell cell = cells[cellName];

            if (evaluating.Contains(cellName))
            {
                cell.setHasError(true);
                cell.setErrorMessage("Циклічне посилання між клітинками");
                throw new InvalidOperationException($"Циклічне посилання ({cellName})");
            }

            if (evaluated.Contains(cellName))
            {
                if (cell.getHasError())
                {
                    throw new ArgumentException($"Помилка в клітинці {cellName}: {cell.getErrorMessage()}");
                }
                return cell.getComputedValue();
            }

            evaluating.Add(cellName);
            string input = cell.getExpression();

            if (string.IsNullOrWhiteSpace(input))
            {
                cell.setHasError(false);
                cell.setErrorMessage(null);
                cell.computedValueZero();
                evaluating.Remove(cellName);
                evaluated.Add(cellName);
                return 0;
            }

            try
            {
                AntlrInputStream ant = new AntlrInputStream(input);
                CalculatorLexer lex = new CalculatorLexer(ant);
                CommonTokenStream com = new CommonTokenStream(lex);
                CalculatorParser parser = new CalculatorParser(com);
                var tree = parser.compileUnit();

                if (parser.NumberOfSyntaxErrors > 0)
                {
                    throw new ArgumentException("Синтаксична помилка у формулі");
                }

                CalculatorVisitor visitor = new CalculatorVisitor(refName => EvaluateCell(refName, evaluating, evaluated));
                BigInteger result = visitor.Visit(tree);

                cell.setComputedValue(result);
                cell.setHasError(false);
                cell.setErrorMessage(null);
                evaluating.Remove(cellName);
                evaluated.Add(cellName);
                return result;
            }
            catch (Exception ex)
            {
                cell.setHasError(true);
                cell.setErrorMessage(ex.Message);
                cell.computedValueZero();
                evaluating.Remove(cellName);
                evaluated.Add(cellName);
                throw;
            }
        }

        public int getRowCount() { return rowCount; }
        public int getColCount() { return colCount; }

        public Cell getCell(int row, int col)
        {
            string v = GetCellName(row, col);
            return cells.ContainsKey(v) ? cells[v] : null;
        }
    }
}