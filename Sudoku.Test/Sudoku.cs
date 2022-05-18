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

        [TestMethod]
        public void Section3x4()
        {
            TestSections(3, 4,
                (0, 0), (0, 4), (0, 8),
                (3, 0), (3, 4), (3, 8),
                (6, 0), (6, 4), (6, 8),
                (9, 0), (9, 4), (9, 8));
        }

        [TestMethod]
        public void Section4x3()
        {
            TestSections(4, 3,
                (0, 0), (0, 3), (0, 6), (0, 9),
                (4, 0), (4, 3), (4, 6), (4, 9),
                (8, 0), (8, 3), (8, 6), (8, 9));
        }

        [TestMethod]
        public void Section4x4()
        {
            TestSections(4, 4,
                (0, 0), (0, 4), (0, 8), (0, 12),
                (4, 0), (4, 4), (4, 8), (4, 12),
                (8, 0), (8, 4), (8, 8), (8, 12),
                (12, 0), (12, 4), (12, 8), (12, 12));
        }

        [TestMethod]
        public void Section4x5()
        {
            TestSections(4, 5,
                (0, 0), (0, 5), (0, 10), (0, 15),
                (4, 0), (4, 5), (4, 10), (4, 15),
                (8, 0), (8, 5), (8, 10), (8, 15),
                (12, 0), (12, 5), (12, 10), (12, 15),
                (16, 0), (16, 5), (16, 10), (16, 15));
        }

        [TestMethod]
        public void Section5x4()
        {
            TestSections(5, 4,
                (0, 0), (0, 4), (0, 8), (0, 12), (0, 16),
                (5, 0), (5, 4), (5, 8), (5, 12), (5, 16),
                (10, 0), (10, 4), (10, 8), (10, 12), (10, 16),
                (15, 0), (15, 4), (15, 8), (15, 12), (15, 16));
        }

        [TestMethod]
        public void Section5x5()
        {
            TestSections(5, 5,
                (0, 0), (0, 5), (0, 10), (0, 15), (0, 20),
                (5, 0), (5, 5), (5, 10), (5, 15), (5, 20),
                (10, 0), (10, 5), (10, 10), (10, 15), (10, 20),
                (15, 0), (15, 5), (15, 10), (15, 15), (15, 20),
                (20, 0), (20, 5), (20, 10), (20, 15), (20, 20));
        }
    }
}
