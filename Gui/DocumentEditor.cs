using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
                    var secondSection = DocumentHelper.SecondSection(categorizedChangesets, lastParagraph);
                    var thirdSection = DocumentHelper.ThirdSection(workItems, secondSection);
                    var fourthSection = DocumentHelper.FourthSection(pbi, thirdSection);

                    var fifthSection = fourthSection.CreateHeadingSection("Test Report");
                    var sixthSection = DocumentHelper.SixthSection(fifthSection);
                    
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

    }
}
