using System.Linq;

namespace Sudoku.Algorithm
{
    public class Sudoku
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public int Size => Rows * Cols;
        public Status Status { get; private set; }
        private int[,] Matrix { get; set; }

        public Sudoku(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Matrix = new int[Size, Size];
        }

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

        public Sudoku Clone()
        {
            var sudoku = new Sudoku(Rows, Cols);
            sudoku.Status = Status;

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

        public int[] ColumnItemsLeft(int row)
        {
            var temp = new bool[Size];
            var count = 0;

            for (var i = 0; i < Size; i++)
            {
                if (Matrix[row, i] == 0) continue;
                if (temp[Matrix[row, i] - 1]) return null;

                count++;
                temp[Matrix[row, i] - 1] = true;
            }

            var itemsLeft = new int[Size - count];
            var index = 0;

            for (var i = 0; i < Size; i++)
            {
                if (temp[i]) continue;
                itemsLeft[index] = i + 1;
                index++;
            }

            return itemsLeft;
        }

        public int[] RowItemsLeft(int column)
        {
            var temp = new bool[Size];
            var count = 0;

            for (var i = 0; i < Size; i++)
            {
                if (Matrix[i, column] == 0) continue;
                if (temp[Matrix[i, column] - 1]) return null;

                count++;
                temp[Matrix[i, column] - 1] = true;
            }

            var leftItems = new int[Size - count];
            var index = 0;

            for (var i = 0; i < Size; i++)
            {
                if (temp[i]) continue;
                leftItems[index] = i + 1;
                index++;
            }

            return leftItems;
        }

        public int[] SectionItemsLeft(int section)
        {
            var temp = new bool[Size];
            var count = 0;
            var (rowStart, colStart) = GetSectionStart(section);

            for (var i = rowStart; i < rowStart + Rows; i++)
                for (var j = colStart; j < colStart + Cols; j++)
                {
                    if (Matrix[i, j] == 0) continue;
                    if (temp[Matrix[i, j] - 1]) return null;

                    count++;
                    temp[Matrix[i, j] - 1] = true;
                }

            var leftItems = new int[Size - count];
            var index = 0;

            for (var i = 0; i < Size; i++)
            {
                if (temp[i]) continue;

                leftItems[index] = i + 1;
                index++;
            }
            return leftItems;
        }

        public (int row, int col)? FirstEmptyPoint()
        {
            for (var i = 0; i < Size; i++)
                for (var j = 0; j < Size; j++)
                    if (this[i, j] == 0) return (i, j);

            return null;
        }

        public (int row, int col)? FindOptimalEmptyPoint()
        {
            int index = 0, value = Size, type = 0;

            for (int i = 0; i < Size; i++)
            {
                var itemsLeft = SectionItemsLeft(i);
                if (itemsLeft.Length == 0) continue;

                if (itemsLeft.Length == 2)
                {
                    type = 1;
                    index = i;
                    value = 2;
                    break;
                }
                else if (itemsLeft.Length < value)
                {
                    type = 1;
                    index = i;
                    value = itemsLeft.Length;
                }
            }

            if (value > 2)
            {
                for (int i = 0; i < Size; i++)
                {
                    var columnsLeft = ColumnItemsLeft(i);
                    if (columnsLeft.Length != 0)
                    {
                        if (columnsLeft.Length == 2)
                        {
                            type = 2;
                            index = i;
                            value = 2;
                            break;
                        }
                        else if (columnsLeft.Length < value)
                        {
                            type = 2;
                            index = i;
                            value = columnsLeft.Length;
                        }
                    }

                    var rowsLeft = RowItemsLeft(i);
                    if (rowsLeft.Length != 0)
                    {
                        if (rowsLeft.Length == 2)
                        {
                            type = 3;
                            index = i;
                            value = 2;
                            break;
                        }
                        else if (rowsLeft.Length < value)
                        {
                            type = 3;
                            index = i;
                            value = rowsLeft.Length;
                        }
                    }
                }
            }

            if (type == 1)
            {
                var (rowStart, colStart) = GetSectionStart(index);

                for (var i = rowStart; i < rowStart + Rows; i++)
                    for (var j = colStart; j < colStart + Cols; j++)
                        if (this[i, j] == 0) return (i, j);
            }
            else if (type == 2)
            {
                for (var i = 0; i < Size; i++)
                {
                    if (this[index, i] == 0) return (index, i);
                }
            }
            else if (type == 3)
            {
                for (var i = 0; i < Size; i++)
                {
                    if (this[i, index] == 0) return (i, index);
                }
            }

            return null;
        }

        private bool Solve(ref int[][] sections, ref int[][] rows, ref int[][] columns)
        {
            var success = false;

            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                {
                    if (Matrix[i, j] != 0) continue;
                    var s = GetSection(i, j);

                    var intersect = sections[s].Intersect(rows[j]).Intersect(columns[i]).ToArray();
                    var count = intersect.Length;

                    if (count == 0)
                    {
                        Status = Status.Fail;
                        return false;
                    }
                    if (count > 1) continue;

                    var value = intersect.First();
                    Matrix[i, j] = value;
                    success = true;

                    sections[s] = sections[s].Where(x => x != value).ToArray();
                    rows[j] = rows[j].Where(x => x != value).ToArray();
                    columns[i] = columns[i].Where(x => x != value).ToArray();
                }

            return success;
        }

        public void Solve()
        {
            var sections = new int[Size][];
            var rows = new int[Size][];
            var columns = new int[Size][];

            for (int i = 0; i < Size; i++)
            {
                sections[i] = SectionItemsLeft(i);
                if (sections[i] == null)
                {
                    Status = Status.Fail;
                    return;
                }

                rows[i] = RowItemsLeft(i);
                if (rows[i] == null)
                {
                    Status = Status.Fail;
                    return;
                }

                columns[i] = ColumnItemsLeft(i);
                if (columns[i] == null)
                {
                    Status = Status.Fail;
                    return;
                }
            }

            while (Solve(ref sections, ref rows, ref columns)) ;

            if (Status != Status.Fail)
            {
                for (var i = 0; i < Size; i++)
                    for (var j = 0; j < Size; j++)
                    {

                        if (Matrix[i, j] == 0)
                        {
                            Status = Status.InProgress;
                            return;
                        }
                    }
                Status = Status.Solved;
            }
        }
    }
}
