using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sudoku
{
    public partial class ParametersDialog : Window
    {
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public ParametersDialog(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            InitializeComponent();

            txtRow.Text = rows.ToString();
            txtCol.Text = cols.ToString();
        }

        private void Text_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            if (regex.IsMatch(e.Text))
            {
                e.Handled = true;
                return;
            }

            var textBox = sender as TextBox;
            var text = textBox.Text.Substring(0, textBox.SelectionStart) + e.Text + textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);

            if (text.StartsWith("0"))
            {
                e.Handled = true;
                return;
            }

            var value = Convert.ToInt32(text);
            if (value < 2 || value > 5)
            {
                e.Handled = true;
            }
        }

        private void btnSuccess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rows = Convert.ToInt32(txtRow.Text);
                var cols = Convert.ToInt32(txtCol.Text);

                if (rows < 2 || rows > 5 || cols < 2 || cols > 5) return;

                Rows = rows;
                Cols = cols;
                DialogResult = true;
            }
            catch { }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
