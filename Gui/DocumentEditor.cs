using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DataModel;
using Xceed.Words.NET;

namespace Gui
{
    public class DocumentEditor
    {
        public string ProcessData(ReleaseData data, Dictionary<string, List<ChangesetInfo>> categorizedChangesets,
            IEnumerable<ClientWorkItem> workItems, IEnumerable<ClientWorkItem> pbi)
        {
            try
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
                    doc.ReplaceText("{PsRefreshChangeset}", data.PsRefresh.changesetId.ToString());
                    doc.ReplaceText("{PsRefreshDate}",
                        data.PsRefresh.createdDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US")));
                    doc.ReplaceText("{PsRefreshName}", data.PsRefresh.comment);
                    doc.ReplaceText("{CoreChangeset}", data.CoreChange.changesetId.ToString());
                    doc.ReplaceText("{CoreDate}",
                        data.CoreChange.createdDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US")));
                    
                    doc.InsertTableOfContents("Contents",
                        TableOfContentsSwitches.O | TableOfContentsSwitches.U | TableOfContentsSwitches.Z | TableOfContentsSwitches.H | TableOfContentsSwitches.T,
                        "FR HeadNoToc");

                    doc.InsertSectionPageBreak();
                    var lastParagraph = doc.Paragraphs[doc.Paragraphs.Count - 1];
                    var secondSection = SecondSection(categorizedChangesets, lastParagraph);
                    var thirdSection = ThirdSection(workItems, secondSection);
                    var fourthSection = FourthSection(pbi, thirdSection);

                    var fifthSection = fourthSection.CreateHeadingSection("Test Report");
                    var sixthSection = fifthSection.CreateHeadingSection("Known issues in this Release");
                    
                    doc.SaveAs(dTestDocx);
                }

                Process.Start(dTestDocx);
                return String.Empty;
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
        }

        private InsertBeforeOrAfter SecondSection(Dictionary<string, List<ChangesetInfo>> categorizedChangesets, InsertBeforeOrAfter lastPart)
        {
            var paragraph = CreateSectionWithParagraph(lastPart, "Code Change sets in this Release",
                "The following list of code check-ins to TFS was compiled to make up this release");
            InsertBeforeOrAfter newLastPart = paragraph;
            foreach (var category in categorizedChangesets)
            {
                var p = newLastPart.InsertParagraphAfterSelf(category.Key).FontSize(11d).Heading(HeadingType.Heading2);

                var table = p.InsertTableAfterSelf(2, 6);
                table.SetWidthsPercentage(new[] {10f, 15f, 15f, 20f, 10f, 30f}, null);
                table.GetCell(0,0).FillFirstParagraph("TFS").Bold();
                table.GetCell(0,1).FillFirstParagraph("Developer").Bold();
                table.GetCell(0,2).FillFirstParagraph("Date/Time").Bold();
                table.GetCell(0,3).FillFirstParagraph("Description").Bold();
                table.GetCell(0,4).FillFirstParagraph("Work Item").Bold();
                table.GetCell(0,5).FillFirstParagraph("Work Item Description").Bold();

                var rowPattern = table.Rows[1];
                table.GetCell(1,0).FillFirstParagraph("{TfsID}");
                table.GetCell(1,1).FillFirstParagraph("{Dev}");
                table.GetCell(1,2).FillFirstParagraph("{Date}");
                table.GetCell(1,3).FillFirstParagraph("{Desc}");
                table.GetCell(1,4).FillFirstParagraph("{WorkItemId}");
                table.GetCell(1,5).FillFirstParagraph("{WorkItemTitle}");

                foreach (var change in category.Value)
                {
                    var newItem = table.InsertRow(rowPattern, table.RowCount - 1);

                    newItem.ReplaceText("{TfsID}", change.Id.ToString());
                    newItem.ReplaceText("{Dev}", change.CommitedBy);
                    newItem.ReplaceText("{Date}", change.Created.ToString());
                    newItem.ReplaceText("{Desc}", change.Comment??" ");
                    newItem.ReplaceText("{WorkItemId}", change.WorkItemId);
                    newItem.ReplaceText("{WorkItemTitle}", change.WorkItemTitle);
                }

                rowPattern.Remove();
                table.AutoFit = AutoFit.ColumnWidth;
                newLastPart = table;
            }

            return newLastPart;
        }

        private Table ThirdSection(IEnumerable<ClientWorkItem> workItems, InsertBeforeOrAfter lastPart)
        {
            var paragraph = CreateSectionWithParagraph(lastPart, "Product reported Defects in this Release",
                "This section gives a list of Client-facing defects that were fixed in this release");
            var table = paragraph.InsertTableAfterSelf(2, 3);
            table.SetWidthsPercentage(new[] {10f, 75f, 15f}, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0,0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0,1).FillFirstParagraph("Work Item Description").Bold();
            table.GetCell(0,2).FillFirstParagraph("Client Project").Bold();

            var placeholderRow = table.Rows[1];
            table.GetCell(1,0).FillFirstParagraph("{TfsID}");
            table.GetCell(1,1).FillFirstParagraph("{WorkItemTitle}");
            table.GetCell(1,2).FillFirstParagraph("{Client}");

            foreach (var item in workItems)
            {
                var newItem = table.InsertRow(placeholderRow, table.RowCount - 1);

                newItem.ReplaceText("{TfsID}", item.Id.ToString());
                newItem.ReplaceText("{WorkItemTitle}", item.Title);
                newItem.ReplaceText("{Client}", item.ClientProject);
            }

            placeholderRow.Remove();
            return table;
        }

        private Table FourthSection(IEnumerable<ClientWorkItem> pbi, InsertBeforeOrAfter lastPart)
        {
            var paragraph = CreateSectionWithParagraph(lastPart,"Product Backlog Items in this Release",
                "This section gives a list of PBIs that were delivered in this release");
            var table = paragraph.InsertTableAfterSelf(2, 2);
            table.SetWidthsPercentage(new[] {25f, 75f}, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0,0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0,1).FillFirstParagraph("Work Item Description").Bold();

            var placeholderRow = table.Rows[1];
            table.GetCell(1,0).FillFirstParagraph("{TfsID}");
            table.GetCell(1,1).FillFirstParagraph("{WorkItemTitle}");

            foreach (var item in pbi)
            {
                var newItem = table.InsertRow(placeholderRow, table.RowCount - 1);

                newItem.ReplaceText("{TfsID}", item.Id.ToString());
                newItem.ReplaceText("{WorkItemTitle}", item.Title);
                newItem.ReplaceText("{Client}", item.ClientProject);
            }

            placeholderRow.Remove();
            return table;
        }

        private Paragraph CreateSectionWithParagraph(InsertBeforeOrAfter lastPart, string headingTitle, string paragraphText)
        {
            var heading = lastPart.CreateHeadingSection(headingTitle);
            var paragraph = heading.InsertParagraphAfterSelf(paragraphText).FontSize(10d);
            return paragraph;

        }
    }
}
