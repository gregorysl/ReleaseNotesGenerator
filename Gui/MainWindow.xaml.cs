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

            ProjectCombo.ItemsSource = _tfs.Projects();
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = App.Data;
        }


        private void ProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            IterationCombo.Visibility = Visibility.Hidden;
            if (ProjectCombo.SelectedItem == null) return;
            IterationCombo.Visibility = Visibility.Visible;
            var iterationPaths = _tfs.GetIterationPaths(App.Data.ProjectSelected);

            var regex = new Regex(RegexString);
            var filtered = iterationPaths.Where(x => regex.IsMatch(x)).ToList();

            IterationCombo.ItemsSource = filtered;
        }

        private void IterationSelected(object sender, SelectionChangedEventArgs e)
        {
            if (IterationCombo.SelectedItem == null) return;
            var iteration = IterationCombo.SelectedItem.ToString();
            App.Data.IterationSelected = iteration;
            var regex = new Regex(RegexString);
            var matchedGroups = regex.Match(iteration).Groups;

            var extractedData = matchedGroups.Count == 3
                ? new Tuple<string, string>(matchedGroups[1].Value, matchedGroups[2].Value)
                : new Tuple<string, string>("", matchedGroups[1].Value);

            App.Data.ReleaseName = extractedData.Item1;
            App.Data.TfsBranch = extractedData.Item2;
        }

        private async void ConvertClicked(object sender, RoutedEventArgs e)
        {
            var queryLocation = $"$/{App.Data.TfsProject}/{App.Data.TfsBranch}";
            var workItemTypeExclude = GettrimmedSettingList("workItemTypeExclude");
            LoadingBar.Visibility = Visibility.Visible;
            var downloadedData = await Task.Run(() => _tfs.GetChangesetsRest(queryLocation, App.Data.ChangesetFrom, App.Data.ChangesetTo, Categories));

            LoadingBar.Visibility = Visibility.Hidden;
            App.Data.tfs = downloadedData;

            _dataGrid.ItemsSource = App.Data.tfs.Changes;
            FilterTfsChanges();
            WorkItemProgress.Value = 0;
            WorkItemProgress.Maximum = App.Data.tfs.Changes.Count;
            var workToDownload = new List<int>();
            foreach (var item in App.Data.tfs.Changes)
            {
                var wok = await Task.Run(() => _tfs.GetChangesetWorkItemsRest(item));
                var filteredWok = wok
                    .Where(x => !workItemTypeExclude.Contains(x.workItemType))
                    .Select(x => x.id).ToList();
                workToDownload.AddRange(filteredWok);
                item.Works = filteredWok;
                WorkItemProgress.Value += 1;

            }
            App.Data.tfs.WorkItems = _tfs.GetWorkItemsByIdAndIteration(workToDownload, App.Data.IterationSelected, workItemTypeExclude);


            //if (!string.IsNullOrWhiteSpace(downloadedData.ErrorMessgage))
            //{
            //    MessageBox.Show(downloadedData.ErrorMessgage);
            //}
            //else
            //{
            //    App._data.CategorizedChanges = downloadedData.CategorizedChanges;
            //    App._data.Changes = downloadedData.Changes;


            //    App._data.WorkItems = downloadedData.WorkItems;
            //    //App._dataGrid.ItemsSource = App._data.CategorizedChanges;
            //    App._dataGrid.ItemsSource = App._data.Changes;
            //    App._dataGrid.Items.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));
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
            App.Data.PsRefresh = item;
        }

        private void SetAsCoreClick(object sender, RoutedEventArgs e)
        {
            Change item = (Change)((Button)e.Source).DataContext;
            App.Data.CoreChange = item;
        }

        private void CreateDocument(object sender, RoutedEventArgs e)
        {
            var list = new List<ChangesetInfo>();
            var selectedChangesets = App.Data.tfs.Changes.Where(x => x.Selected).OrderBy(x => x.changesetId).ToList();
            foreach (var item in selectedChangesets)
            {
                var change = new ChangesetInfo { Id = item.changesetId, Comment = item.comment, CommitedBy = item.checkedInBy.displayName, Created = item.createdDate, WorkItemId = "N/A", WorkItemTitle = "N/A" };
                if (!item.Works.Any()) { list.Add(change); }
                foreach (var workItemId in item.Works)
                {
                    var workItem = App.Data.tfs.WorkItems.FirstOrDefault(x => x.Id == workItemId);

                    change = new ChangesetInfo { Id = item.changesetId, Comment = item.comment, CommitedBy = item.checkedInBy.displayName, Created = item.createdDate, WorkItemId = workItem.Id.ToString(), WorkItemTitle = workItem.Title };
                    list.Add(change);
                }
            }



            var categories = new Dictionary<string, List<ChangesetInfo>>();
            foreach (var category in App.Data.tfs.Categorized)
            {
                var cha = list.Where(x => category.Value.Contains(x.Id)).ToList();
                if (cha.Any())
                {
                    categories.Add(category.Key, cha);
                }
            }

            var workItemStateInclude = GettrimmedSettingList("workItemStateInclude");
            var workItems = App.Data.tfs.WorkItems
                .Where(x => workItemStateInclude.Contains(x.State))
                .Where(x => x.ClientProject != "General");
            var pbi = App.Data.tfs.WorkItems.Where(x => x.ClientProject == "General");
            var message = new DocumentEditor().ProcessData(App.Data, categories, workItems, pbi);
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
            foreach (var change in App.Data.tfs.Changes)
            {
                if (change.checkedInBy.displayName == "TFS Service" || change.checkedInBy.displayName == "Project Collection Build Service (Product)" || change.comment.Contains("Automatic refresh", StringComparison.OrdinalIgnoreCase))
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

        private void CopyClick(object sender, MouseButtonEventArgs e)
        {
            App.Data.TfsProject = ProjectCombo.SelectedItem.ToString();
        }
    }
}
