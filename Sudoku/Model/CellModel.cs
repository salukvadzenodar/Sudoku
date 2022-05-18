using System;
using System.Windows.Controls;

namespace Sudoku.Model
{
    public class CellModel
    {
        public CellModel(TextBox control)
        {
            Control = control;
        }

        public TextBox Control { get; private set; }

        public int? Value
        {
            get
            {
                if (string.IsNullOrEmpty(Control.Text)) return null;
                return Convert.ToInt32(Control.Text);
            }

            set
            {
                Control.Text = value?.ToString() ?? "";
            }
        }

        public static implicit operator CellModel(TextBox control)
        {
            return new CellModel(control);
        }

        public static implicit operator TextBox(CellModel model)
        {
            return model.Control;
        }
    }
}
