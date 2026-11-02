using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace SpreadsheetUI
{
    public partial class MainPage : ContentPage
    {
        private SpreadsheetModel model = new SpreadsheetModel();
        private bool isExprMode = true;
        private Entry[,] cellEntries;

        public MainPage()
        {
            InitializeComponent();
            RebuildGrid();
        }

        public void RebuildGrid()
        {
            SpreadsheetGrid.Children.Clear();
            SpreadsheetGrid.RowDefinitions.Clear();
            SpreadsheetGrid.ColumnDefinitions.Clear();

            int rows = model.getRowCount();
            int cols = model.getColCount();
            cellEntries = new Entry[rows, cols];

            SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = 35 });
            for (int i = 0; i < rows; i++)
            {
                SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = 40 });
            }

            SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 50 });
            for (int j = 0; j < cols; j++)
            {
                SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            }

            for (int j = 0; j < cols; j++)
            {
                Label colHeader = new Label
                {
                    Text = SpreadsheetModel.GetColumnName(j),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold
                };
                Grid.SetRow(colHeader, 0);
                Grid.SetColumn(colHeader, j + 1);
                SpreadsheetGrid.Children.Add(colHeader);
            }

            for (int i = 0; i < rows; i++)
            {
                Label rowHeader = new Label
                {
                    Text = (i + 1).ToString(),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold
                };
                Grid.SetRow(rowHeader, i + 1);
                Grid.SetColumn(rowHeader, 0);
                SpreadsheetGrid.Children.Add(rowHeader);

                for (int j = 0; j < cols; j++)
                {
                    int r = i, c = j;
                    Entry entry = new Entry
                    {
                        WidthRequest = 100,
                        HeightRequest = 40
                    };

                    Cell cell = model.getCell(r, c);
                    if (cell != null)
                    {
                        entry.Text = cell.GetDisplayValue(isExprMode);
                    }

                    entry.Focused += (s, e) =>
                    {
                        Cell curCell = model.getCell(r, c);
                        if (curCell != null)
                        {
                            entry.Text = curCell.getExpression() ?? "";
                        }
                    };

                    entry.Completed += (s, e) =>
                    {
                        model.ProcessInput(r, c, entry.Text);
                        UpdateGridValues();
                    };

                    entry.Unfocused += (s, e) =>
                    {
                        model.ProcessInput(r, c, entry.Text);
                        UpdateGridValues();
                    };

                    cellEntries[r, c] = entry;
                    Grid.SetRow(entry, r + 1);
                    Grid.SetColumn(entry, c + 1);
                    SpreadsheetGrid.Children.Add(entry);
                }
            }
        }

        public void UpdateGridValues()
        {
            int rows = model.getRowCount();
            int cols = model.getColCount();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Entry entry = cellEntries[i, j];
                    Cell cell = model.getCell(i, j);
                    if (entry != null && cell != null)
                    {
                        if (entry.IsFocused)
                        {
                            entry.Text = cell.getExpression() ?? "";
                        }
                        else
                        {
                            entry.Text = cell.GetDisplayValue(isExprMode);
                        }
                    }
                }
            }
        }

        private void OnAddRowClicked(object sender, EventArgs e)
        {
            model.AddRow();
            RebuildGrid();
        }

        private void OnAddColClicked(object sender, EventArgs e)
        {
            model.AddCol();
            RebuildGrid();
        }

        private void OnToggleModeClicked(object sender, EventArgs e)
        {
            isExprMode = !isExprMode;
            BtnToggleMode.Text = isExprMode ? "Режим: ВИРАЗ" : "Режим: ЗНАЧЕННЯ";
            UpdateGridValues();
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "table.json");
            bool success = model.SaveData(path);

            if (success)
                DisplayAlert("Успіх", "Таблицю успішно збережено!", "OK");
            else
                DisplayAlert("Помилка", "Не вдалося зберегти дані.", "OK");
        }

        private void OnLoadClicked(object sender, EventArgs e)
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "table.json");
            bool success = model.LoadData(path);

            if (success)
            {
                RebuildGrid();
                DisplayAlert("Успіх", "Таблицю завантажено!", "OK");
            }
            else
            {
                DisplayAlert("Помилка", "Не вдалося завантажити дані або файл не існує.", "OK");
            }
        }

        private void OnHelpClicked(object sender, EventArgs e)
        {
            DisplayAlert("Довідка", "Лабораторна робота №1\nВаріант 11\n\nДоступні операції:\n+, -, *, /, ^\nmax(x,y), min(x,y)\ninc(x), dec(x)", "OK");
        }

        private void OnDeleteRowClicked(object sender, EventArgs e)
        {
            model.DeleteRow();
            RebuildGrid();
        }

        private void OnDeleteColClicked(object sender, EventArgs e)
        {
            model.DeleteCol();
            RebuildGrid();
        }
    }
}