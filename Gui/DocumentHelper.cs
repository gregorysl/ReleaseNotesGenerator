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
        public static InsertBeforeOrAfter ChangesetsSection(this InsertBeforeOrAfter lastPart, Dictionary<string, List<ChangesetInfo>> categorizedChangesets)
        {
            var paragraph = lastPart.CreateSectionWithParagraph("Code Change sets in this Release",
                "The following list of code check-ins to TFS was compiled to make up this release");
            InsertBeforeOrAfter newLastPart = paragraph;
            foreach (var category in categorizedChangesets)
            {
                var headers = new[] {"TFS", "Developer", "Date/Time", "Description"};
                var columSizes = new[] { 10f, 25, 30f, 35f };
                var p = newLastPart.InsertParagraphAfterSelf(category.Key).FontSize(11d).Heading(HeadingType.Heading2);

                var table = p.InsertTableAfterSelf(category.Value.Count + 1, 4);
                table.SetWidthsPercentage(columSizes, null);
                table.AutoFit = AutoFit.ColumnWidth;
                table.FillRow(0, headers, true);

                for (var i = 0; i < category.Value.Count - 1; i++)
                {
                    var item = category.Value[i];
                    var rowData = new[] {item.Id.ToString(), item.CommitedBy, item.Created.ToString(), item.Comment};
                    table.FillRow(i + 1, rowData);
                }

                newLastPart = table;
            }

            return newLastPart;
        }

        public static Table WorkItemSection(this InsertBeforeOrAfter lastPart, IEnumerable<ClientWorkItem> workItems)
        {
            var headers = new[] { "Bug Id", "Work Item Description", "Client Project" };
            var columSizes = new[] { 10f, 75f, 15f };
            var workItemList = workItems.ToList();
            var paragraph = lastPart.CreateSectionWithParagraph("Product reported Defects in this Release",
                "This section gives a list of Client-facing defects that were fixed in this release");
            var table = paragraph.InsertTableAfterSelf(workItemList.Count + 2, 3);
            table.SetWidthsPercentage(columSizes, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.FillRow(0, headers, true);

            for (var i = 0; i < workItemList.Count - 1; i++)
            {
                var item = workItemList[i];
                var rowData = new[] { item.Id.ToString(), item.Title, item.ClientProject };
                table.FillRow(i + 1, rowData);
            }

            return table;
        }

        public static Table PbiSection(this InsertBeforeOrAfter lastPart, IEnumerable<ClientWorkItem> pbi)
        {
            var headers = new[] { "Bug Id", "Work Item Description" };
            var columSizes = new[] { 25f, 75f };
            var pbiList = pbi.ToList();
            var paragraph = lastPart.CreateSectionWithParagraph("Product Backlog Items in this Release",
                "This section gives a list of PBIs that were delivered in this release");
            var table = paragraph.InsertTableAfterSelf(pbiList.Count + 2, 2);
            table.SetWidthsPercentage(columSizes, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.FillRow(0, headers, true);

            for (var i = 0; i < pbiList.Count - 1; i++)
            {
                var item = pbiList[i];
                var rowData = new[] { item.Id.ToString(), item.Title };
                table.FillRow(i + 1, rowData);
            }
            return table;
        }
        public static Table KnownIssuesSection(this InsertBeforeOrAfter lastPart)
        {
            var headers = new[] { "Bug Id", "Work Item Description" };
            var columSizes = new[] { 25f, 75f };
            var paragraph = lastPart.CreateSectionWithParagraph("Known issues in this Release",
                "This section gives a list of bugs that were identified throughout testing of this release");
            var table = paragraph.InsertTableAfterSelf(2, 2);
            table.SetWidthsPercentage(columSizes, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.FillRow(0, headers, true);
            return table;
        }

        public static Paragraph CreateSectionWithParagraph(this InsertBeforeOrAfter lastPart, string headingTitle, string paragraphText)
        {
            return lastPart.CreateHeadingSection(headingTitle).InsertParagraphAfterSelf(paragraphText).FontSize(10d);
        }

        public static void FillRow(this Table table, int row, string[] data, bool bold = false)
        {
            for (var i = 0; i < data.Length - 1; i++)
            {
                var paragraph = table.GetCell(row, i).FillFirstParagraph(data[i]);
                if (bold) paragraph.Bold();
            }
        }
    }
}
