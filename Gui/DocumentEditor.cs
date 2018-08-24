using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DataModel;
using Xceed.Words.NET;

namespace Gui
{
    public class DocumentEditor
    {
        public void ProcessData(ReleaseData data, Dictionary<string, List<ChangesetInfo>> categorizedChangesets,
            IEnumerable<ClientWorkItem> workItems, IEnumerable<ClientWorkItem> pbi)
        {

            var dTestDocx = @"D:\test.docx";
            string fileName = @"D:\Template.docx";

            using (var doc = DocX.Load(fileName))
            {
                doc.ReplaceText("{ReleaseName}", data.ReleaseName);
                doc.ReplaceText("{ReleaseDate}", data.ReleaseDateFormated);
                doc.ReplaceText("{TfsBranch}", data.TfsBranch);
                doc.ReplaceText("{QaBuildName}", data.QaBuildName);
                doc.ReplaceText("{QaBuildDate}", data.QaBuildDateFormated);
                doc.ReplaceText("{CoreBuildName}", data.CoreBuildName);
                doc.ReplaceText("{CoreBuildDate}", data.CoreBuildDateFormated);
                doc.ReplaceText("{PsRefreshChangeset}", data.PsRefresh.Id.ToString());
                doc.ReplaceText("{PsRefreshDate}", data.PsRefresh.Created.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US")));
                doc.ReplaceText("{PsRefreshName}", data.PsRefresh.Comment);
                doc.ReplaceText("{CoreChangeset}", data.CoreChange.Id.ToString());
                doc.ReplaceText("{CoreDate}", data.CoreChange.Created.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US")));
                var lastParagraph = doc.Paragraphs[doc.Paragraphs.Count - 1];
                var secondSection = SecondSection(categorizedChangesets, lastParagraph);
                var thirdSection = ThirdSection(workItems, secondSection);
                var fourthSection = FourthSection(pbi, thirdSection);

                var fifthSection = fourthSection.CreateHeadingSection("Test Report");
                var sixthSection = fifthSection.CreateHeadingSection("Known issues in this Release");

                doc.SaveAs(dTestDocx);
            }

            Process.Start(dTestDocx);
        }

        private static InsertBeforeOrAfter SecondSection(Dictionary<string, List<ChangesetInfo>> categorizedChangesets, InsertBeforeOrAfter lastPart)
        {
            var secondSection = lastPart.CreateHeadingSection("Code Change sets in this Release");
            var paragraph = secondSection
                .InsertParagraphAfterSelf("The following list of code check-ins to TFS was compiled to make up this release:")
                .FontSize(10d);
            InsertBeforeOrAfter newLastPart = paragraph;
            foreach (var category in categorizedChangesets)
            {
                var p = newLastPart.InsertParagraphAfterSelf(category.Key).FontSize(11d).Heading(HeadingType.Heading2);

                var table = p.InsertTableAfterSelf(2, 6);
                table.SetWidthsPercentage(new[] {10f, 15f, 15f, 20f, 10f, 30f}, null);
                table.Rows[0].Cells[0].FillFirstParagraph("TFS").Bold();
                table.Rows[0].Cells[1].FillFirstParagraph("Developer").Bold();
                table.Rows[0].Cells[2].FillFirstParagraph("Date/Time").Bold();
                table.Rows[0].Cells[3].FillFirstParagraph("Description").Bold();
                table.Rows[0].Cells[4].FillFirstParagraph("Work Item").Bold();
                table.Rows[0].Cells[5].FillFirstParagraph("Work Item Description").Bold();

                var rowPattern = table.Rows[1];
                rowPattern.Cells[0].FillFirstParagraph("{TfsID}");
                rowPattern.Cells[1].FillFirstParagraph("{Dev}");
                rowPattern.Cells[2].FillFirstParagraph("{Date}");
                rowPattern.Cells[3].FillFirstParagraph("{Desc}");
                rowPattern.Cells[4].FillFirstParagraph("{WorkItemId}");
                rowPattern.Cells[5].FillFirstParagraph("{WorkItemTitle}");

                foreach (var change in category.Value)
                {
                    var newItem = table.InsertRow(rowPattern, table.RowCount - 1);

                    newItem.ReplaceText("{TfsID}", change.Id.ToString());
                    newItem.ReplaceText("{Dev}", change.CommitedBy);
                    newItem.ReplaceText("{Date}", change.Created.ToString());
                    newItem.ReplaceText("{Desc}", change.Comment);
                    newItem.ReplaceText("{WorkItemId}", change.WorkItemId);
                    newItem.ReplaceText("{WorkItemTitle}", change.WorkItemTitle);
                }

                rowPattern.Remove();
                table.AutoFit = AutoFit.ColumnWidth;
                newLastPart = table;
            }

            return newLastPart;
        }

        private static Table ThirdSection(IEnumerable<ClientWorkItem> workItems, InsertBeforeOrAfter lastPart)
        {
            var thirdSection = lastPart.CreateHeadingSection("Product reported Defects in this Release");
            var thirdSectionParagraph = thirdSection
                .InsertParagraphAfterSelf("This section gives a list of Client-facing defects that were fixed in this release")
                .FontSize(10d);
            var workItemTable = thirdSectionParagraph.InsertTableAfterSelf(2, 3);
            workItemTable.SetWidthsPercentage(new[] {10f, 75f, 15f}, null);
            workItemTable.AutoFit = AutoFit.ColumnWidth;
            workItemTable.Rows[0].Cells[0].FillFirstParagraph("Bug Id").Bold();
            workItemTable.Rows[0].Cells[1].FillFirstParagraph("Work Item Description").Bold();
            workItemTable.Rows[0].Cells[2].FillFirstParagraph("Client Project").Bold();

            var placeholderRow = workItemTable.Rows[1];
            placeholderRow.Cells[0].FillFirstParagraph("{TfsID}");
            placeholderRow.Cells[1].FillFirstParagraph("{WorkItemTitle}");
            placeholderRow.Cells[2].FillFirstParagraph("{Client}");

            foreach (var item in workItems)
            {
                var newItem = workItemTable.InsertRow(placeholderRow, workItemTable.RowCount - 1);

                newItem.ReplaceText("{TfsID}", item.Id.ToString());
                newItem.ReplaceText("{WorkItemTitle}", item.Title);
                newItem.ReplaceText("{Client}", item.ClientProject);
            }

            placeholderRow.Remove();
            return workItemTable;
        }

        private static Table FourthSection(IEnumerable<ClientWorkItem> pbi, InsertBeforeOrAfter lastPart)
        {
            var fourthSection = lastPart.CreateHeadingSection("Product Backlog Items in this Release");
            var fourthSectionParagraph = fourthSection
                .InsertParagraphAfterSelf("This section gives a list of PBIs that were delivered in this release:")
                .FontSize(10d);
            var pbiTable = fourthSectionParagraph.InsertTableAfterSelf(2, 2);
            pbiTable.SetWidthsPercentage(new[] {25f, 75f}, null);
            pbiTable.AutoFit = AutoFit.ColumnWidth;
            pbiTable.Rows[0].Cells[0].FillFirstParagraph("Bug Id").Bold();
            pbiTable.Rows[0].Cells[1].FillFirstParagraph("Work Item Description").Bold();

            var placeholderRow = pbiTable.Rows[1];
            placeholderRow.Cells[0].FillFirstParagraph("{TfsID}");
            placeholderRow.Cells[1].FillFirstParagraph("{WorkItemTitle}");

            foreach (var item in pbi)
            {
                var newItem = pbiTable.InsertRow(placeholderRow, pbiTable.RowCount - 1);

                newItem.ReplaceText("{TfsID}", item.Id.ToString());
                newItem.ReplaceText("{WorkItemTitle}", item.Title);
                newItem.ReplaceText("{Client}", item.ClientProject);
            }

            placeholderRow.Remove();
            return pbiTable;
        }
    }
}
