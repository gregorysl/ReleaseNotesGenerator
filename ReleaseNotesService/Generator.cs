using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TfsData;

namespace ReleaseNotesService
{
    public class Generator
    {
        private readonly TfsConnector _connector;

        public Generator(TfsConnector connector)
        {
            _connector = connector;
        }

        public string CreateDoc(DownloadedItems downloadedData, Change psRefresh, List<string> workItemStateInclude, ReleaseData releaseData, string documentLocation, string testReport = "")
        {
            var selectedChangesets = downloadedData.Changes
                .Where(x => x.Selected)
                .OrderBy(x => x.changesetId)
                .Select(item => new ChangesetInfo
                {
                    Id = item.changesetId,
                    Comment = item.comment,
                    CommitedBy = item.checkedInBy.displayName,
                    Created = item.createdDate
                }).ToList();

            var categories = downloadedData.Categorized.ToDictionary(
                    category => category.Key,
                    category => selectedChangesets.Where(x => category.Value.Contains(x.Id)).ToList()).Where(x=>x.Value.Any()).ToList();

            var workItems = downloadedData.WorkItems.Where(x => workItemStateInclude.Contains(x.State) && x.ClientProject != null)
                .OrderBy(x => x.ClientProject);
            var pbi = downloadedData.WorkItems.Where(x => workItemStateInclude.Contains(x.State) && x.ClientProject == null)
                .OrderBy(x => x.Id);
            var message = new DocumentEditor().ProcessData(documentLocation, psRefresh, releaseData, categories, workItems, pbi, testReport);
            return message;
        }

        public DownloadedItems DownloadData(ReleaseData data, bool includeTfsService = false)
        {
            var queryLocation = $"$/{data.TfsProject}/{data.TfsBranch}";
            var downloadedData = _connector.GetChangesetsRest(queryLocation, data.ChangesetFrom, data.ChangesetTo);
            downloadedData.FilterTfsChanges(includeTfsService);
            var reg = new Regex(@".*\[((\d*\,)*?(\d*))\].*");
            var changesetWorkItemsId = downloadedData.Changes.Where(x => !string.IsNullOrWhiteSpace(x.comment) &&  reg.Match(x.comment).Success)
                .Select(x => reg.Match(x.comment).Groups[1].Captures[0].Value).Select(x => x.Split(',')).SelectMany(x => x)
                .Select(x => Convert.ToInt32(x)).ToList();
            downloadedData.WorkItems = _connector.GetWorkItems(data.Iteration, changesetWorkItemsId);
            return downloadedData;
        }
    }
}