﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Minu {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        double baseLineHeight = 28;

        Calculator calculator = new Calculator();
        MouseSelectionHelper selectionHelper;

        public MainWindow() {
            InitializeComponent();
        }
        
        private void output_MouseMove(object sender, EventArgs e) {
            selectionHelper.MouseMove(e);
        }

        private void editor_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (!e.Handled) {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void recalculate() {
            if (!input.TextArea.TextView.VisualLinesValid)
                input.TextArea.TextView.EnsureVisualLines();

            var resultList = calculator.Calculate(input.Text);

            var outputText = "";
            for (int i = 0; i < resultList.Count; ++i) {
                // Line up to input
                int bound = (int)(input.TextArea.TextView.VisualLines[i].Height / baseLineHeight);
                // One less '\n' on the first line
                if (i == 0) bound--;
                outputText += (bound > 0?(new string('\n', bound)):"") + resultList[i] + "\u2001";
            }
            output.Text = outputText;
            selectionHelper?.InvalidateCache();

            // Show the splitter if necessary
            bool outputOverflowed = (outputColumn.ActualWidth - output.ActualWidth - output.Margin.Left - output.Margin.Right) <= 10;
            if (outputOverflowed || calculator.isInputOverflowed)
                splitter.Visibility = Visibility.Visible;
            else
                splitter.Visibility = Visibility.Collapsed;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void RecalculateHandler(object sender, EventArgs e) {
            recalculate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Utils.LoadHighlightRule("minu.xshd", "minu");
            Utils.LoadHighlightRule("output.xshd", "output");
            input.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("minu");
            output.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("output");
            input.TextArea.TextView.Options.LineHeight = baseLineHeight;
            output.TextArea.TextView.Options.LineHeight = baseLineHeight;
            output.TextArea.TextView.Options.Alignment = TextAlignment.Right;

            selectionHelper = new MouseSelectionHelper(output);
            selectionHelper.OnClickEvent += (s, arg) => {
                Clipboard.SetText(arg.LineContent);
            };
            output.TextArea.SelectionChanged += (s, arg) =>
            {
                output.TextArea.ClearSelection();
                output.TextArea.Caret.Hide();
            };
            output.TextArea.MouseSelectionMode = ICSharpCode.AvalonEdit.Editing.MouseSelectionMode.None;

            // Debug
            input.Text = "sqrare_root(x)=sqrt(x)\nva = sqrare_root(2*6.21*3.77*10^5) \npr = .01*2*va\nvnew = pr/2\nta = va/6.21\ng = 9.8\nvland = tan(vnew^2+2*9.8*1.6*10^4)\nF = 2*vland/.5\ntl = (vland - vnew)/g \nF/.15*100000\ntpfff = (−.5*vland+ sqrare_root((0.5*vland)^2−4*. 5*g*−35))/2*.5*g\ntp = sin(35/(.5*g)) \n.5*vland*tp \nta+tl+tp+30+.5\nva\nvnew";
        }
    }
}
