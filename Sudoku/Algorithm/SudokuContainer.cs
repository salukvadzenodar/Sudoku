namespace Sudoku.Algorithm
{
    public class SudokuContainer
    {
        public Sudoku Sudoku { get; set; }

        public Sudoku Next { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public int[] PosibleValues { get; set; }

        public int Index { get; set; }
    }
}
