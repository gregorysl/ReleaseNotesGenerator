using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataModel;
using TfsData;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace Gui
{
    public partial class MainWindow
    {
        private ReleaseData _data;
        private bool _includeTfsService = false;
        private const string RegexString = @".*\\\w+(?:.*)?\\((\w\d.\d+.\d+).\d+)";
        private static TfsConnector _tfs;
        public List<string> Categories => GettrimmedSettingList("categories");
        public MainWindow()
        {
            InitializeComponent();
            var tfsUrl = ConfigurationManager.AppSettings["tfsUrl"];
            var tfsUsername = ConfigurationManager.AppSettings["tfsUsername"];
            var tfsKey = ConfigurationManager.AppSettings["tfsKey"];
            if (string.IsNullOrWhiteSpace(tfsUrl)) return;

            _tfs = new TfsConnector(tfsUrl, tfsUsername, tfsKey);

            if (!_tfs.IsConnected) return;
            ProjectCombo.ItemsSource = _tfs.Projects;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            _data = new ReleaseData();
            ProjectCombo.SelectedItem = "FenergoCoreSupport";
            _data = new ReleaseData { TfsProject = "FenergoCore", ChangesetFrom = "180899", IterationSelected = @"FenergoCoreSupport\Current\R8.3.1.8", QaBuildName = "R8.4.0_QA.9", CoreBuildName = "R8.4.0_IntTests.16", ReleaseDate = new DateTime(2018, 09, 14), QaBuildDate = new DateTime(2018, 09, 12), CoreBuildDate = new DateTime(2018, 09, 10) };
            DataContext = _data;

        }


        private void ProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            IterationCombo.Visibility = Visibility.Hidden;
            if (ProjectCombo.SelectedItem == null) return;
            IterationCombo.Visibility = Visibility.Visible;
            _data.TfsProject = ProjectCombo.SelectedItem.ToString();
            var iterationPaths = _tfs.GetIterationPaths(_data.TfsProject);

            var regex = new Regex(RegexString);
            var filtered = iterationPaths.Where(x => regex.IsMatch(x)).ToList();

            IterationCombo.ItemsSource = filtered;
        }

        private void IterationSelected(object sender, SelectionChangedEventArgs e)
        {
            if (IterationCombo.SelectedItem == null) return;
            var iteration = IterationCombo.SelectedItem.ToString();
            _data.IterationSelected = iteration;
            var regex = new Regex(RegexString);
            var matchedGroups = regex.Match(iteration).Groups;

            var extractedData = matchedGroups.Count == 3
                ? new Tuple<string, string>(matchedGroups[1].Value, matchedGroups[2].Value)
                : new Tuple<string, string>("", matchedGroups[1].Value);

            _data.ReleaseName = extractedData.Item1;
            _data.TfsBranch = extractedData.Item2;
        }

        private async void ConvertClicked(object sender, RoutedEventArgs e)
        {
            var queryLocation = $"$/{_data.TfsProject}/{_data.TfsBranch}";
            var workItemStateInclude = GettrimmedSettingList("workItemStateInclude");
            var workItemTypeExclude = GettrimmedSettingList("workItemTypeExclude");
            LoadingBar.Visibility = Visibility.Visible;
            var downloadedData = await Task.Run(() => _tfs.GetChangesetsRest(queryLocation,_data.ChangesetFrom, _data.ChangesetTo, Categories));

            LoadingBar.Visibility = Visibility.Hidden;
            _data.tfs = downloadedData;

            _dataGrid.ItemsSource = _data.tfs.Changes;
            FilterTfsChanges();
            WorkItemProgress.Value = 0;
            WorkItemProgress.Maximum= _data.tfs.Changes.Count;
            var workToDownload = new List<int>();
            foreach (var item in _data.tfs.Changes)
            {
                var wok = await Task.Run(() => _tfs.GetChangesetWorkItemsRest(item));
                var filteredWok = wok
                    .Where(x => !workItemTypeExclude.Contains(x.workItemType))
                    .Where(x => workItemStateInclude.Contains(x.state))
                    .Select(x=>x.id).ToList();
                workToDownload.AddRange(filteredWok);
                item.Works = filteredWok;
                WorkItemProgress.Value += 1;

            }

            var joinedWorkItems = string.Join(",", workToDownload.Distinct().ToList());
            _data.tfs.WorkItems = _tfs.GetWorkItemsByIdAndIteration(joinedWorkItems, _data.IterationSelected, workItemStateInclude,workItemTypeExclude);

            //if (!string.IsNullOrWhiteSpace(downloadedData.ErrorMessgage))
            //{
            //    MessageBox.Show(downloadedData.ErrorMessgage);
            //}
            //else
            //{
            //    _data.CategorizedChanges = downloadedData.CategorizedChanges;
            //    _data.Changes = downloadedData.Changes;


            //    _data.WorkItems = downloadedData.WorkItems;
            //    //_dataGrid.ItemsSource = _data.CategorizedChanges;
            //    _dataGrid.ItemsSource = _data.Changes;
            //    _dataGrid.Items.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));
            //}
        }

        private static List<string> GettrimmedSettingList(string key)
        {
            return ConfigurationManager.AppSettings[key].Split(',').Select(x => x.Trim()).ToList();
        }

        private void GetChangesetTo(object sender, RoutedEventArgs e)
        {
            ShowChangesetTitleByChangesetId(ChangesetTo);
        }

        private void GetChangesetFrom(object sender, RoutedEventArgs e)
        {
            ShowChangesetTitleByChangesetId(ChangesetFrom);
        }

        private async void ShowChangesetTitleByChangesetId(TextBox input)
        {
            input.ToolTip = "";
            var parsed = int.TryParse(input.Text, out int changeset);
            if (!parsed) return;

            var result = "";
            if (changeset > 1) result = await Task.Run(() => _tfs.GetChangesetTitleById(changeset));

            input.ToolTip = result;
        }

        private void SetAsPsRefreshClick(object sender, RoutedEventArgs e)
        {
            Change item = (Change)((Button)e.Source).DataContext;
            _data.PsRefresh = item;
        }

        private void SetAsCoreClick(object sender, RoutedEventArgs e)
        {
            Change item = (Change)((Button)e.Source).DataContext;
            _data.CoreChange = item;
        }

        private void CreateDocument(object sender, RoutedEventArgs e)
        {
            var list = new List<ChangesetInfo>();
            var selectedChangesets = _data.tfs.Changes.Where(x => x.Selected).ToList();
            foreach (var item in selectedChangesets)
            {
                var change = new ChangesetInfo { Id = item.changesetId, Comment = item.comment, CommitedBy = item.checkedInBy.displayName, Created = item.createdDate, WorkItemId="N/A", WorkItemTitle="N/A" };
                if (!item.Works.Any()) { list.Add(change); }
                foreach (var workItemId in item.Works)
                {
                    var workItem = _data.tfs.WorkItems.FirstOrDefault(x => x.Id == workItemId);
                    change.WorkItemId = workItem.Id.ToString();
                    change.WorkItemTitle = workItem.Title;
                    list.Add(change);
                }
            }



            var categories = new Dictionary<string, List<ChangesetInfo>>();
            foreach (var category in _data.tfs.Categorized)
            {
                var cha = list.Where(x => category.Value.Contains(x.Id)).ToList();
                if (cha.Any())
                {
                    categories.Add(category.Key, cha);
                }
            }

            var workItems = _data.tfs.WorkItems.Where(x => x.ClientProject != "General");
            var pbi = _data.tfs.WorkItems.Where(x => x.ClientProject == "General");
            var message = new DocumentEditor().ProcessData(_data, categories, workItems, pbi);
            if (!string.IsNullOrWhiteSpace(message)) MessageBox.Show(message);
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            if (checkbox == null) return;
            _includeTfsService = checkbox.IsChecked.GetValueOrDefault(false);

            FilterTfsChanges();

        }

        private void FilterTfsChanges()
        {
            foreach (var change in _data.tfs.Changes)
            {
                if (change.checkedInBy.displayName == "TFS Service")
                {
                    change.Selected = _includeTfsService;
                }
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
