using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        private string lastExpression = "";
        private bool isError = false;
        private Stack<string> history = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();
            foreach (UIElement el in MainRoot.Children)
            {
                if (el is Button button)
                {
                    button.Click += ButtonClick;
                }
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var column = MainRoot.ColumnDefinitions[4];
            column.Width = column.Width == new GridLength(1, GridUnitType.Star) ? new GridLength(0, GridUnitType.Star) : new GridLength(1, GridUnitType.Star);
        }

        private async void ShowError()
        {
            Output.Text = "ERROR";
            isError = true;
            await Task.Delay(2000);
            Output.Text = "";
            isError = false;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (isError)
            {
                return;
            }
            string str = ((Button)sender).Content.ToString();
            if (str == "C")
            {
                Output.Text = "";
                lastExpression = "";
                history.Clear();
            }
            else if (str == "CE")
            {
                if (history.Count > 0)
                {
                    Output.Text = history.Pop();
                }
            }
            else if (str == "▷")
            {
                if (Output.Text.Length > 0)
                {
                    history.Push(Output.Text);
                    Output.Text = Output.Text[..^1];
                }
                else
                {
                    ShowError();
                }
            }
            else if (str == "=")
            {
                try
                {
                    lastExpression = Output.Text;
                    history.Push(Output.Text);
                    string expression = Output.Text.Replace(",", ".");

                    if (expression.Contains("/0"))
                    {
                        throw new DivideByZeroException();
                    }

                    Output.Text = new DataTable().Compute(expression, null).ToString();
                }
                catch (DivideByZeroException)
                {
                    ShowError();
                }
                catch
                {
                    ShowError();
                }
            }
            else if (str == "√")
            {
                try
                {
                    lastExpression = Output.Text;
                    history.Push(Output.Text);
                    double number = Convert.ToDouble(Output.Text);
                    if (number < 0)
                    {
                        ShowError();
                    }
                    else
                    {
                        Output.Text = Math.Sqrt(number).ToString();
                    }
                }
                catch
                {
                    ShowError();
                }
            }
            else if (str == "n²")
            {
                try
                {
                    lastExpression = Output.Text;
                    history.Push(Output.Text);
                    double number = Convert.ToDouble(Output.Text);
                    Output.Text = Math.Pow(number, 2).ToString();
                }
                catch
                {
                    ShowError();
                }
            }
            else if (str == "ln")
            {
                try
                {
                    lastExpression = Output.Text;
                    history.Push(Output.Text);
                    double number = Convert.ToDouble(Output.Text);
                    if (number > 0)
                    {
                        Output.Text = Math.Log(number).ToString();
                    }
                    else
                    {
                        ShowError();
                    }
                }
                catch
                {
                    ShowError();
                }
            }
            else if (str == "Pi")
            {
                history.Push(Output.Text);
                if (Output.Text != "")
                {
                    Output.Text += "*";
                    Output.Text += Math.PI.ToString();
                }
                else
                {
                    Output.Text += Math.PI.ToString();
                }
            }
            else if (str == "e")
            {
                history.Push(Output.Text);
                if (Output.Text != "")
                {
                    Output.Text += "*";
                    Output.Text += Math.E.ToString();
                }
                else
                {
                    Output.Text += Math.E.ToString();
                }
            }
            else if (str == ",")
            {
                if (!Output.Text.Contains(",") && Output.Text != "")
                {
                    Output.Text += str;
                }
            }
            else if (str == "+")
            {
                if (!Output.Text.EndsWith("+") && Output.Text != "")
                {
                    Output.Text += str;
                }
            }
            else if (str == "/")
            {
                if (!Output.Text.EndsWith("/") && Output.Text != "")
                {
                    Output.Text += str;
                }
            }
            else if (str == "*")
            {
                if (!Output.Text.EndsWith("*") && Output.Text != "")
                {
                    Output.Text += str;
                }
            }
            else if (str == "-")
            {
                if (!Output.Text.EndsWith("-"))
                {
                    Output.Text += str;
                }
            }
            else if (str == "0")
            {
                if (Output.Text != "0" && Output.Text != "-0")
                {
                    Output.Text += str;
                }
            }
            else if (str == "00")
            {
                if (Output.Text != "0" && Output.Text != "" && Output.Text != "-" && Output.Text != "-0")
                {
                    Output.Text += str;
                }
            }
            else if (str == "≡")
            {
                return;
            }
            else
            {
                history.Push(Output.Text);
                if (Output.Text == "0")
                {
                    Output.Text = str;
                }
                else
                {
                    Output.Text += str;
                }
            }
        }
    }
}
