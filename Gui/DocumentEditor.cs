using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    doc.ReplaceText("{PsRefreshDate}", data.PsRefresh.createdDate.FormatData());
                    doc.ReplaceText("{PsRefreshName}", data.PsRefresh.comment);
                    doc.ReplaceText("{CoreChangeset}", data.CoreChange.changesetId.ToString());
                    doc.ReplaceText("{CoreDate}", data.CoreChange.createdDate.FormatData());

                    doc.InsertTableOfContents("Contents",
                        TableOfContentsSwitches.O | TableOfContentsSwitches.U | TableOfContentsSwitches.Z | TableOfContentsSwitches.H | TableOfContentsSwitches.T,
                        "FR HeadNoToc");

                    doc.InsertSectionPageBreak();
                    var lastParagraph = doc.Paragraphs[doc.Paragraphs.Count - 1]
                        .ChangesetsSection(categorizedChangesets)
                        .WorkItemSection(workItems)
                        .PbiSection(pbi)
                        .CreateHeadingSection("Test Report")
                        .KnownIssuesSection();

                    doc.SaveAs(dTestDocx);
                }

                Process.Start(dTestDocx);
                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
        }
    }
}
