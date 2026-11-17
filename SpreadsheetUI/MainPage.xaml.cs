using System;
using System.Collections.Generic;
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
        private int selectedRow = -1;
        private int selectedCol = -1;

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
                    MenuFlyout flyout = new MenuFlyout();

                    MenuFlyoutItem addRowItem = new MenuFlyoutItem { Text = "Додати рядок" };
                    addRowItem.Clicked += (s, e) => { model.AddRow(r); RebuildGrid(); };

                    MenuFlyoutItem addColItem = new MenuFlyoutItem { Text = "Додати стовпець" };
                    addColItem.Clicked += (s, e) => { model.AddCol(c); RebuildGrid(); };

                    MenuFlyoutItem delRowItem = new MenuFlyoutItem { Text = "Видалити рядок" };
                    delRowItem.Clicked += (s, e) => { InfoPanel.IsVisible = false; model.DeleteRow(r); RebuildGrid(); };

                    MenuFlyoutItem delColItem = new MenuFlyoutItem { Text = "Видалити стовпець" };
                    delColItem.Clicked += (s, e) => { InfoPanel.IsVisible = false; model.DeleteCol(c); RebuildGrid(); };

                    flyout.Add(addRowItem);
                    flyout.Add(addColItem);
                    flyout.Add(delRowItem);
                    flyout.Add(delColItem);

                    FlyoutBase.SetContextFlyout(entry, flyout);
                    Cell cell = model.getCell(r, c);
                    if (cell != null)
                    {
                        entry.Text = cell.GetDisplayValue(isExprMode);
                    }

                    entry.Focused += (s, e) =>
                    {
                        selectedRow = r;
                        selectedCol = c;
                        Cell curCell = model.getCell(r, c);
                        if (curCell != null)
                        {
                            entry.Text = curCell.getExpression() ?? "";
                        }
                        UpdateInfoPanel(r, c, entry.Text);
                    };

                    entry.TextChanged += (s, e) =>
                    {
                        if (entry.IsFocused && selectedRow == r && selectedCol == c)
                        {
                            LblCellExpression.Text = $"Вираз: {(string.IsNullOrEmpty(e.NewTextValue) ? "(порожньо)" : e.NewTextValue)}";
                        }
                    };

                    entry.Completed += (s, e) =>
                    {
                        model.ProcessInput(r, c, entry.Text);
                        UpdateGridValues();
                        UpdateInfoPanel(r, c);
                    };

                    entry.Unfocused += (s, e) =>
                    {
                        model.ProcessInput(r, c, entry.Text);
                        UpdateGridValues();
                        if (selectedRow == r && selectedCol == c)
                        {
                            UpdateInfoPanel(r, c);
                        }
                    };

                    cellEntries[r, c] = entry;
                    Grid.SetRow(entry, r + 1);
                    Grid.SetColumn(entry, c + 1);
                    SpreadsheetGrid.Children.Add(entry);
                }
            }
        }

        private void UpdateInfoPanel(int r, int c, string liveText = null)
        {
            Cell cell = model.getCell(r, c);
            if (cell == null)
            {
                InfoPanel.IsVisible = false;
                return;
            }

            InfoPanel.IsVisible = true;
            LblCellTitle.Text = $"Клітинка: {cell.getName()}";

            string expr = liveText ?? cell.getExpression();
            LblCellExpression.Text = $"Вираз: {(string.IsNullOrWhiteSpace(expr) ? "(порожньо)" : expr)}";

            if (string.IsNullOrWhiteSpace(cell.getExpression()))
            {
                LblCellValue.Text = "Значення: (порожньо)";
                LblCellError.IsVisible = false;
            }
            else if (cell.getHasError())
            {
                LblCellValue.Text = "Значення: ERROR";
                LblCellError.Text = $"Причина помилки: {cell.getErrorMessage()}";
                LblCellError.IsVisible = true;
            }
            else
            {
                LblCellValue.Text = $"Значення: {cell.getComputedValue()}";
                LblCellError.IsVisible = false;
            }
        }

        private void OnCloseInfoPanelClicked(object sender, EventArgs e)
        {
            InfoPanel.IsVisible = false;
            selectedRow = -1;
            selectedCol = -1;
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
            model.AddRow(selectedRow);
            RebuildGrid();
        }

        private void OnAddColClicked(object sender, EventArgs e)
        {
            model.AddCol(selectedCol);
            RebuildGrid();
        }

        private void OnToggleModeClicked(object sender, EventArgs e)
        {
            isExprMode = !isExprMode;
            BtnToggleMode.Text = isExprMode ? "Режим: ВИРАЗ" : "Режим: ЗНАЧЕННЯ";
            UpdateGridValues();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                string path = null;

#if WINDOWS
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("JSON файл таблиці", new List<string>() { ".json" });
                savePicker.SuggestedFileName = "table";

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this.Window.Handler.PlatformView);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                var file = await savePicker.PickSaveFileAsync();
                if (file == null)
                    return;

                path = file.Path;
#else
                string fileName = await DisplayPromptAsync("Збереження", "Введіть назву файлу:", "Зберегти", "Скасувати", initialValue: "table.json");
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    fileName += ".json";

                path = Path.Combine(FileSystem.AppDataDirectory, fileName);
#endif

                bool success = model.SaveData(path);

                if (success)
                    await DisplayAlert("Успіх", $"Таблицю успішно збережено:\n{path}", "OK");
                else
                    await DisplayAlert("Помилка", "Не вдалося зберегти дані.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Не вдалося зберегти файл: {ex.Message}", "OK");
            }
        }

        private async void OnLoadClicked(object sender, EventArgs e)
        {
            try
            {
                var jsonFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".json" } },
                        { DevicePlatform.macOS, new[] { "json" } },
                        { DevicePlatform.Android, new[] { "application/json" } },
                        { DevicePlatform.iOS, new[] { "public.json" } }
                    });

                var options = new PickOptions
                {
                    PickerTitle = "Оберіть файл таблиці (.json)",
                    FileTypes = jsonFileType
                };

                var result = await FilePicker.Default.PickAsync(options);
                if (result == null)
                    return;

                bool success = model.LoadData(result.FullPath);

                if (success)
                {
                    InfoPanel.IsVisible = false;
                    RebuildGrid();
                    await DisplayAlert("Успіх", $"Таблицю завантажено:\n{result.FullPath}", "OK");
                }
                else
                {
                    await DisplayAlert("Помилка", "Не вдалося завантажити дані або файл пошкоджено.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка", $"Не вдалося відкрити файл: {ex.Message}", "OK");
            }
        }

        private void OnHelpClicked(object sender, EventArgs e)
        {
            DisplayAlert("Довідка", "Лабораторна робота №1\nВаріант 11\n\nДоступні операції:\n+, -, *, /, ^\nmax(x,y), min(x,y)\ninc(x), dec(x)", "OK");
        }

        private void OnDeleteRowClicked(object sender, EventArgs e)
        {
            InfoPanel.IsVisible = false;
            model.DeleteRow(selectedRow);
            selectedRow = -1;
            RebuildGrid();
        }

        private void OnDeleteColClicked(object sender, EventArgs e)
        {
            InfoPanel.IsVisible = false;
            model.DeleteCol(selectedCol);
            selectedCol = -1;
            RebuildGrid();
        }
    }
}