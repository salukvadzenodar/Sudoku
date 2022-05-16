using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sudoku.ViewModel
{
    public class SudokuViewModel
    {
        private int rows;
        private int cols;
        private int size;
        private CellModel[,] grid;

        public SudokuViewModel(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;

            size = rows * cols;
            grid = new CellModel[size, size];
        }

        public int Size => size;

        public int[,] Grid
        {
            get
            {
                var grid = new int[size, size];

                for (var row = 0; row < size; row++)
                {
                    for (var col = 0; col < size; col++)
                    {
                        grid[row, col] = this[row, col] ?? 0;
                    }
                }

                return grid;
            }
        }

        public TextBox GetControl(int row, int col)
        {
            return grid[row, col].Control;
        }

        public void SetControl(int row, int col, TextBox control)
        {
            grid[row, col] = new CellModel { Control = control };
        }

        public int? this[int row, int col]
        {
            get { return grid[row, col].Value; }
            set { grid[row, col].Value = value; }
        }

        public bool Validate(Brush brush)
        {
            var fillCount = 0;

            bool ValidateCell(int row, int col)
            {
                var control = GetControl(row, col);
                var value = this[row, col];
                if (value == null) return true;
                fillCount++;

                for (var i = 0; i < size; i++)
                {
                    if (i == col) continue;

                    if (this[row, i] == value)
                    {
                        control.Foreground = brush;
                        return false;
                    }
                }

                for (var i = 0; i < size; i++)
                {
                    if (i == row) continue;

                    if (this[i, col] == value)
                    {
                        control.Foreground = brush;
                        return false;
                    }
                }

                var rowStart = (row / rows) * (size / rows);
                var colStart = (col / cols) * (size / cols);

                for (var i = rowStart; i < rowStart + rows; i++)
                {
                    for (var j = colStart; j < colStart + cols; j++)
                    {
                        if (i == row && j == col) continue;

                        if (this[i, j] == value)
                        {
                            control.Foreground = brush;
                            return false;
                        }
                    }
                }

                return true;
            }
            var valid = true;

            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    if (!ValidateCell(row, col))
                    {
                        valid = false;
                    }
                }
            }

            return fillCount >= 3 && valid;
        }

        public void Highlite(Brush brush, bool? filled = null)
        {
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    var control = GetControl(row, col);
                    if (filled != null && string.IsNullOrEmpty(control.Text) == filled.Value) continue;

                    control.Foreground = brush;
                }
            }
        }

        public void SetState(bool enabled, bool? filled = null)
        {
            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    var control = GetControl(row, col);
                    if (filled != null && string.IsNullOrEmpty(control.Text) == filled.Value) continue;

                    control.IsEnabled = enabled;
                }
            }
        }

        class CellModel
        {
            public TextBox Control { get; set; }

            public int? Value
            {
                get
                {
                    if (string.IsNullOrEmpty(Control.Text)) return null;
                    return Convert.ToInt32(Control.Text);
                }

                set
                {
                    Control.Text = value?.ToString() ?? "";
                }
            }
        }
    }
}
