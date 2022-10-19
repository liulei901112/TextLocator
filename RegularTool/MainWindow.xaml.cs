using RegularTool.Model;
using RegularTool.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace RegularTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            txtContent.SelectionStart = 0;
            txtContent.SelectAll();
            txtContent.SelectionBackColor = System.Drawing.Color.White;
            if (txtSearch.Text == "")
            {

                return;
            }

            int startIndex = 0;
            while (startIndex < txtContent.TextLength)
            {
                int wordStartIndex = txtContent.Find(txtSearch.Text, startIndex, System.Windows.Forms.RichTextBoxFinds.None);
                if (wordStartIndex != -1)
                {
                    txtContent.SelectionStart = wordStartIndex;
                    txtContent.SelectionLength = txtSearch.Text.Length;
                    txtContent.SelectionBackColor = System.Drawing.Color.Yellow;
                }
                else
                    break;
                startIndex += wordStartIndex + txtSearch.Text.Length;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var contentModel = e.NewValue as GrammarModel;
            if (contentModel != null && !contentModel.IsGrouping)
            {
                MainVm.Instance.SelectGrammar = contentModel;
                tabOption.SelectedIndex = 0;
            }
        }

        private void BtnMatch_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            int count = 0;
            tabOption.SelectedIndex = 2;
            if (string.IsNullOrWhiteSpace(txtRegular.Text) || string.IsNullOrWhiteSpace(txtContent.Text))
            {
                dataResult.ItemsSource = dataTable.DefaultView;
                statusMatchCount.Content = 0;
                statusMatchSubCount.Content = 0;
                return;
            }
            Regex regex = new Regex(txtRegular.Text, rbMulti.IsChecked == true ? RegexOptions.Multiline : rbSingle.IsChecked == true ? RegexOptions.Singleline : RegexOptions.IgnoreCase);

            var result = regex.Matches(txtContent.Text);
            if (result.Count == 0)
            {

                dataResult.ItemsSource = dataTable.DefaultView;
                statusMatchCount.Content = 0;
                statusMatchSubCount.Content = 0;
                return;
            }
            List<List<string>> matchResults = new List<List<string>>();
            foreach (var item in result)
            {
                var match = item as Match;
                List<string> matchGroups = new List<string>();
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    matchGroups.Add(match.Groups[i].Value);
                }
                matchResults.Add(matchGroups);
            }
            dataTable.Columns.Add("命中序号");
            dataTable.Columns.Add("命中内容");
            count = matchResults.FirstOrDefault()?.Count ?? 0;
            if (count > 0)
            {
                for (int i = 1; i < count; i++)
                {
                    dataTable.Columns.Add("子表达式" + i);
                }
            }
            for (int i = 0; i < matchResults.Count; i++)
            {
                var row = dataTable.NewRow();

                for (int j = 0; j < matchResults[i].Count; j++)
                {
                    if (j == 0)
                    {
                        row["命中序号"] = i + 1;
                        row["命中内容"] = matchResults[i][j];
                    }
                    else
                    {
                        row["子表达式" + j] = matchResults[i][j];
                    }
                }
                dataTable.Rows.Add(row);

            }
            dataResult.ItemsSource = dataTable.DefaultView;
            statusMatchCount.Content = matchResults.Count;
            statusMatchSubCount.Content = count - 1;
        }

        private void BtnReplacce_Click(object sender, RoutedEventArgs e)
        {
            tabOption.SelectedIndex = 3;

            if (string.IsNullOrWhiteSpace(txtRegular.Text) || string.IsNullOrWhiteSpace(txtContent.Text))
            {
                txtReplaceResult.Text = "";
                return;
            }
            Regex regex = new Regex(txtRegular.Text, rbMulti.IsChecked == true ? RegexOptions.Multiline : rbSingle.IsChecked == true ? RegexOptions.Singleline : RegexOptions.IgnoreCase);

            var isMatch = regex.IsMatch(txtContent.Text);
            if (isMatch)
            {
                txtReplaceResult.Text = regex.Replace(txtContent.Text, txtReplace.Text);
            }
            else
            {
                txtReplaceResult.Text = "";
            }
        }

        private void BtnGenerateCode_Click(object sender, RoutedEventArgs e)
        {
            tabOption.SelectedIndex = 4;

            var code = "//matchResults为匹配到的结果,inputText为要匹配的内容";
            code += "\r\nstring inputText = \"\";";
            code += "\r\nRegex regex = new Regex(@\"" + txtRegular.Text + "\", " + (rbMulti.IsChecked == true ? "RegexOptions.Multiline" : rbSingle.IsChecked == true ? "RegexOptions.Singleline" : "RegexOptions.IgnoreCase") + ");";
            code += "\r\nvar result = regex.Matches(inputText);";
            code += "\r\nList<List<string>> matchResults = new List<List<string>>();";
            code += "\r\nforeach (var item in result)" +
            "\r\n{" +
            "\r\n   var match = item as Match;" +
            "\r\n   List<string> matchGroups = new List<string>();" +
            "\r\n   for (int i = 0; i < match.Groups.Count; i++)" +
            "\r\n   {" +
            "\r\n       matchGroups.Add(match.Groups[i].Value);" +
            "\r\n   }" +
            "\r\n   matchResults.Add(matchGroups);" +
            "\r\n}";
            //txtCodeEdit.Text = code;
        }
    }
}
