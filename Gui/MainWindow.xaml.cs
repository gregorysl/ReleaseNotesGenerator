using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TfsData;
using Xceed.Words.NET;

namespace Gui
{
    public partial class MainWindow
    {
        private readonly string _regexString = @".*\\\w+(?:.*)?\\((\w\d.\d+.\d+).\d+)";
        public static TfsConnector _tfs;
        public MainWindow()
        {
            //DoStuff();

            InitializeComponent();
            //Close();
        }

        private static void DoStuff()
        {
            var categories = new[] {"UIFramework", "Framework", "Infrastructure", "WebApp", "PSSolution"};
            var dTestDocx = @"D:\test.docx";
            string fileName = @"D:\Template.docx";

            using (var doc = DocX.Load(fileName))
            {
                //doc.ReplaceText(Tokens.ReleaseNumber, releaseName);

                var secondSection = doc.Paragraphs.FirstOrDefault(x => x.Text == "Code Change sets in this Release");
                var paragraph = secondSection.InsertParagraphAfterSelf("asd").FontSize(10d);
                InsertBeforeOrAfter placeholder = paragraph;
                foreach (var category in categories)
                {
                    var p = placeholder.InsertParagraphAfterSelf(category).FontSize(11d).Heading(HeadingType.Heading2);

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
                    table.Rows[1].Cells[5].Paragraphs[0].Append("{WorkItemI}");
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
            _tfs = new TfsConnector(TfsUrl.Text);

            if (!_tfs.IsConnected) return;
            ProjectStack.Visibility = Visibility.Visible;
            ProjectCombo.ItemsSource = _tfs.Projects;
        }

        private void ProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedIndex == -1) return;
            IterationStack.Visibility = Visibility.Visible;
            var projectName = ProjectCombo.SelectedItem.ToString();
            var iterationPaths = _tfs.GetIterationPaths(projectName);

            
            var regex = new Regex(_regexString);
            var filtered = iterationPaths.Where(x => regex.IsMatch(x)).ToList();

            IterationCombo.ItemsSource = filtered;


        }

        private void IterationSelected(object sender, SelectionChangedEventArgs e)
        {
            if (IterationCombo.SelectedIndex == -1) return;
            var iteration = IterationCombo.SelectedItem.ToString();
            var regex = new Regex(_regexString);
            var a = regex.Match(iteration).Groups;
            if (a.Count == 3)
            {
                ReleaseName.Text = a[1].Value;
                Branch.Text = a[2].Value;
            }
            else
            {
                ReleaseName.Text = "";
                Branch.Text = a[1].Value;
            }
        }

        private void ConvertClicked(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
