using System.Windows.Media;

namespace Sudoku.Model
{
    public class MainViewModel : SudokuModel
    {
        private bool startEnabled = true;
        private bool validateEnabled = true;
        private bool solveEnabled = true;
        private bool clearEnabled = true;
        private bool paramsEnabled = true;

        private string message;
        private Brush messageColor;
        private string startContent = Defaults.Start_Content;
        private MainViewState state = MainViewState.Initial;

        public bool StartEnabled
        {
            get => startEnabled;
            set
            {
                startEnabled = value;
                NotifyChange();
            }
        }

        public bool ValidateEnabled
        {
            get => validateEnabled;
            set
            {
                validateEnabled = value;
                NotifyChange();
            }
        }

        public bool SolveEnabled
        {
            get => solveEnabled;
            set
            {
                solveEnabled = value;
                NotifyChange();
            }
        }

        public bool ClearEnabled
        {
            get => clearEnabled;
            set
            {
                clearEnabled = value;
                NotifyChange();
            }
        }

        public bool ParamsEnabled
        {
            get => paramsEnabled;
            set
            {
                paramsEnabled = value;
                NotifyChange();
            }
        }

        public string Message
        {
            get => message;
            set
            {
                message = value;
                NotifyChange();
            }
        }

        public Brush MessageColor
        {
            get => messageColor;
            set
            {
                messageColor = value ?? Defaults.Text_Color;
                NotifyChange();
            }
        }

        public string StartContent
        {
            get => startContent;
            set
            {
                startContent = value;
                NotifyChange();
            }
        }

        public MainViewState State
        {
            get => state;
            set
            {
                var wasSolving = state == MainViewState.Solving;

                state = value;
                void SetDefaultState(bool state)
                {
                    StartEnabled = state;
                    ValidateEnabled = state;
                    SolveEnabled = state;
                    ClearEnabled = state;
                    ParamsEnabled = state;

                    Message = "";
                    startContent = Defaults.Start_Content;
                }

                if (state == MainViewState.Initial)
                {
                    SetDefaultState(true);
                    SetColor(Defaults.Text_Color);
                    SetEnabled(true);
                }
                else if (state == MainViewState.Started)
                {
                    SetDefaultState(true);
                    SetColor(Defaults.Text_Success_Color, true);
                    SetEnabled(false, true);

                    StartContent = Defaults.Stop_Content;
                }
                else if (state == MainViewState.Stopped)
                {
                    SetDefaultState(true);
                    if(!wasSolving) SetColor(Defaults.Text_Color);
                    SetEnabled(true);

                    StartContent = Defaults.Start_Content;
                }
                else if (state == MainViewState.Solving)
                {
                    SetDefaultState(false);
                    SetColor(Defaults.Text_Success_Color, true);
                    SetEnabled(false);

                    StartEnabled = true;
                    StartContent = Defaults.Stop_Content;
                }
            }
        }

        public MainViewModel(int rows, int cols) : base(rows, cols) { }

        public void SetText(string text, Brush? color = null)
        {
            Message = text;
            MessageColor = color;
        }

        public bool Validate()
        {
            SetColor(Defaults.Text_Color);
            return Validate(Defaults.Text_Danger_Color);
        }
    }
}
