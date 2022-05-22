using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Algorithm
{
    public class Sudoku
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        public int Size => Rows * Cols;
        public bool Solved
        {
            get
            {
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        if (Matrix[i, j] == 0) return false;
                    }
                }

                return true;
            }
        }

        private int[,] Matrix { get; set; }
        public int this[int i, int j]
        {
            get
            {
                return Matrix[i, j];
            }
            set
            {
                Matrix[i, j] = value;
            }
        }

        public Sudoku(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Matrix = new int[Size, Size];
        }

        public Sudoku Clone()
        {
            var sudoku = new Sudoku(Rows, Cols);

            for (var i = 0; i < Size; i++)
                for (var j = 0; j < Size; j++)
                    sudoku[i, j] = Matrix[i, j];

            return sudoku;
        }

        public int GetSection(int row, int column)
        {
            return (row / Rows) * Rows + (column / Cols);
        }

        public (int row, int col) GetSectionStart(int section)
        {
            return (section / Rows * Rows, section % Rows * Cols);
        }

        private int[] PossibleValues(bool[] itemsFilled)
        {
            var count = itemsFilled.Where(x => x).Count();
            var leftItems = new int[Size - count];
            var index = 0;

            for (var i = 0; i < Size; i++)
            {
                if (itemsFilled[i]) continue;

                leftItems[index] = i + 1;
                index++;
            }
            return leftItems;
        }

        public int[] ColumnPossibleValues(int row)
        {
            var temp = new bool[Size];

            for (var i = 0; i < Size; i++)
            {
                if (Matrix[row, i] == 0) continue;
                if (temp[Matrix[row, i] - 1]) return new int[0];

                temp[Matrix[row, i] - 1] = true;
            }

            return PossibleValues(temp);
        }

        public int[] RowPossibleValues(int column)
        {
            var temp = new bool[Size];

            for (var i = 0; i < Size; i++)
            {
                if (Matrix[i, column] == 0) continue;
                if (temp[Matrix[i, column] - 1]) return new int[0];

                temp[Matrix[i, column] - 1] = true;
            }

            return PossibleValues(temp);
        }

        public int[] SectionPossibleValues(int section)
        {
            var temp = new bool[Size];
            var (rowStart, colStart) = GetSectionStart(section);

            for (var i = rowStart; i < rowStart + Rows; i++)
                for (var j = colStart; j < colStart + Cols; j++)
                {
                    if (Matrix[i, j] == 0) continue;
                    if (temp[Matrix[i, j] - 1]) return new int[0];

                    temp[Matrix[i, j] - 1] = true;
                }

            return PossibleValues(temp);
        }

        public int[] PossibleValues(int row, int col)
        {
            if (this[row, col] != 0) return new int[] { this[row, col] };

            var columnPossibleValues = ColumnPossibleValues(row);
            var rowPossibleValues = RowPossibleValues(col);

            var section = GetSection(row, col);
            var sectionIPossibleValues = SectionPossibleValues(section);

            var possibleValues = columnPossibleValues.Intersect(rowPossibleValues).Intersect(sectionIPossibleValues).ToArray();
            return possibleValues;
        }

        public (int row, int col)? FindEmptyPoint()
        {
            for (var i = 0; i < Size; i++)
                for (var j = 0; j < Size; j++)
                    if (this[i, j] == 0) return (i, j);

            return null;
        }

        public (int row, int col, int[] possibleValues)? FindOptimalEmptyPoint()
        {
            int row = -1, column = -1;
            int[] possibleValues = null;

            for (var i = 0; i < Size; i++)
            {
                for (var j = 0; j < Size; j++)
                {
                    if (this[i, j] != 0) continue;
                    var values = PossibleValues(i, j);

                    if (values.Length == 0) return null;
                    if (values.Length <= 2) return (i, j, values);

                    if (possibleValues == null || possibleValues.Length > values.Length)
                    {
                        possibleValues = values;
                        row = i;
                        column = j;
                    }
                }
            }

            if (possibleValues == null) return null;
            return (row, column, possibleValues);
        }

        public Sudoku Simplify()
        {
            var sudoku = Clone();
            var possibleValues = new List<int>[Size, Size];

            bool SetValue(int row, int col, int value)
            {
                sudoku[row, col] = value;
                possibleValues[row, col] = null;

                for (int i = 0; i < Size; i++)
                {
                    if (possibleValues[i, col] == null) continue;

                    possibleValues[i, col].Remove(value);
                    if (possibleValues[i, col].Count == 0) return false;
                }

                for (int i = 0; i < Size; i++)
                {
                    if (possibleValues[row, i] == null) continue;

                    possibleValues[row, i].Remove(value);
                    if (possibleValues[row, i].Count == 0) return false;
                }

                var section = GetSection(row, col);
                var point = GetSectionStart(section);
                for (int i = point.row; i < point.row + Rows; i++)
                {
                    for (int j = point.col; j < point.col + Cols; j++)
                    {
                        if (possibleValues[i, j] == null) continue;

                        possibleValues[i, j].Remove(value);
                        if (possibleValues[i, j].Count == 0) return false;
                    }
                }

                return true;
            }

            bool Process(Sudoku sudoku)
            {
                var again = false;
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        if (sudoku[i, j] > 0) continue;

                        var values = possibleValues[i, j];
                        if (values == null || values.Count == 0) return false;
                        if (values.Count > 1) continue;

                        SetValue(i, j, values[0]);
                        again = true;
                    }
                }

                if (again) return Process(sudoku);
                return true;
            }

            // initialize possible values
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (sudoku[i, j] > 0) continue;

                    var values = PossibleValues(i, j);
                    if (values.Length == 0) return null;

                    possibleValues[i, j] = values.ToList();
                }
            }

            var valid = Process(sudoku);
            return valid ? sudoku : null;
        }
    }
}
