using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DataModel;
using TfsData;
using Xceed.Words.NET;
using Xceed.Wpf.Toolkit;

namespace Gui
{
    public partial class MainWindow
    {
        private ReleaseData _data;
        private const string RegexString = @".*\\\w+(?:.*)?\\((\w\d.\d+.\d+).\d+)";
        private static TfsConnector _tfs;
        public List<string> Categories => GettrimmedSettingList("categories");
        public MainWindow()
        {
            //DoStuff();

            InitializeComponent();

            //Close();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _data = new ReleaseData();
            DataContext = _data;
            ConnectTfsButton(null, null);
        }

        private void DoStuff(ReleaseData data)
        {

            var dTestDocx = @"D:\test.docx";
            string fileName = @"D:\Template.docx";

            using (var doc = DocX.Load(fileName))
            {
                //doc.ReplaceText(Tokens.ReleaseNumber, releaseName);

                var secondSection = doc.Paragraphs.FirstOrDefault(x => x.Text == "Code Change sets in this Release");
                var paragraph = secondSection.InsertParagraphAfterSelf("asd").FontSize(10d);
                InsertBeforeOrAfter placeholder = paragraph;
                foreach (var category in data.CategorizedChanges)
                {
                    var p = placeholder.InsertParagraphAfterSelf(category.Name).FontSize(11d).Heading(HeadingType.Heading2);

                    var table = p.InsertTableAfterSelf(2, 6);
                    table.Rows[0].Cells[0].Paragraphs[0].Append("TFS").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("Developer").Bold();
                    table.Rows[0].Cells[2].Paragraphs[0].Append("Date/Time").Bold();
                    table.Rows[0].Cells[3].Paragraphs[0].Append("Description").Bold();
                    table.Rows[0].Cells[4].Paragraphs[0].Append("Work Item").Bold();
                    table.Rows[0].Cells[5].Paragraphs[0].Append("Work Item Description").Bold();
                    table.Rows[1].Cells[0].Paragraphs[0].Append("{TfsID}");
                    table.Rows[1].Cells[1].Paragraphs[0].Append("{Dev}");
                    table.Rows[1].Cells[2].Paragraphs[0].Append("{Date}");
                    table.Rows[1].Cells[3].Paragraphs[0].Append("{Desc}");
                    table.Rows[1].Cells[4].Paragraphs[0].Append("{WorkItemId}");
                    table.Rows[1].Cells[5].Paragraphs[0].Append("{WorkItemTitle}");

                    var rowPattern = table.Rows[1];
                    foreach (var change in category.Changes)
                    {
                        var newItem = table.InsertRow(rowPattern, table.RowCount - 1);

                        newItem.ReplaceText("{TfsID}", change.Id.ToString());
                        newItem.ReplaceText("{Dev}", change.CommitedBy);
                        newItem.ReplaceText("{Date}", change.Created.ToString());
                        newItem.ReplaceText("{Desc}", change.Comment);
                        newItem.ReplaceText("{WorkItemId}", change.WorkItemId.ToString());
                        newItem.ReplaceText("{WorkItemTitle}", change.WorkItemTitle.ToString());

                    }





                    rowPattern.Remove();
                    placeholder = table;
                }

                var thirdSection = placeholder.CreateHeadingSection("Product reported Defects in this Release");
                var fourthSection = thirdSection.CreateHeadingSection("Product Backlog Items and KTRs in this Release");
                var fifthSection = fourthSection.CreateHeadingSection("Test Report");
                var sixthSection = fifthSection.CreateHeadingSection("Known issues in this Release");

                doc.SaveAs(dTestDocx);
            }

            Process.Start(dTestDocx);
        }

        private void ConnectTfsButton(object sender, RoutedEventArgs e)
        {
            _tfs = new TfsConnector(_data.Url);

            if (!_tfs.IsConnected) return;
            ProjectStack.Visibility = Visibility.Visible;
            TfsProjectStack.Visibility = Visibility.Visible;
            ProjectCombo.ItemsSource = _tfs.Projects;
        }

        private void ProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_data.ProjectSelected == "") return;
            IterationStack.Visibility = Visibility.Visible;
            BranchStack.Visibility = Visibility.Visible;
            TfsProject.Text = _data.ProjectSelected;
            var iterationPaths = _tfs.GetIterationPaths(_data.ProjectSelected);

            var regex = new Regex(RegexString);
            var filtered = iterationPaths.Where(x => regex.IsMatch(x)).ToList();

            IterationCombo.ItemsSource = filtered;
        }

        private void IterationSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_data.IterationSelected == "") return;
            var iteration = IterationCombo.SelectedItem.ToString();
            var regex = new Regex(RegexString);
            var matchedGroups = regex.Match(iteration).Groups;

            var extractedData = matchedGroups.Count == 3
                ? new Tuple<string, string>(matchedGroups[1].Value, matchedGroups[2].Value)
                : new Tuple<string, string>("", matchedGroups[1].Value);

            _data.ReleaseName = extractedData.Item1;
            _data.TfsBranch = extractedData.Item2;
        }

        private void ConvertClicked(object sender, RoutedEventArgs e)
        {
            var queryLocation = $"$/FenergoCore/{_data.TfsBranch}";
            var workItemStateFilter = GettrimmedSettingList("workItemStateFilter");
            var data = _tfs.GetChangesetsAndWorkItems(_data.IterationSelected, queryLocation,
                _data.ChangesetFrom, _data.ChangesetTo, Categories, workItemStateFilter);

            DoStuff(data);
            //_listBox.ItemsSource = data.;
        }

        private static List<string> GettrimmedSettingList(string key)
        {
            return ConfigurationManager.AppSettings[key].Split(',').Select(x => x.Trim()).ToList();
        }

        private void GetChangesetTo(object sender, RoutedEventArgs e)
        {
            ShowChangesetTitleByChangesetId(ChangesetTo, ChangesetToText);
        }

        private void GetChangesetFrom(object sender, RoutedEventArgs e)
        {
            ShowChangesetTitleByChangesetId(ChangesetFrom, ChangesetFromText);
        }

        private async void ShowChangesetTitleByChangesetId(IntegerUpDown input, TextBlock output)
        {
            var changeset = input.Value.GetValueOrDefault();

            string result = "";
            if (changeset > 1) result = await Task.Run(() => _tfs.GetChangesetTitleById(changeset));

            output.Text = result;
        }
    }
}
