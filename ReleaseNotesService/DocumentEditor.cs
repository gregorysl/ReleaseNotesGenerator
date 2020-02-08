using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DataModel;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace ReleaseNotesService
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
                    
                    var headers = new[] { "Item", "Details","Date" };
                    var location = $"$/{data.TfsProject}/{data.TfsBranch}";
                    var columnSizes = new[] { 25f, 45, 30f };
                    var tableWithHeader = doc.Paragraphs[doc.Paragraphs.Count - 1]
                        .CreateTableWithHeader(headers, columnSizes, 4);
                    
                    tableWithHeader.FillRow(1, new []{"Core Changeset",data.CoreChange.changesetId.ToString(),data.CoreChange.createdDate.FormatData()});
                    tableWithHeader.FillRow(2, new []{"PS Refresh Changeset",data.PsRefresh.changesetId.ToString(),data.PsRefresh.createdDate.FormatData()});
                    tableWithHeader.FillRow(3, new []{"QA Build",data.QaBuildName,data.QaBuildDate});

                    var fi = tableWithHeader.InsertParagraphAfterSelf($"This release will be available in ")
                        .Append(location, new Formatting {Bold = true})
                        .Append($" branch using the label ")
                        .Append($"{data.ReleaseName}", new Formatting {Bold = true});
                    
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