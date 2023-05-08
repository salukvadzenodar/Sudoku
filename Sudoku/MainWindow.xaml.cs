using Sudoku.Model;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sudoku
{
    public partial class MainWindow : Window
    {
        private MainViewModel model;
        private CancellationTokenSource cancelationSource;
        private double cellWidth = Defaults.Cell_Size;
        private double cellHeight = Defaults.Cell_Size;

        public MainWindow()
        {
            InitializeComponent();
            Initialize(Defaults.Row_Count, Defaults.Cell_Count);
        }

        private void Initialize(int rows, int cells)
        {
            model = new MainViewModel(rows, cells);
            cancelationSource = null;
            DataContext = model;

            SetWindowSize();
            CreateMainGrid();
        }

        private void SetCurrentDimensions()
        {
            var width = Width - 200 - 10;
            var height = Height - 10;

            cellWidth = width / model.Size;
            cellHeight = height / model.Size;
        }

        private void SetWindowSize()
        {
            Width = model.Size * cellWidth + 10 + 200;
            Height = model.Size * cellHeight + 10;
        }

        private void CreateMainGrid()
        {
            SudokuGrid.Children.Clear();
            SudokuGrid.RowDefinitions.Clear();
            SudokuGrid.ColumnDefinitions.Clear();
            (SudokuGrid.Parent as Border).BorderBrush = Defaults.Main_Border_Color;

            for (var row = 0; row < model.Cols; row++)
            {
                SudokuGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (var col = 0; col < model.Rows; col++)
            {
                SudokuGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var row = 0; row < model.Cols; row++)
            {
                for (var col = 0; col < model.Rows; col++)
                {
                    var border = new Border
                    {
                        BorderBrush = Defaults.Main_Border_Color,
                        BorderThickness = new Thickness(0, 0, (col < model.Rows - 1 ? 1 : 0), (row < model.Cols - 1 ? 1 : 0))
                    };

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);

                    SudokuGrid.Children.Add(border);
                    CreateContentGrid(border, row, col);
                }
            }
        }

        private void CreateContentGrid(Border parent, int rowGroup, int colGroup)
        {
            var grid = new Grid();

            for (var row = 0; row < model.Rows; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (var col = 0; col < model.Cols; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var row = 0; row < model.Rows; row++)
            {
                for (var col = 0; col < model.Cols; col++)
                {
                    var border = new Border
                    {
                        Opacity = 0.8,
                        BorderBrush = new SolidColorBrush(Colors.Gray) { Opacity = 0.5 },
                        BorderThickness = new Thickness(0, 0, (col < model.Cols - 1 ? 1 : 0), (row < model.Rows - 1 ? 1 : 0))
                    };

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);

                    var textbox = new TextBox
                    {
                        VerticalContentAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        FontSize = 18,
                        FontWeight = FontWeights.Bold,
                        BorderThickness = new Thickness(0),
                        Foreground = Defaults.Text_Color,
                        MaxLength = model.Rows * model.Cols / 10 + 1
                    };
                    textbox.PreviewTextInput += Cell_PreviewInput;
                    CommandManager.AddPreviewExecutedHandler(textbox, Cell_PreviewExecuted);

                    model.SetControl(rowGroup * model.Rows + row, colGroup * model.Cols + col, textbox);
                    border.Child = textbox;
                    grid.Children.Add(border);
                }
            }

            parent.Child = grid;
        }

        private void Cell_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            var textBox = sender as TextBox;
            var text = textBox.Text.Substring(0, textBox.SelectionStart) + e.Text + textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);

            if (text.StartsWith("0"))
            {
                e.Handled = true;
                return;
            }

            var value = Convert.ToInt32(text);
            if (value > model.Rows * model.Cols)
            {
                e.Handled = true;
            }
        }

        private void Cell_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (model.State == MainViewState.Started || model.State == MainViewState.Solving)
            {
                model.State = MainViewState.Stopped;

                if (cancelationSource != null)
                {
                    cancelationSource.Cancel();
                    cancelationSource = null;
                }

                return;
            }
            else
            {
                if (!model.Validate())
                {
                    model.SetText(Defaults.Invalid_Text, Defaults.Text_Danger_Color);
                    return;
                }

                model.State = MainViewState.Started;
                model.SetText("");
            }
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (!model.Validate())
            {
                model.SetText(Defaults.Invalid_Text, Defaults.Text_Danger_Color);
                return;
            }

            model.SetText(Defaults.Valid_Text, Defaults.Text_Success_Color);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Please Confirm", "Clear", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var rows = model.Rows;
                var cols = model.Cols;

                SetCurrentDimensions();
                Initialize(rows, cols);
            }
        }

        private void btnParams_Click(object sender, RoutedEventArgs e)
        {
            var paramsDialog = new ParametersDialog(model.Rows, model.Cols);
            if (paramsDialog.ShowDialog() != true) return;

            var rows = paramsDialog.Rows;
            var cols = paramsDialog.Cols;

            SetCurrentDimensions();
            Initialize(rows, cols);
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            if (!model.Validate())
            {
                model.SetText(Defaults.Invalid_Text, Defaults.Text_Danger_Color);
                return;
            }

            model.State = MainViewState.Solving;

            var initialSudoku = new Algorithm.Sudoku(model.Rows, model.Cols);
            for (var i = 0; i < model.Size; i++)
            {
                for (var j = 0; j < model.Size; j++)
                {
                    initialSudoku[i, j] = model[i, j] ?? 0;
                }
            }

            cancelationSource = new CancellationTokenSource();
            var token = cancelationSource.Token;

            void ShowSudoku(Algorithm.Sudoku sudoku)
            {
                for (var i = 0; i < sudoku.Size; i++)
                {
                    for (var j = 0; j < sudoku.Size; j++)
                    {
                        var value = sudoku[i, j];
                        model[i, j] = value == 0 ? null : (int?)value;
                    }
                }
            }

            Task.Factory.StartNew(async (_) =>
            {
                try
                {
                    var startTime = DateTime.Now;
                    var sudoku = await Algorithm.SudokuSolve.SolveAsync(initialSudoku, cancelationSource, (s, stackCount, isBack) =>
                    {
                        if (token.IsCancellationRequested) return;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSudoku(s);
                        });
                    });
                    if (sudoku == null) return;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        model.State = MainViewState.Stopped;
                        model.SetText($"Solved in {DateTime.Now.Subtract(startTime).TotalSeconds.ToString("0")} Seconds", Defaults.Text_Success_Color);
                        ShowSudoku(sudoku);
                    });
                }
                catch
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        model.State = MainViewState.Stopped;
                        model.SetText("Unable To Solve", Defaults.Text_Danger_Color);
                    });
                }
            }, TaskCreationOptions.LongRunning, token);
        }
    }
}
