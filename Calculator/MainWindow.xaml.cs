using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = false;
        private Stack<string> historyStack = new Stack<string>();
        private Stack<string> redoStack = new Stack<string>();
        private Dictionary<string, CommandBase> commands = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializeCommands();

            foreach (UIElement el in MainRoot.Children)
            {
                if (el is Button button)
                {
                    button.Click += Button_Click;
                }
            }

            menuButton.Click += MenuButton_Click;
        }

        private void InitializeCommands()
        {
            commands["C"] = new ClearCommand();
            commands["CE"] = new UndoCommand();
            commands["Redo"] = new RedoCommand();
            commands["⌫"] = new BackspaceCommand();
            commands["n²"] = new SquareCommand();
            commands["π"] = new PiCommand();
            commands["e"] = new ECommand();
            commands["ln"] = new LnCommand();
            commands["√"] = new SqrtCommand();
            commands["="] = new EvaluateCommand();
            commands["default"] = new AppendTextCommand();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == menuButton)
                return; 

            string content = ((Button)sender).Content.ToString();

            if (!commands.TryGetValue(content, out var command))
                command = commands["default"];

            command.Execute(this, content);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            menuColumn.Width = isMenuOpen ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            isMenuOpen = !isMenuOpen;
        }

        public void PushToHistory(string value) => historyStack.Push(value);
        public string PopFromHistory() => historyStack.Count > 0 ? historyStack.Pop() : "";
        public void ClearHistory() => historyStack.Clear();

        public void PushToRedo(string value) => redoStack.Push(value);
        public string PopFromRedo() => redoStack.Count > 0 ? redoStack.Pop() : "";
        public void ClearRedo() => redoStack.Clear();

        public string GetText() => Output.Text;
        public void SetText(string value) => Output.Text = value;

        public async void ShowError()
        {
            SetText("ERROR");
            await Task.Delay(2000);
            SetText("");
        }
    }

    public abstract class CommandBase
    {
        public abstract void Execute(MainWindow window, string parameter);
    }

    public class ClearCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            window.SetText("");
            window.ClearHistory();
            window.ClearRedo();
        }
    }

    public class UndoCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            window.PushToRedo(window.GetText());
            window.SetText(window.PopFromHistory());
        }
    }

    public class RedoCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string redo = window.PopFromRedo();
            if (!string.IsNullOrEmpty(redo))
            {
                window.PushToHistory(window.GetText());
                window.SetText(redo);
            }
            else
            {
                window.ShowError();
            }
        }
    }

    public class BackspaceCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            if (!string.IsNullOrEmpty(current))
            {
                window.PushToRedo(current);
                window.SetText(current[..^1]);
            }
        }
    }

    public class SquareCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            if (double.TryParse(current, out double number))
            {
                window.PushToHistory(current);
                window.ClearRedo();
                window.SetText(Math.Pow(number, 2).ToString());
            }
            else
            {
                window.ShowError();
            }
        }
    }

    public class PiCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);
            window.SetText(current + Math.PI.ToString());
        }
    }

    public class ECommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);
            window.SetText(current + Math.E.ToString());
        }
    }

    public class LnCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            if (double.TryParse(current, out double number) && number > 0)
            {
                window.PushToHistory(current);
                window.ClearRedo();
                window.SetText(Math.Log(number).ToString());
            }
            else
            {
                window.ShowError();
            }
        }
    }

    public class SqrtCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            if (double.TryParse(current, out double number) && number >= 0)
            {
                window.PushToHistory(current);
                window.ClearRedo();
                window.SetText(Math.Sqrt(number).ToString());
            }
            else
            {
                window.ShowError();
            }
        }
    }

    public class EvaluateCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string expr = window.GetText()
                .Replace("×", "*")
                .Replace(",", ".")
                .Replace("÷", "/");

            if (expr.Contains("/0"))
            {
                window.ShowError();
                return;
            }

            try
            {
                var result = new DataTable().Compute(expr, null).ToString();
                window.PushToHistory(window.GetText());
                window.ClearRedo();

                if (result.Length > 20)
                    window.SetText(Convert.ToDouble(result).ToString("E"));
                else
                    window.SetText(result);
            }
            catch
            {
                window.ShowError();
            }
        }
    }

    public class AppendTextCommand : CommandBase
    {
        public override void Execute(MainWindow window, string parameter)
        {
            string current = window.GetText();
            window.PushToHistory(current);

            string[] operators = { "+", "-", "×", "÷" };
            string lastNumber = current.Split(operators, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "";

            if (parameter == "0")
            {
                if (current == "0" || current == "-0")
                    return;
                window.SetText(current + "0");
            }
            else if (parameter == ",")
            {
                if (!lastNumber.Contains(",") && lastNumber != "")
                {
                    window.SetText(current + ",");
                }
            }
            else if ("×÷+-".Contains(parameter))
            {
                if (!string.IsNullOrEmpty(current) && !"+-×÷".Contains(current.Last()))
                {
                    window.SetText(current + parameter);
                }
            }
            else
            {
                if (current == "0")
                    window.SetText(parameter);
                else
                    window.SetText(current + parameter);
            }
        }
    }
}
