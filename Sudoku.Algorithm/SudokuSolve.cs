using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sudoku.Algorithm
{
    public static class SudokuSolve
    {
        private static object lock_obj = new object();
        private static volatile bool aquired = false;

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

        private static bool Solve(Stack<SudokuContainer> stack, Action<SudokuProgress> notify, long notifyTime, ref DateTime time)
        {
            var item = stack.Peek();

            var container = NewContainer(item.Next);
            while (container == null)
            {
                if (stack.Count == 0) return false;

                item = stack.Pop();
                container = NextContainer(item);

                if (container == null && notify != null && (notifyTime == 0 || DateTime.Now.Subtract(time).TotalMilliseconds > notifyTime))
                {
                    time = DateTime.Now;
                    notify(new SudokuProgress
                    {
                        Sudoku = stack.Peek().Next,
                        Count = stack.Count,
                        IsBack = true
                    });
                }
            }

            stack.Push(container);
            if (notify != null && (notifyTime == 0 || DateTime.Now.Subtract(time).TotalMilliseconds > notifyTime))
            {
                time = DateTime.Now;
                notify(new SudokuProgress
                {
                    Sudoku = container.Next ?? container.Sudoku,
                    Count = stack.Count,
                    IsBack = false
                });
            }

            return container.Next == null;
        }

        public static Sudoku Solve(Sudoku sudoku, CancellationToken token, Action<SudokuProgress> notify = null, long notifyTime = 1000)
        {
            var container = NewContainer(sudoku);
            if (container == null) return null;
            if (container.Next == null) return container.Sudoku;

            var time = DateTime.Now;
            var stack = new Stack<SudokuContainer>();
            stack.Push(container);

            do
            {
                if (token.IsCancellationRequested || stack.Count == 0) return null;
            }
            while (!Solve(stack, notify, notifyTime, ref time));

            var item = stack.Peek();
            return item.Sudoku.Solved ? item.Sudoku : null;
        }

        public static async Task<Sudoku> SolveAsync(Sudoku sudoku, CancellationTokenSource tokenSource, Action<SudokuProgress> notify = null, long notifyTime = 1000)
        {
            var concurentThreads = Process.GetCurrentProcess().Threads.Count / 2;
            var tasks = new List<Task<Sudoku>>();

            var timer = new System.Timers.Timer(notifyTime + 100);

            SudokuProgress progress = null;
            aquired = true;

            Action<SudokuProgress> notifyFn = notify != null ? (p) =>
            {

                if (aquired) return;
                lock (lock_obj)
                {
                    if (aquired) return;
                    aquired = true;
                }

                progress = p;
            } : null;

            sudoku = sudoku.Simplify();
            if (sudoku == null) return null;

            if (concurentThreads <= 2)
            {
                tasks.Add(Task.Factory.StartNew((_) => Solve(sudoku, tokenSource.Token, notifyFn, notifyTime), TaskCreationOptions.LongRunning, tokenSource.Token));
            }
            else
            {
                var miscellaneous = new List<Sudoku>() { sudoku };
                while (miscellaneous.Count > 0 && concurentThreads - 2 > miscellaneous.Count)
                {
                    var top = miscellaneous[0];
                    miscellaneous.RemoveAt(0);

                    var point = top.FindOptimalEmptyPoint();
                    if (point == null) continue;

                    foreach(var value in point?.possibleValues){
                        var clone = top.Clone();
                        clone[point.Value.row, point.Value.col] = value;

                        clone = clone.Simplify();
                        if (clone == null) continue;

                        miscellaneous.Add(clone);
                    }
                }

                miscellaneous.ForEach(s =>
                {
                    tasks.Add(Task.Factory.StartNew((_) => Solve(s, tokenSource.Token, notifyFn, notifyTime), TaskCreationOptions.LongRunning, tokenSource.Token));
                });
            }

            if(notify != null)
            {
                aquired = false;
                timer.Elapsed += (o, e)=>
                {
                    if(tokenSource.Token.IsCancellationRequested)
                    {
                        timer.Enabled = false;
                        return;
                    }
                    if (progress == null) return;

                    notify(progress);
                    progress = null;
                    aquired = false;
                };

                timer.Enabled = true;
            }

            while (tasks.Any())
            {
                if (tokenSource.Token.IsCancellationRequested) return null;

                var finished = await Task.WhenAny(tasks);
                tasks.Remove(finished);

                var solved = await finished;
                if (solved == null) continue;

                tokenSource.Cancel();
                return solved;
            }

            tokenSource.Cancel();
            return null;
        }
    }
}
