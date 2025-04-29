using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GoogleGiminiDemo
{
    /// <summary>
    /// ResultBrowser.xaml 的互動邏輯
    /// </summary>
    public partial class ResultBrowser : Window
    {
        public ResultBrowser(ParseResult response)
        {
            InitializeComponent();
            lbTitle.Content = "Title: " + response.title;
            tbSummary.Text = "Summary: " + response.image_content;
            mdxamViewer.Markdown = response.text;
            int i = (int)response.image_type + 1;
            if (typePanel.Children[i] is Label label)
            {
                label.Foreground = Brushes.Black;
                label.FontWeight = FontWeights.Bold;
            }
        }
    }
}
