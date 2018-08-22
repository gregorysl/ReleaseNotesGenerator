﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DataModel;
using Xceed.Words.NET;

namespace Gui
{
    public class DocumentEditor
    {
        public void ProcessData(ReleaseData data, Dictionary<string, List<ChangesetInfo>> categorizedChangesets)
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

                var secondSection = doc.Paragraphs.FirstOrDefault(x => x.Text == "Code Change sets in this Release");
                if (secondSection == null) return;
                var paragraph = secondSection.InsertParagraphAfterSelf("The following list of code check-ins to TFS was compiled to make up this release:").FontSize(10d);
                InsertBeforeOrAfter placeholder = paragraph;
                foreach (var category in categorizedChangesets)
                {
                    var p = placeholder.InsertParagraphAfterSelf(category.Key).FontSize(11d).Heading(HeadingType.Heading2);

                    var table = p.InsertTableAfterSelf(2, 6);
                    table.SetWidths(new[] { 100f,150f,150f,200f, 100f, 400f });
                    table.Rows[0].Cells[0].Paragraphs[0].Append("TFS").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("Developer").Bold();
                    table.Rows[0].Cells[2].Paragraphs[0].Append("Date/Time").Bold();
                    table.Rows[0].Cells[3].Paragraphs[0].Append("Description").Bold();
                    table.Rows[0].Cells[4].Paragraphs[0].Append("Work Item").Bold();
                    table.Rows[0].Cells[5].Paragraphs[0].Append("Work Item Description").Bold();

                    var rowPattern = table.Rows[1];
                    rowPattern.Cells[0].Paragraphs[0].Append("{TfsID}");
                    rowPattern.Cells[1].Paragraphs[0].Append("{Dev}");
                    rowPattern.Cells[2].Paragraphs[0].Append("{Date}");
                    rowPattern.Cells[3].Paragraphs[0].Append("{Desc}");
                    rowPattern.Cells[4].Paragraphs[0].Append("{WorkItemId}");
                    rowPattern.Cells[5].Paragraphs[0].Append("{WorkItemTitle}");

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
                    placeholder = table;
                }

                var thirdSection = placeholder.CreateHeadingSection("Product reported Defects in this Release");
                var workItemTable = thirdSection.InsertTableAfterSelf(2, 3);
                workItemTable.SetWidths(new[] { 100f, 1000f, 100f });
                workItemTable.Rows[0].Cells[0].Paragraphs[0].Append("Bug Id").Bold();
                workItemTable.Rows[0].Cells[1].Paragraphs[0].Append("Work Item Description").Bold();
                workItemTable.Rows[0].Cells[2].Paragraphs[0].Append("Client Project").Bold();

                var placeholderRow = workItemTable.Rows[1];
                placeholderRow.Cells[0].Paragraphs[0].Append("{TfsID}");
                placeholderRow.Cells[1].Paragraphs[0].Append("{WorkItemTitle}");
                placeholderRow.Cells[2].Paragraphs[0].Append("{Client}");

                foreach (var item in data.WorkItems)
                {
                    var newItem = workItemTable.InsertRow(placeholderRow, workItemTable.RowCount - 1);

                    newItem.ReplaceText("{TfsID}", item.Id.ToString());
                    newItem.ReplaceText("{WorkItemTitle}", item.Title);
                    newItem.ReplaceText("{Client}", item.ClientProject);

                }
                placeholderRow.Remove();


                var fourthSection = workItemTable.CreateHeadingSection("Product Backlog Items and KTRs in this Release");
                var fifthSection = fourthSection.CreateHeadingSection("Test Report");
                var sixthSection = fifthSection.CreateHeadingSection("Known issues in this Release");

                doc.SaveAs(dTestDocx);
            }

            Process.Start(dTestDocx);
        }
    }
}