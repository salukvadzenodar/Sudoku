using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Algorithm
{
    public class Sudoku
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        public int Size => Rows * Cols;
        public bool IsOptimized => Data != null;

        private AuxilaryData Data { get; set; }

        public bool Solved
        {
            get
            {
                if (Data != null) return Data.Solved;

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
                var valueChanged = Matrix[i, j] != 0 && Matrix[i, j] != value;
                Matrix[i, j] = value;

                if (Data != null)
                {
                    if (!valueChanged)
                    {
                        int section = GetSection(i, j);
                        // row should be removed from col index and column from row index
                        Data.Remove(j, i, section, value);
                    }
                    else
                    {
                        BuildData();
                    }
                }
            }
        }

        public Sudoku(int rows, int cols, bool optimize = true)
        {
            Rows = rows;
            Cols = cols;
            Matrix = new int[Size, Size];

            if (optimize)
            {
                BuildData();
            }
        }

        public Sudoku Clone()
        {
            var sudoku = new Sudoku(Rows, Cols);
            sudoku.Data = Data?.Clone();

            for (var i = 0; i < Size; i++)
                for (var j = 0; j < Size; j++)
                    sudoku[i, j] = Matrix[i, j];

            return sudoku;
        }

        private void BuildData()
        {
            Data = null;
            var auxilaryData = new AuxilaryData(Size);

            for (var i = 0; i < Size; i++)
            {
                auxilaryData.RowPossibleValues[i] = RowPossibleValues(i).ToList();
                auxilaryData.ColumnPossibleValues[i] = ColumnPossibleValues(i).ToList();
                auxilaryData.SectionPossibleValues[i] = SectionPossibleValues(i).ToList();
            }

            auxilaryData.CheckSolved();
            Data = auxilaryData;
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
            var count = 0;
            for (var i = 0; i < itemsFilled.Length; i++)
            {
                if (!itemsFilled[i]) count++;
            }

            var leftItems = new int[count];
            var index = 0;

            for (var i = 0; i < Size; i++)
            {
                if (itemsFilled[i]) continue;

                leftItems[index] = i + 1;
                index++;
            }

            return leftItems;
        }

        public int[] RowPossibleValues(int column)
        {
            if (Data != null)
            {
                return Data.RowPossibleValues[column]?.ToArray() ?? new int[0];
            }

            var temp = new bool[Size];
            for (var i = 0; i < Size; i++)
            {
                if (Matrix[i, column] == 0) continue;
                if (temp[Matrix[i, column] - 1]) return new int[0];

                temp[Matrix[i, column] - 1] = true;
            }

            return PossibleValues(temp);
        }

        public int[] ColumnPossibleValues(int row)
        {
            if (Data != null)
            {
                return Data.ColumnPossibleValues[row]?.ToArray() ?? new int[0];
            }

            var temp = new bool[Size];
            for (var i = 0; i < Size; i++)
            {
                if (Matrix[row, i] == 0) continue;
                if (temp[Matrix[row, i] - 1]) return new int[0];

                temp[Matrix[row, i] - 1] = true;
            }

            return PossibleValues(temp);
        }

        public int[] SectionPossibleValues(int section)
        {
            if (Data != null)
            {
                return Data.SectionPossibleValues[section]?.ToArray() ?? new int[0];
            }

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

            var rowPossibleValues = RowPossibleValues(col);
            var columnPossibleValues = ColumnPossibleValues(row);

            var section = GetSection(row, col);
            var sectionPossibleValues = SectionPossibleValues(section);

            var possibleValues = rowPossibleValues.Intersect(columnPossibleValues).Intersect(sectionPossibleValues).ToArray();
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

            bool Process(Sudoku sudoku)
            {
                var again = false;

                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        if (sudoku[i, j] > 0) continue;

                        var values = sudoku.PossibleValues(i, j);
                        if (values == null || values.Length == 0) return false;
                        if (values.Length > 1) continue;

                        sudoku[i, j] = values[0];
                        again = true;
                    }
                }

                if (again) return Process(sudoku);
                return true;
            }

            var valid = Process(sudoku);
            return valid ? sudoku : null;
        }
    }
}
