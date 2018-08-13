using System.Diagnostics;
using System.Linq;
using Xceed.Words.NET;

namespace Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            var categories = new[] { "UIFramework", "Framework", "Infrastructure", "WebApp", "PSSolution" };
            var dTestDocx = @"D:\test.docx";
            string fileName = @"D:\Template.docx";

            using (var doc = DocX.Load(fileName))
            {
                var secondSection = doc.Paragraphs.FirstOrDefault(x => x.Text == "Code Change sets in this Release");
                var paragraph = secondSection.InsertParagraphAfterSelf("asd").FontSize(10d);
                InsertBeforeOrAfter placeholder = paragraph;
                foreach (var category in categories)
                {
                    var p = placeholder.InsertParagraphAfterSelf(category).FontSize(11d).Heading(HeadingType.Heading2);

                    var table = p.InsertTableAfterSelf(2, 6);
                    table.Rows[0].Cells[0].Paragraphs[0].Append(category).Bold();
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

                var thirdSection = placeholder.InsertParagraphAfterSelf("Product reported Defects in this Release")
                    .Heading(HeadingType.Heading1);
                var fourthSection1 = thirdSection.InsertParagraphAfterSelf("Product Backlog Items and KTRs in this Release")
                    .Heading(HeadingType.Heading1);

                doc.SaveAs(dTestDocx);
            }

            Process.Start(dTestDocx);
            
            InitializeComponent();
            this.Close();
        }
    }
}
