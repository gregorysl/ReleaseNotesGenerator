using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DataModel;
using TfsData;
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
            
            var tfsUrl = ConfigurationManager.AppSettings["tfsUrl"];
            if (string.IsNullOrWhiteSpace(tfsUrl)) return;

            _tfs = new TfsConnector(tfsUrl);

            if (!_tfs.IsConnected) return;
            ProjectStack.Visibility = Visibility.Visible;
            ProjectCombo.ItemsSource = _tfs.Projects;
        }
        

        private void ProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_data.ProjectSelected == "") return;
            IterationStack.Visibility = Visibility.Visible;
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

            _data.CategorizedChanges = data.CategorizedChanges;
            _data.WorkItems = data.WorkItems;
            _dataGrid.ItemsSource = _data.CategorizedChanges;
            _dataGrid.Items.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));
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

        private void SetAsPsRefreshClick(object sender, RoutedEventArgs e)
        {
            ChangesetInfo item = (ChangesetInfo) ((Button) e.Source).DataContext;
            _data.PsRefresh = item;
        }

        private void SetAsCoreClick(object sender, RoutedEventArgs e)
        {
            ChangesetInfo item = (ChangesetInfo)((Button)e.Source).DataContext;
            _data.CoreChange = item;
        }

        private void CreateDocument(object sender, RoutedEventArgs e)
        {

            var changesets = _data.CategorizedChanges.Where(x=>x.CommitedBy != "TFS Service").ToList();
            var categories = new Dictionary<string, List<ChangesetInfo>>();
            foreach (var category in Categories)
            {
                var cha = changesets.Where(x => x.Categories.Contains(category)).ToList();
                if (cha.Any())
                {
                    categories.Add(category, cha);
                }
            }

            var workItems = _data.WorkItems.Where(x => x.ClientProject != "General");
            var pbi = _data.WorkItems.Where(x => x.ClientProject == "General");
            new DocumentEditor().ProcessData(_data, categories, workItems, pbi);
        }
    }
}
