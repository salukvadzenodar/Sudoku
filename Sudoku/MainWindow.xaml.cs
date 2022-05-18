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
        private int cellSize = Defaults.Cell_Size;

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

            SetWindowSize(cellSize);
            CreateMainGrid();
        }

        private void SetWindowSize(int cellSize)
        {
            var blockSize = model.Rows * model.Cols * cellSize;

            Width = blockSize + 10 + 200;
            Height = blockSize + 10;
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
            if (model.State == MainViewState.Started)
            {
                model.State = MainViewState.Stopped;
            }
            else
            {
                model.State = MainViewState.Started;
            }


            //var button = sender as Button;
            //if (button.Content.ToString() == "Stop")
            //{
            //    button.Content = "Start";
            //    model.SetState(true);
            //    model.SetColor(Text_Color);

            //    if (cancelationSource != null)
            //    {
            //        cancelationSource.Cancel();
            //        cancelationSource = null;
            //        SettButtonState(true);
            //    }

            //    return;
            //}

            //txtInfo.Text = "";
            //if (!model.Validate(Text_Danger_Color))
            //{
            //    txtInfo.Text = Invalid_Text;
            //    txtInfo.Foreground = Text_Danger_Color;
            //    return;
            //}

            //button.Content = "Stop";

            //model.SetState(false, true);
            //model.SetColor(Text_Success_Color, true);
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            //if (!model.Validate(Text_Danger_Color))
            //{
            //    txtInfo.Text = Invalid_Text;
            //    txtInfo.Foreground = Text_Danger_Color;
            //    return;
            //}

            //txtInfo.Text = Valid_Text;
            //txtInfo.Foreground = Text_Success_Color;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            //if (MessageBox.Show("Please Confirm", "Clear", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //    Initialize();
            //}
        }

        private void btnParams_Click(object sender, RoutedEventArgs e)
        {
            //var paramsDialog = new ParametersDialog(Rows, Cols);
            //if (paramsDialog.ShowDialog() != true) return;

            //Rows = paramsDialog.Rows;
            //Cols = paramsDialog.Cols;
            //Initialize();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            //    txtInfo.Text = "";
            //    if (!model.Validate(Text_Danger_Color))
            //    {
            //        txtInfo.Text = Invalid_Text;
            //        txtInfo.Foreground = Text_Danger_Color;
            //        return;
            //    }

            //    SettButtonState(false);
            //    model.SetState(false);
            //    model.SetColor(Text_Success_Color, true);
            //    btnStart.Content = "Stop";
            //    btnStart.IsEnabled = true;

            //    var initialSudoku = new Algorithm.Sudoku(Rows, Cols);
            //    for (var i = 0; i < model.Size; i++)
            //    {
            //        for (var j = 0; j < model.Size; j++)
            //        {
            //            initialSudoku[i, j] = model[i, j] ?? 0;
            //        }
            //    }

            //    cancelationSource = new CancellationTokenSource();
            //    var token = cancelationSource.Token;

            //    void ShowSudoku(Algorithm.Sudoku sudoku)
            //    {
            //        for (var i = 0; i < sudoku.Size; i++)
            //        {
            //            for (var j = 0; j < sudoku.Size; j++)
            //            {
            //                var value = sudoku[i, j];
            //                model[i, j] = value == 0 ? null : (int?)value;
            //            }
            //        }
            //    }

            //    Task.Factory.StartNew((_) =>
            //    {
            //        try
            //        {
            //            var time = DateTime.Now;
            //            var sudoku = Algorithm.SudokuSolve.Solve(initialSudoku, token, (s, stackCount, isBack) =>
            //            {
            //                if (DateTime.Now.Subtract(time).TotalSeconds < 1 || token.IsCancellationRequested) return;
            //                time = DateTime.Now;

            //                Application.Current.Dispatcher.Invoke(() =>
            //                {
            //                    ShowSudoku(s);
            //                });
            //            });

            //            if (sudoku == null) return;

            //            Application.Current.Dispatcher.Invoke(() =>
            //            {
            //                txtInfo.Text = "Solved";
            //                txtInfo.Foreground = Text_Success_Color;
            //                btnStart.IsEnabled = false;
            //                btnClear.IsEnabled = true;
            //                btnParams.IsEnabled = true;
            //                ShowSudoku(sudoku);
            //            });
            //        }
            //        catch
            //        {
            //            Application.Current.Dispatcher.Invoke(() =>
            //            {
            //                SettButtonState(true);
            //                btnStart_Click(btnStart, new RoutedEventArgs());

            //                txtInfo.Text = "Unable To Solve";
            //                txtInfo.Foreground = Text_Danger_Color;
            //            });
            //        }
            //    }, TaskCreationOptions.LongRunning, token);
        }
    }
}
