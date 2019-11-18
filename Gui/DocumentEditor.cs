using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DataModel;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Gui
{
    public class DocumentEditor
    {
        private readonly string _templateName = "Template.docx";
        public string ProcessData(string documentLocaion,ReleaseData data, Dictionary<string, List<ChangesetInfo>> categorizedChangesets,
            IEnumerable<ClientWorkItem> workItems, IEnumerable<ClientWorkItem> pbi)
        {
            try
            {
                var templatePath = Path.Combine(documentLocaion, _templateName);
                var releasePath = Path.Combine(documentLocaion, $"{data.ReleaseName} Patch Release Notes.docx");
                if (!File.Exists(templatePath)) return $"Template file not found in following location {templatePath}";

                using (var doc = DocX.Load(templatePath))
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

                    doc.SaveAs(releasePath);
                }

                Process.Start(releasePath);
                return $"Successfully generated document! You can find it in following location {releasePath}";
            }
            catch (Exception e)
            {
                return e.Message + "\n" + e.StackTrace;
            }
        }
    }
}
