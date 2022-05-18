using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;

namespace Sudoku.Algorithm
{
    public static class SudokuSolve
    {
        private const string Error_Message = "Invalid Sudoku";

        private static SudokuContainer NewContainer(Sudoku sudoku)
        {
            var simplified = sudoku.Simplify();

            if (simplified == null) return null;
            if (simplified.Solved) return new SudokuContainer { Sudoku = simplified };

            var point = simplified.FindOptimalEmptyPoint();
            if (point == null) return null;

            var next = simplified.Clone();
            next[point.Value.row, point.Value.col] = point.Value.possibleValues[0];

            return new SudokuContainer
            {
                Sudoku = simplified,
                Next = next,
                Row = point.Value.row,
                Column = point.Value.col,
                PosibleValues = point.Value.possibleValues,
                Index = 0
            };
        }

        private static SudokuContainer NextContainer(SudokuContainer container)
        {
            container.Index++;
            while (container.Index < container.PosibleValues.Length)
            {
                var next = container.Sudoku.Clone();
                next[container.Row, container.Column] = container.PosibleValues[container.Index];

                var nextContainer = NewContainer(next);
                if (nextContainer != null) return nextContainer;
                container.Index++;
            }

            return null;
        }

        private static bool Solve(Stack<SudokuContainer> stack, Action<Sudoku, int, bool> notify)
        {
            var item = stack.Peek();

            var container = NewContainer(item.Next);
            while (container == null)
            {
                if (stack.Count == 0) return false;

                item = stack.Pop();
                container = NextContainer(item);

                if(container == null && notify != null)
                {
                    notify(stack.Peek().Next, stack.Count, true);
                }
            }

            stack.Push(container);
            if (notify != null)
            {
                notify(container.Next ?? container.Sudoku, stack.Count, false);
            }

            return container.Next == null;
        }

        public static Sudoku Solve(Sudoku sudoku, CancellationToken token, Action<Sudoku, int, bool> notify = null)
        {
            var container = NewContainer(sudoku);
            if (container == null) throw new ValidationException(Error_Message);
            if (container.Next == null) return container.Sudoku;

            var stack = new Stack<SudokuContainer>();
            stack.Push(container);

            do
            {
                if (token.IsCancellationRequested) return null;
                if (stack.Count == 0) throw new ValidationException(Error_Message);
            }
            while (!Solve(stack, notify));

            var item = stack.Peek();
            return item.Sudoku.Solved ? item.Sudoku : throw new ValidationException(Error_Message);
        }
    }
}
