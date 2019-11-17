using System;
using System.Collections.Generic;
using System.Linq;
using DataModel;
using Xceed.Document.NET;

namespace Gui
{

    public static class DocumentHelper
    {
        public static Paragraph CreateHeadingSection(this InsertBeforeOrAfter paragraph, string title)
        {
            return paragraph.InsertParagraphAfterSelf(title)
                .Heading(HeadingType.Heading1);
        }

        public static Paragraph FillFirstParagraph(this Cell c, string text)
        {
            return c.Paragraphs[0].Append(text);
        }

        public static Cell GetCell(this Table t, int row, int col)
        {
            return t.Rows[row].Cells[col];
        }
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
        public static InsertBeforeOrAfter SecondSection(Dictionary<string, List<ChangesetInfo>> categorizedChangesets, InsertBeforeOrAfter lastPart)
        {
            var paragraph = lastPart.CreateSectionWithParagraph("Code Change sets in this Release",
                "The following list of code check-ins to TFS was compiled to make up this release");
            InsertBeforeOrAfter newLastPart = paragraph;
            foreach (var category in categorizedChangesets)
            {
                var p = newLastPart.InsertParagraphAfterSelf(category.Key).FontSize(11d).Heading(HeadingType.Heading2);

                var table = p.InsertTableAfterSelf(category.Value.Count + 2, 4);
                table.SetWidthsPercentage(new[] { 10f, 25, 30f, 35f }, null);
                table.AutoFit = AutoFit.ColumnWidth;
                table.GetCell(0, 0).FillFirstParagraph("TFS").Bold();
                table.GetCell(0, 1).FillFirstParagraph("Developer").Bold();
                table.GetCell(0, 2).FillFirstParagraph("Date/Time").Bold();
                table.GetCell(0, 3).FillFirstParagraph("Description").Bold();

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

        public static Table ThirdSection(IEnumerable<ClientWorkItem> workItems, InsertBeforeOrAfter lastPart)
        {
            var workItemList = workItems.ToList();
            var paragraph = lastPart.CreateSectionWithParagraph("Product reported Defects in this Release",
                "This section gives a list of Client-facing defects that were fixed in this release");
            var table = paragraph.InsertTableAfterSelf(workItemList.Count + 2, 3);
            table.SetWidthsPercentage(new[] { 10f, 75f, 15f }, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0, 0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0, 1).FillFirstParagraph("Work Item Description").Bold();
            table.GetCell(0, 2).FillFirstParagraph("Client Project").Bold();

            for (int i = 0; i < workItemList.Count - 1; i++)
            {
                var item = workItemList[i];
                table.GetCell(i + 1, 0).FillFirstParagraph(item.Id.ToString());
                table.GetCell(i + 1, 1).FillFirstParagraph(item.Title);
                table.GetCell(i + 1, 2).FillFirstParagraph(item.ClientProject);
            }

            return table;
        }

        public static Table FourthSection(IEnumerable<ClientWorkItem> pbi, InsertBeforeOrAfter lastPart)
        {
            var pbiList = pbi.ToList();
            var paragraph = lastPart.CreateSectionWithParagraph("Product Backlog Items in this Release",
                "This section gives a list of PBIs that were delivered in this release");
            var table = paragraph.InsertTableAfterSelf(pbiList.Count + 2, 2);
            table.SetWidthsPercentage(new[] { 25f, 75f }, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0, 0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0, 1).FillFirstParagraph("Work Item Description").Bold();

            for (int i = 0; i < pbiList.Count - 1; i++)
            {
                var item = pbiList[i];
                table.GetCell(i + 1, 0).FillFirstParagraph(item.Id.ToString());
                table.GetCell(i + 1, 1).FillFirstParagraph(item.Title);
            }
            return table;
        }
        public static Table SixthSection(InsertBeforeOrAfter lastPart)
        {
            var paragraph = lastPart.CreateSectionWithParagraph("Known issues in this Release",
                "This section gives a list of bugs that were identified throughout testing of this release");
            var table = paragraph.InsertTableAfterSelf(2, 2);
            table.SetWidthsPercentage(new[] { 25f, 75f }, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.GetCell(0, 0).FillFirstParagraph("Bug Id").Bold();
            table.GetCell(0, 1).FillFirstParagraph("Work Item Description").Bold();

            return table;
        }

        public static Paragraph CreateSectionWithParagraph(this InsertBeforeOrAfter lastPart, string headingTitle, string paragraphText)
        {
            var heading = lastPart.CreateHeadingSection(headingTitle);
            var paragraph = heading.InsertParagraphAfterSelf(paragraphText).FontSize(10d);
            return paragraph;

        }
    }
}
