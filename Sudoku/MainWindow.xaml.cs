using Sudoku.ViewModel;
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
        private const int Cell_Size = 55;
        private static readonly SolidColorBrush Main_Border_Color = new SolidColorBrush(Colors.Black) { Opacity = 0.8 };
        private static readonly SolidColorBrush Text_Color = new SolidColorBrush(Colors.Black) { Opacity = 0.8 };
        private static readonly SolidColorBrush Text_Success_Color = new SolidColorBrush(Colors.Green) { Opacity = 0.8 };
        private static readonly SolidColorBrush Text_Danger_Color = new SolidColorBrush(Colors.Red) { Opacity = 0.8 };

        private static readonly string Invalid_Text = "Invalid Numbers";
        private static readonly string Valid_Text = "Valid";

        private int Rows = 3;
        private int Cols = 3;
        private SudokuViewModel model;
        private CancellationTokenSource cancelationSource;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void SettButtonState(bool state)
        {
            btnStart.IsEnabled = state;
            btnValidate.IsEnabled = state;
            btnSolve.IsEnabled = state;
            btnClear.IsEnabled = state;
            btnParams.IsEnabled = state;
        }

        private void Initialize()
        {
            model = new SudokuViewModel(Rows, Cols);
            cancelationSource = null;

            SetWindowSize();
            CreateMainGrid();

            txtInfo.Text = "";
            btnStart.Content = "Start";
            SettButtonState(true);
        }

        private void SetWindowSize()
        {
            var blockSize = Rows * Cols * Cell_Size;

            Width = blockSize + 10 + 200;
            Height = blockSize + 10;
        }

        private void CreateMainGrid()
        {
            SudokuGrid.Children.Clear();
            SudokuGrid.RowDefinitions.Clear();
            SudokuGrid.ColumnDefinitions.Clear();
            (SudokuGrid.Parent as Border).BorderBrush = Main_Border_Color;

            for (var row = 0; row < Cols; row++)
            {
                SudokuGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (var col = 0; col < Rows; col++)
            {
                SudokuGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }


            for (var row = 0; row < Cols; row++)
            {
                for (var col = 0; col < Rows; col++)
                {
                    var border = new Border
                    {
                        BorderBrush = Main_Border_Color,
                        BorderThickness = new Thickness(0, 0, (col < Rows - 1 ? 1 : 0), (row < Cols - 1 ? 1 : 0))
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

            for (var row = 0; row < Rows; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (var col = 0; col < Cols; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }


            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    var border = new Border
                    {
                        Opacity = 0.8,
                        BorderBrush = new SolidColorBrush(Colors.Gray) { Opacity = 0.5 },
                        BorderThickness = new Thickness(0, 0, (col < Cols - 1 ? 1 : 0), (row < Rows - 1 ? 1 : 0))
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
                        Foreground = Text_Color,
                        MaxLength = Rows * Cols / 10 + 1
                    };
                    textbox.PreviewTextInput += Cell_PreviewInput;
                    CommandManager.AddPreviewExecutedHandler(textbox, Cell_PreviewExecuted);

                    model.SetControl(rowGroup * Rows + row, colGroup * Cols + col, textbox);
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
            if (value > Rows * Cols)
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
            var button = sender as Button;
            if (button.Content.ToString() == "Stop")
            {
                button.Content = "Start";
                model.SetState(true);
                model.Highlite(Text_Color);

                if (cancelationSource != null)
                {
                    cancelationSource.Cancel();
                    cancelationSource = null;
                    SettButtonState(true);
                }

                return;
            }

            txtInfo.Text = "";
            if (!model.Validate(Text_Danger_Color))
            {
                txtInfo.Text = Invalid_Text;
                txtInfo.Foreground = Text_Danger_Color;
                return;
            }

            button.Content = "Stop";

            model.SetState(false, true);
            model.Highlite(Text_Success_Color, true);
        }

        private void btnValidate_Click(object sender, RoutedEventArgs e)
        {
            if (!model.Validate(Text_Danger_Color))
            {
                txtInfo.Text = Invalid_Text;
                txtInfo.Foreground = Text_Danger_Color;
                return;
            }

            txtInfo.Text = Valid_Text;
            txtInfo.Foreground = Text_Success_Color;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Please Confirm", "Clear", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Initialize();
            }
        }

        private void btnParams_Click(object sender, RoutedEventArgs e)
        {
            var paramsDialog = new ParametersDialog(Rows, Cols);
            if (paramsDialog.ShowDialog() != true) return;

            Rows = paramsDialog.Rows;
            Cols = paramsDialog.Cols;
            Initialize();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            txtInfo.Text = "";
            if (!model.Validate(Text_Danger_Color))
            {
                txtInfo.Text = Invalid_Text;
                txtInfo.Foreground = Text_Danger_Color;
                return;
            }

            SettButtonState(false);
            model.SetState(false);
            model.Highlite(Text_Success_Color, true);
            btnStart.Content = "Stop";
            btnStart.IsEnabled = true;

            var initialSudoku = new Algorithm.Sudoku(Rows, Cols);
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

            Task.Factory.StartNew((_) =>
            {
                try
                {
                    var time = DateTime.Now;
                    var sudoku = Algorithm.SudokuSolve.Solve(initialSudoku, token, s =>
                    {
                        if (DateTime.Now.Subtract(time).TotalSeconds < 1) return;
                        time = DateTime.Now;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowSudoku(s);
                        });
                    });

                    if (sudoku == null) return;
                    if (sudoku.Status != Algorithm.Status.Solved) throw new Exception();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        txtInfo.Text = "Solved";
                        txtInfo.Foreground = Text_Success_Color;
                        btnStart.IsEnabled = false;
                        btnClear.IsEnabled = true;
                        btnParams.IsEnabled = true;
                        ShowSudoku(sudoku);
                    });
                }
                catch
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SettButtonState(true);
                        btnStart_Click(btnStart, new RoutedEventArgs());

                        txtInfo.Text = "Unable To Solve";
                        txtInfo.Foreground = Text_Danger_Color;
                    });
                }
            }, TaskCreationOptions.LongRunning, token);
        }
    }
}
