using System.Collections.Generic;

namespace Sudoku.Algorithm
{
    public class AuxilaryData
    {
        public List<int>[] RowPossibleValues { get; set; }

        public List<int>[] ColumnPossibleValues { get; set; }

        public List<int>[] SectionPossibleValues { get; set; }

        public bool Solved { get; set; }

        public AuxilaryData(int size)
        {
            RowPossibleValues = new List<int>[size];
            ColumnPossibleValues = new List<int>[size];
            SectionPossibleValues = new List<int>[size];
        }

        public void CheckSolved()
        {
            Solved = true;
            foreach (var item in RowPossibleValues)
            {
                if (item?.Count > 0)
                {
                    Solved = false;
                    break;
                }
            }
        }

        public void Remove(int rowIdx, int colIdx, int sectionIdx, int value)
        {
            RowPossibleValues[rowIdx].Remove(value);
            ColumnPossibleValues[colIdx].Remove(value);
            SectionPossibleValues[sectionIdx].Remove(value);

            CheckSolved();
        }

        public AuxilaryData Clone()
        {
            var data = new AuxilaryData(RowPossibleValues.Length) { Solved = Solved };

            for (var i = 0; i < RowPossibleValues.Length; i++)
            {
                if (RowPossibleValues[i] == null) continue;
                data.RowPossibleValues[i] = new List<int>(RowPossibleValues[i]);
            }

            for (var i = 0; i < ColumnPossibleValues.Length; i++)
            {
                if (ColumnPossibleValues[i] == null) continue;
                data.ColumnPossibleValues[i] = new List<int>(ColumnPossibleValues[i]);
            }

            for (var i = 0; i < SectionPossibleValues.Length; i++)
            {
                if (SectionPossibleValues[i] == null) continue;
                data.SectionPossibleValues[i] = new List<int>(SectionPossibleValues[i]);
            }

            return data;
        }
    }
}
