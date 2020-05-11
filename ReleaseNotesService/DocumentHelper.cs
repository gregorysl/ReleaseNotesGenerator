using System.Collections.Generic;
using System.Linq;
using TfsData;
using Xceed.Document.NET;

namespace ReleaseNotesService
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

        public static InsertBeforeOrAfter ChangesetsSection(this InsertBeforeOrAfter lastPart, Dictionary<string, List<ChangesetInfo>> categorizedChangesets)
        {
            var heading = "Code Change sets in this Release";
            var subHeading = "The following list of code check-ins to TFS was compiled to make up this release";
            var paragraph = lastPart.CreateSectionWithParagraph(heading, subHeading);
            InsertBeforeOrAfter newLastPart = paragraph;
            foreach (var category in categorizedChangesets.Where(x => x.Value.Count > 0))
            {
                var headers = new[] { "TFS", "Developer", "Date/Time", "Description" };
                var columnSizes = new[] { 10f, 25, 30f, 35f };
                var p = newLastPart.InsertParagraphAfterSelf(category.Key).FontSize(11d).Heading(HeadingType.Heading2);
                var table = p.CreateTableWithHeader(headers, columnSizes, category.Value.Count + 1);

                for (var i = 0; i < category.Value.Count; i++)
                {
                    var item = category.Value[i];
                    var rowData = new[] { item.Id.ToString(), item.CommitedBy, item.Created.ToString(), item.Comment };
                    table.FillRow(i + 1, rowData);
                }

                newLastPart = table;
            }

            return newLastPart;
        }

        public static Table WorkItemSection(this InsertBeforeOrAfter lastPart, IEnumerable<ClientWorkItem> workItems)
        {
            var workItemList = workItems.ToList();

            var headers = new[] { "Bug Id", "Work Item Description", "Client Project" };
            var columnSizes = new[] { 10f, 75f, 15f };
            var heading = "Product reported Defects in this Release";
            var subHeading = "This section gives a list of Client-facing defects that were fixed in this release";
            var paragraph = lastPart.CreateSectionWithParagraph(heading, subHeading);
            var table = paragraph.CreateTableWithHeader(headers, columnSizes, workItemList.Count + 1);

            for (var i = 0; i < workItemList.Count; i++)
            {
                var item = workItemList[i];
                var rowData = new[] { item.Id.ToString(), item.Title, item.ClientProject };
                table.FillRow(i + 1, rowData);
            }

            return table;
        }

        public static Table PbiSection(this InsertBeforeOrAfter lastPart, IEnumerable<ClientWorkItem> pbi)
        {
            var pbiList = pbi.ToList();

            var headers = new[] { "Bug Id", "Work Item Description" };
            var columnSizes = new[] { 25f, 75f };
            var heading = "Product Backlog Items in this Release";
            var subHeading = "This section gives a list of PBIs that were delivered in this release";
            var paragraph = lastPart.CreateSectionWithParagraph(heading, subHeading);
            var table = paragraph.CreateTableWithHeader(headers, columnSizes, pbiList.Count + 1);

            for (var i = 0; i < pbiList.Count; i++)
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
            var columnSizes = new[] { 25f, 75f };
            var heading = "Known issues in this Release";
            var subHeading = "This section gives a list of bugs that were identified throughout testing of this release";
            var paragraph = lastPart.CreateSectionWithParagraph(heading, subHeading);
            return paragraph.CreateTableWithHeader(headers, columnSizes, 2);
        }

        public static Paragraph CreateSectionWithParagraph(this InsertBeforeOrAfter lastPart, string headingTitle, string paragraphText)
        {
            return lastPart.CreateHeadingSection(headingTitle).InsertParagraphAfterSelf(paragraphText).FontSize(10d);
        }

        public static void FillRow(this Table table, int row, string[] data, bool bold = false)
        {
            for (var i = 0; i < data.Length; i++)
            {
                var paragraph = table.GetCell(row, i).FillFirstParagraph(data[i]);
                if (bold) paragraph.Bold();
            }
        }

        public static Table CreateTableWithHeader(this InsertBeforeOrAfter lastPart, string[] headers,
            float[] columnSizes, int rows)
        {
            rows = rows >= 1 ? rows : 1;
            var table = lastPart.InsertTableAfterSelf(rows, headers.Length);
            table.SetWidthsPercentage(columnSizes, null);
            table.AutoFit = AutoFit.ColumnWidth;
            table.FillRow(0, headers, true);
            return table;
        }
    }
}