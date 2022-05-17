using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Sudoku.Test
{
    [TestClass]
    public class SudokuTest
    {
        private void TestSections(int rows, int cols, params (int row, int col)[] points)
        {
            var size = rows * cols;
            var sudoku = new Algorithm.Sudoku(rows, cols);

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var section = sudoku.GetSection(i, j);

                    var point = points[section];
                    if (i < point.row || i > point.row + rows ||
                        j < point.col || j > point.col + cols)
                    {
                        Assert.ThrowsException<Exception>(() => new Exception($"Invalid Section - {section} at Point {i}, {j}"));
                    }
                }
            }

            for (var i = 0; i < size; i++)
            {
                var (rowStart, colStart) = sudoku.GetSectionStart(i);
                Assert.AreEqual(rowStart, points[i].row);
                Assert.AreEqual(colStart, points[i].col);
            }
        }

        [TestMethod]
        public void Section2x2()
        {
            TestSections(2, 2,
                (0, 0), (0, 2),
                (2, 0), (2, 2));
        }

        [TestMethod]
        public void Section2x3()
        {
            TestSections(2, 3,
                (0, 0), (0, 3),
                (2, 0), (2, 3),
                (4, 0), (4, 3));
        }

        [TestMethod]
        public void Section3x2()
        {
            TestSections(3, 2,
                (0, 0), (0, 2), (0, 4),
                (3, 0), (3, 2), (3, 4));
        }

        [TestMethod]
        public void Section3x3()
        {
            TestSections(3, 3,
                (0, 0), (0, 3), (0, 6),
                (3, 0), (3, 3), (3, 6),
                (6, 0), (6, 3), (6, 6));
        }
    }
}
