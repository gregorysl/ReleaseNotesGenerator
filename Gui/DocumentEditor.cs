using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DataModel;
using Xceed.Document.NET;
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
                var dTestDocx = $@"D:\{data.ReleaseName} Patch Release Notes.docx";
                string fileName = @"D:\Template.docx";

                using (var doc = DocX.Load(fileName))
                {
                    doc.ReplaceText("{ReleaseName}", data.ReleaseName);
                    doc.ReplaceText("{ReleaseDate}", data.ReleaseDate);
                    doc.ReplaceText("{TfsBranch}", data.TfsBranch);
                    doc.ReplaceText("{QaBuildName}", data.QaBuildName);
                    doc.ReplaceText("{QaBuildDate}", data.QaBuildDate);
                    doc.ReplaceText("{PsRefreshChangeset}", data.PsRefresh.changesetId.ToString());
                    doc.ReplaceText("{PsRefreshDate}",FormatData(data.PsRefresh.createdDate));
                    doc.ReplaceText("{PsRefreshName}", data.PsRefresh.comment);
                    doc.ReplaceText("{CoreChangeset}", data.CoreChange.changesetId.ToString());
                    doc.ReplaceText("{CoreDate}",FormatData(data.CoreChange.createdDate));

                    doc.InsertTableOfContents("Contents",
                        TableOfContentsSwitches.O | TableOfContentsSwitches.U | TableOfContentsSwitches.Z | TableOfContentsSwitches.H | TableOfContentsSwitches.T,
                        "FR HeadNoToc");

                    doc.InsertSectionPageBreak();
                    var lastParagraph = doc.Paragraphs[doc.Paragraphs.Count - 1];
                    var secondSection = SecondSection(categorizedChangesets, lastParagraph);
                    var thirdSection = ThirdSection(workItems, secondSection);
                    var fourthSection = FourthSection(pbi, thirdSection);

                    var fifthSection = fourthSection.CreateHeadingSection("Test Report");
                    var sixthSection = SixthSection(fifthSection);
                    
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

        private static string FormatData(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));
        }

        private InsertBeforeOrAfter SecondSection(Dictionary<string, List<ChangesetInfo>> categorizedChangesets, InsertBeforeOrAfter lastPart)
        {
            var paragraph = CreateSectionWithParagraph(lastPart, "Code Change sets in this Release",
                "The following list of code check-ins to TFS was compiled to make up this release");
            InsertBeforeOrAfter newLastPart = paragraph;
            foreach (var category in categorizedChangesets)
            {
                var p = newLastPart.InsertParagraphAfterSelf(category.Key).FontSize(11d).Heading(HeadingType.Heading2);

                var table = p.InsertTableAfterSelf(category.Value.Count +2, 4);
                table.SetWidthsPercentage(new[] {10f, 25, 30f, 35f}, null);
                table.AutoFit = AutoFit.ColumnWidth;
                table.GetCell(0,0).FillFirstParagraph("TFS").Bold();
                table.GetCell(0,1).FillFirstParagraph("Developer").Bold();
                table.GetCell(0,2).FillFirstParagraph("Date/Time").Bold();
                table.GetCell(0,3).FillFirstParagraph("Description").Bold();
                
                for (int i = 0; i < category.Value.Count - 1; i++)
                {
                    var item = category.Value[i];
                    table.GetCell(i + 1, 0).FillFirstParagraph(item.Id.ToString());
                    table.GetCell(i + 1, 1).FillFirstParagraph(item.CommitedBy);
                    table.GetCell(i + 1, 2).FillFirstParagraph(item.Created.ToString());
                    table.GetCell(i + 1, 3).FillFirstParagraph(item.Comment);
                }

                newLastPart = table;
            }

            return newLastPart;
        }

        private Table ThirdSection(IEnumerable<ClientWorkItem> workItems, InsertBeforeOrAfter lastPart)
        {
            var workItemList = workItems.ToList();
            var paragraph = CreateSectionWithParagraph(lastPart, "Product reported Defects in this Release",
                "This section gives a list of Client-facing defects that were fixed in this release");
            var table = paragraph.InsertTableAfterSelf(workItemList.Count +2, 3);
            table.SetWidthsPercentage(new[] {10f, 75f, 15f}, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0,0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0,1).FillFirstParagraph("Work Item Description").Bold();
            table.GetCell(0,2).FillFirstParagraph("Client Project").Bold();

            for (int i = 0; i < workItemList.Count - 1; i++)
            {
                var item = workItemList[i];
                table.GetCell(i + 1, 0).FillFirstParagraph(item.Id.ToString());
                table.GetCell(i + 1, 1).FillFirstParagraph(item.Title);
                table.GetCell(i + 1, 2).FillFirstParagraph(item.ClientProject);
            }

            return table;
        }
        
        private Table FourthSection(IEnumerable<ClientWorkItem> pbi, InsertBeforeOrAfter lastPart)
        {
            var pbiList = pbi.ToList();
            var paragraph = CreateSectionWithParagraph(lastPart,"Product Backlog Items in this Release",
                "This section gives a list of PBIs that were delivered in this release");
            var table = paragraph.InsertTableAfterSelf(pbiList.Count +2, 2);
            table.SetWidthsPercentage(new[] {25f, 75f}, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0,0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0,1).FillFirstParagraph("Work Item Description").Bold();
          
            for (int i = 0; i < pbiList.Count -1; i++)
            {
                var item = pbiList[i];
                table.GetCell(i+1,0).FillFirstParagraph(item.Id.ToString());
                table.GetCell(i+1,1).FillFirstParagraph(item.Title);
            }
            return table;
        }
        private Table SixthSection(InsertBeforeOrAfter lastPart)
        {
            var paragraph = CreateSectionWithParagraph(lastPart,"Known issues in this Release",
                "This section gives a list of bugs that were identified throughout testing of this release");
            var table = paragraph.InsertTableAfterSelf(2, 2);
            table.SetWidthsPercentage(new[] {25f, 75f}, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0,0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0,1).FillFirstParagraph("Work Item Description").Bold();

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
