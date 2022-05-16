using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace Sudoku.Algorithm
{
    public static class SudokuSolve
    {
        private static bool Solve(Stack<SudokuContainer> stack, CancellationToken token, Action<Sudoku> notify)
        {
            var item = stack.Peek();
            if (item.Sudoku.Status == Status.Solved) return true;

            if (item.Simplified == null)
            {
                var point = item.Sudoku.FindOptimalEmptyPoint();
                item.Row = point.Value.row;
                item.Column = point.Value.col;

                var sectionsLeft = item.Sudoku.SectionItemsLeft(item.Sudoku.GetSection(item.Row, item.Column));
                var rowsLeft = item.Sudoku.RowItemsLeft(item.Column);
                var columnsLeft = item.Sudoku.ColumnItemsLeft(item.Row);

                item.PosibleValues = sectionsLeft.Intersect(rowsLeft).Intersect(columnsLeft).ToArray();
                var simplified = item.Sudoku.Clone();
                simplified[item.Row, item.Column] = item.PosibleValues[0];

                simplified.Solve();
                item.Simplified = simplified;
            }

            if (item.Simplified.Status == Status.Solved) return true;
            if (item.Simplified.Status == Status.InProgress)
            {
                if (notify != null) notify(item.Simplified);
                stack.Push(new SudokuContainer
                {
                    Sudoku = item.Simplified.Clone()
                });

                return false;
            }

            if (token.IsCancellationRequested) return false;
            while (item.Index == item.PosibleValues.Length - 1)
            {
                if (token.IsCancellationRequested) return false;

                var peek = stack.Peek();
                item = (item == peek) ? stack.Pop() : peek;
            }

            item.Index++;
            item.Sudoku[item.Row, item.Column] = item.PosibleValues[item.Index];

            var reSimplified = item.Sudoku.Clone();
            reSimplified.Solve();

            item.Simplified = reSimplified;
            if (notify != null) notify(item.Simplified);

            return false;
        }

        public static Sudoku Solve(Sudoku sudoku, CancellationToken token, Action<Sudoku> notify = null)
        {
            var stack = new Stack<SudokuContainer>();
            stack.Push(new SudokuContainer
            {
                Sudoku = sudoku
            });

            do
            {
                if (token.IsCancellationRequested) return null;
                if (stack.Count == 0) throw new ValidationException("Invalid Sudoku");
            }
            while (!Solve(stack, token,  notify));

            var item = stack.Peek();
            return item.Simplified.Status == Status.Solved ? item.Simplified : item.Sudoku;
        }
    }
}
