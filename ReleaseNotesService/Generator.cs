using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using RNA.Model;
using TfsData;

namespace ReleaseNotesService
{
    public class Generator
    {
        private readonly Regex _regex = new Regex(@".*\[((\d*\,)*?(\d*))\].*", RegexOptions.Compiled);
        private readonly Connector _changesetConnector;
        private readonly Connector _workItemConnector;
        public Generator(Connector changesetConnector, Connector workItemConnector)
        {
            _changesetConnector = changesetConnector;
            _workItemConnector = workItemConnector;
        }

        public string CreateDoc(DownloadedItems downloadedData, Change psRefresh, List<string> workItemStateInclude,
        ReleaseData releaseData, string documentLocation, string testReport = "")
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
                    category => selectedChangesets.Where(x => category.Value.Contains(x.Id)).ToList())
                    .Where(x => x.Value.Any()).ToList();

            var workItems = downloadedData.WorkItems
            .Where(x => workItemStateInclude.Contains(x.State) && x.ClientProject != null)
                .OrderBy(x => x.ClientProject);
            var pbi = downloadedData.WorkItems
            .Where(x => workItemStateInclude.Contains(x.State) && x.ClientProject == null)
                .OrderBy(x => x.Id);
            var message = new DocumentEditor().ProcessData(documentLocation, psRefresh, releaseData, categories,
            workItems, pbi, testReport);
            return message;
        }

        public async Task<DownloadedItems> DownloadData(ReleaseData data, bool includeTfsService = false)
        {
            //var downloadedData = await _changesetConnector.GetChangesetsAsync(data);
            var categoryChanges = await _changesetConnector.GetChangesetsAsync(data,"3.1");

            var changesByCategory = categoryChanges
                .Where(x => x.Item2.Any())
                .OrderBy(x => x.Item1)
                .ToDictionary(x => x.Item1, y => y.Item2.Select(z => z.changesetId).ToList());

            var changesList = categoryChanges
                .SelectMany(x => x.Item2)
                .DistinctBy(x => x.changesetId)
                .OrderByDescending(x => x.checkedInBy.date)
                .ToList();
            var downloadedData = new DownloadedItems { Categorized = changesByCategory, Changes = changesList };

            downloadedData.FilterTfsChanges(includeTfsService);

            var changesetWorkItemsId = downloadedData.Changes
                .Where(x => !string.IsNullOrWhiteSpace(x.comment) && _regex.Match(x.comment).Success)
                .Select(x => _regex.Match(x.comment).Groups[1].Captures[0].Value)
                .Select(x => x.Split(','))
                .SelectMany(x => x)
                .Select(x => x).ToList();

            downloadedData.WorkItems = _workItemConnector.GetWorkItems(data.Iteration, changesetWorkItemsId);
            return downloadedData;
        }
    }
}