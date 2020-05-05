using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataModel;
using TfsData;

namespace ReleaseNotesService
{
    public class Generator
    {
        private readonly TfsConnector _tfs;

        public Generator(TfsConnector tfs)
        {
            _tfs = tfs;
        }

        public string CreateDoc(DownloadedItems downloadedData, List<string> workItemStateInclude, ReleaseData releaseData, string documentLocation)
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

            var categories = downloadedData.Categorized
                .Where(category => selectedChangesets.Any(x => category.Value.Contains(x.Id))).ToDictionary(
                    category => category.Key,
                    category => selectedChangesets.Where(x => category.Value.Contains(x.Id)).ToList());

            var workItems = downloadedData.WorkItems.Where(x => workItemStateInclude.Contains(x.State) && x.ClientProject != null)
                .OrderBy(x => x.ClientProject);
            var pbi = downloadedData.WorkItems.Where(x => workItemStateInclude.Contains(x.State) && x.ClientProject == null)
                .OrderBy(x => x.Id);
            var message = new DocumentEditor().ProcessData(documentLocation, releaseData, categories, workItems, pbi);
            return message;
        }

        public async Task<DownloadedItems> DownloadData(string tfsProject, string branch, string changesetFrom, string changesetTo, string iteration,
            bool includeTfsService = false)
        {
            var queryLocation = $"$/{tfsProject}/{branch}";
            var downloadedData = await Task.Run(() => _tfs.GetChangesetsRest(queryLocation, changesetFrom, changesetTo));
            downloadedData.FilterTfsChanges(includeTfsService);
            var reg = new Regex(@".*\[((\d*\,)*?(\d*))\].*");
            var changesetWorkItemsId = downloadedData.Changes.Where(x => reg.Match(x.comment).Success)
                .Select(x => reg.Match(x.comment).Groups[1].Captures[0].Value).Select(x => x.Split(',')).SelectMany(x => x)
                .Select(x => Convert.ToInt32(x)).ToList();
            downloadedData.WorkItems = _tfs.GetWorkItemsAdo(iteration, changesetWorkItemsId);
            return downloadedData;
        }
    }
}