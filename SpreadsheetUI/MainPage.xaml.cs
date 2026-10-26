namespace SpreadsheetUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private SpreadsheetModel model = new SpreadsheetModel();
        private bool isExprMode = true;
        public MainPage()
        {
            InitializeComponent();
            RefreshGrid();
        }
        public void RefreshGrid()
        {
            SpreadsheetGrid.Children.Clear();
            SpreadsheetGrid.RowDefinitions.Clear();
            SpreadsheetGrid.ColumnDefinitions.Clear();
            int a = model.getRowCount();
            int b = model.getColCount();
            for (int i = 0; i < a; i++)
            {
                SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = 40 });
            }
            for (int j = 0; j < b; j++)
            {
                SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });
            }
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
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
                    entry.Completed += (s, e) =>
                    {
                        model.ProcessInput(r, c, entry.Text);
                        RefreshGrid();
                    };
                    entry.Unfocused += (s, e) =>
                    {
                        model.ProcessInput(r, c, entry.Text);
                    };
                    Grid.SetRow(entry, r);
                    Grid.SetColumn(entry, c);
                    SpreadsheetGrid.Children.Add(entry);
                }
            }
        }
        private void OnAddRowClicked(object sender, EventArgs e)
        {
            model.AddRow();
            int newRowIndex = model.getRowCount() - 1;
            SpreadsheetGrid.RowDefinitions.Add(new RowDefinition { Height = 40 });
            for (int j = 0; j < model.getColCount(); j++)
            {
                int r = newRowIndex;
                int c = j;

                Entry entry = new Entry { WidthRequest = 100, HeightRequest = 40 };
                Cell cell = model.getCell(r, c);
                if (cell != null)
                {
                    entry.Text = cell.GetDisplayValue(isExprMode);
                }

                entry.Completed += (s, ev) => { model.ProcessInput(r, c, entry.Text); RefreshGrid(); };
                entry.Unfocused += (s, ev) => { model.ProcessInput(r, c, entry.Text); };

                Grid.SetRow(entry, r);
                Grid.SetColumn(entry, c);
                SpreadsheetGrid.Children.Add(entry);
            }
        }

        private void OnAddColClicked(object sender, EventArgs e)
        {
            model.AddCol();
            int newColIndex = model.getColCount() - 1;

            SpreadsheetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 100 });

            for (int i = 0; i < model.getRowCount(); i++)
            {
                int r = i;
                int c = newColIndex;

                Entry entry = new Entry { WidthRequest = 100, HeightRequest = 40 };
                Cell cell = model.getCell(r, c);
                if (cell != null)
                {
                    entry.Text = cell.GetDisplayValue(isExprMode);
                }

                entry.Completed += (s, ev) => { model.ProcessInput(r, c, entry.Text); RefreshGrid(); };
                entry.Unfocused += (s, ev) => { model.ProcessInput(r, c, entry.Text); };

                Grid.SetRow(entry, r);
                Grid.SetColumn(entry, c);
                SpreadsheetGrid.Children.Add(entry);
            }
        }

        private void OnToggleModeClicked(object sender, EventArgs e)
        {
            isExprMode = !isExprMode;
            BtnToggleMode.Text = isExprMode ? "Режим: ВИРАЗ" : "Режим: ЗНАЧЕННЯ";
            RefreshGrid();
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
                RefreshGrid();
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
            RefreshGrid();
        }
        private void OnDeleteColClicked(object sender, EventArgs e)
        {
            model.DeleteCol();
            RefreshGrid();
        }
    }

}
