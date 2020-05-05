using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataModel;
using ReleaseNotesService;
using TfsData;
using MessageBox = System.Windows.MessageBox;

namespace Gui
{
    public partial class MainWindow
    {
        private bool _includeTfsService = false;
        private readonly string _documentLocation;
        private const string RegexString = @".*\\\w+(?:.*)?\\((\w\d.\d+.\d+).\d+)";
        private static TfsConnector _tfs;
        private static Generator _generator;
        private DownloadedItems _downloadedData;
        public List<string> Categories => GettrimmedSettingList("categories");
        public MainWindow()
        {
            InitializeComponent();
            var tfsUrl = ConfigurationManager.AppSettings["tfsUrl"];
            var tfsUsername = ConfigurationManager.AppSettings["tfsUsername"];
            var tfsKey = ConfigurationManager.AppSettings["tfsKey"];
            var adoUrl = ConfigurationManager.AppSettings["adoUrl"];
            var adoUsername = ConfigurationManager.AppSettings["adoUsername"];
            var adoKey = ConfigurationManager.AppSettings["adoKey"];
            _documentLocation = ConfigurationManager.AppSettings["documentLocation"];
            if (string.IsNullOrWhiteSpace(tfsUrl)) return;

            _tfs = new TfsConnector(tfsUrl, tfsUsername, tfsKey, adoUrl, adoUsername, adoKey);
            _generator = new Generator(_tfs);

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

        private async void DownloadClicked(object sender, RoutedEventArgs e)
        {
            App.Data.PsRefresh = null;
            var tfsProject = App.Data.TfsProject;
            var branch = App.Data.TfsBranch;
            WorkItemProgress.IsIndeterminate = true;
            var changesetFrom = App.Data.ChangesetFrom;
            var changesetTo = App.Data.ChangesetTo;
            var iteration = App.Data.IterationSelected;


            _downloadedData = await _generator.DownloadData(tfsProject, branch, changesetFrom, changesetTo, iteration, _includeTfsService);

            WorkItemProgress.IsIndeterminate = false;
            
            _dataGrid.ItemsSource = _downloadedData.Changes;
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

        private void CreateDocument(object sender, RoutedEventArgs e)
        {
            var workItemStateInclude = GettrimmedSettingList("workItemStateInclude");
            var message = _generator.CreateDoc(_downloadedData, workItemStateInclude, App.Data, _documentLocation);
            if (!string.IsNullOrWhiteSpace(message)) MessageBox.Show(message);
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            if (checkbox == null) return;
            _includeTfsService = checkbox.IsChecked.GetValueOrDefault(false);

            _downloadedData.FilterTfsChanges();

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