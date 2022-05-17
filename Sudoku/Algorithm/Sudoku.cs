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
            bool Process(Sudoku sudoku)
            {
                var again = false;
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        if (sudoku[i, j] > 0) continue;

                        var possibleValues = PossibleValues(i, j);
                        if (possibleValues.Length == 0) return false;
                        if (possibleValues.Length > 1) continue;

                        sudoku[i, j] = possibleValues[0];
                        again = true;
                    }
                }

                if(again) return Process(sudoku);
                return true;
            }

            var sudoku = Clone();
            var valid = Process(sudoku);

            return valid ? sudoku : null;
        }
    }
}
