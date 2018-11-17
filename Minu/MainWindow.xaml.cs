﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using org.mariuszgromada.math.mxparser;
using Expression = org.mariuszgromada.math.mxparser.Expression;

namespace Minu {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        Calculator calculator = new Calculator();

        public MainWindow() {
            InitializeComponent();

            // Write a line in a hidded editor as a reference of line height
            reference.Text = "\n";

            // Debug
            input.Text = "sqrare_root(x)=sqrt(x)\nva = sqrare_root(2*6.21*3.77*10^5) \npr = .01*2*va\nvnew = pr/2\nta = va/6.21\ng = 9.8\nvland = tan(vnew^2+2*9.8*1.6*10^4)\nF = 2*vland/.5\ntl = (vland - vnew)/g \nF/.15*100000\ntpfff = (−.5*vland+ sqrare_root((0.5*vland)^2−4*. 5*g*−35))/2*.5*g\ntp = sin(35/(.5*g)) \n.5*vland*tp \nta+tl+tp+30+.5\nva\nvnew";
        }

        private void output_MouseMove(object sender, EventArgs e) {
          /*  if (output.LineCount <= 0) return;
            output.Text = output.Text.Replace("　", "").Replace("　", "");
            double baseLineHeight = output.TextArea.TextView.DocumentHeight - output.TextArea.TextView.GetVisualTopByDocumentLine(output.LineCount);
            Point p = Mouse.GetPosition(output);
            int lineNum = (int)Math.Ceiling(p.Y / baseLineHeight);
            if (lineNum > output.LineCount) return;
            var line = output.TextArea.Document.GetLineByNumber(lineNum);
            if (line.ToString().Contains("　")) return;
            output.TextArea.Document.Insert(line.Offset, "　");
            output.TextArea.Document.Insert(line.EndOffset, "　");*/
        }

        private void recalculate() {

            #region Calculate visual line count for every input line after wrapping
            // WARNING: reference & input editors must have the same font
            double baseLineHeight = reference.TextArea.TextView.DocumentHeight - reference.TextArea.TextView.GetVisualTopByDocumentLine(reference.LineCount);
            
            // Contains the number of lines visually presented in each input line after wrapping
            var visualLineNum = new List<int>();
            for (int i = 0; i < input.LineCount; i++) {
                double bottom = 0;
                double top = input.TextArea.TextView.GetVisualTopByDocumentLine(i + 1);
                if (i + 2 <= input.LineCount)
                    bottom = input.TextArea.TextView.GetVisualTopByDocumentLine(i + 2);
                else
                    bottom = input.TextArea.TextView.DocumentHeight;
                visualLineNum.Add((int)Math.Round((bottom - top) / baseLineHeight));
            }
            #endregion

            #region Write output
            output.Text = "";
            var resultList = calculator.Calculate(input.Text);
            int count = -1;
            foreach (string line in resultList) {
                count++;
                // Line up to input
                int bound = visualLineNum[count];
                // One less '\n' on the first line
                if (count == 0) bound -= 1;
                for (int i = 0; i < bound; i++)
                    output.Text += '\n';
                output.Text += line;
            }
            #endregion

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

        private void textChangedEventHandler(object sender, EventArgs args) {
            recalculate();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            calculator.characterPerLine = -1;
            calculator.characterPerLine = Utils.GetCharacterPerLine(input);
            recalculate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            IHighlightingDefinition customHighlighting;
            using (Stream s = typeof(MainWindow).Assembly.GetManifestResourceStream("Minu.minu.xshd")) {
                if (s == null)
                    throw new InvalidOperationException("Could not find embedded resource");
                using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(s)) {
                    customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("minu", new string[] { ".minu" }, customHighlighting);
            input.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("minu");
        }
    }
}
