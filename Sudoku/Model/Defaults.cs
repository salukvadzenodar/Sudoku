using System.Windows.Media;

namespace Sudoku.Model
{
    public static class Defaults
    {
        public const int Row_Count = 3;
        public const int Cell_Count = 3;
        public const int Cell_Size = 50;

        public static readonly string Start_Content = "Start";
        public static readonly string Stop_Content = "Stop";
        public static readonly string Invalid_Text = "Invalid Numbers";
        public static readonly string Valid_Text = "Valid";

        public static readonly SolidColorBrush Main_Border_Color = new SolidColorBrush(Colors.Black) { Opacity = 0.8 };
        public static readonly SolidColorBrush Text_Color = new SolidColorBrush(Colors.Black) { Opacity = 0.8 };
        public static readonly SolidColorBrush Text_Success_Color = new SolidColorBrush(Colors.Green) { Opacity = 0.8 };
        public static readonly SolidColorBrush Text_Danger_Color = new SolidColorBrush(Colors.Red) { Opacity = 0.8 };
    }
}
