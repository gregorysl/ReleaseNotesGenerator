using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataModel;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly WorkItemStore _itemStore;
        private readonly VersionControlServer _changesetServer;
        private readonly TfsTeamProjectCollection _tfsTeamProjectCollection;
        private readonly ILinking _linkingServer;
        private readonly string _workItemsForIteration = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private readonly string _workItemsByIds = "SELECT * FROM WorkItems WHERE [System.Id] in ({0})";


        public TfsConnector(string url)
        {
            //var excludedDirs = new[] { "CodeAnalysis", "QA", "_AutomatedBuild" };

            _tfsTeamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(url, UriKind.Absolute));
            _itemStore = _tfsTeamProjectCollection.GetService<WorkItemStore>();
            _changesetServer = _tfsTeamProjectCollection.GetService<VersionControlServer>();
            _linkingServer = _tfsTeamProjectCollection.GetService<ILinking>();
        }

        public bool IsConnected => _tfsTeamProjectCollection.HasAuthenticated;

        public List<string> Projects => _itemStore.Projects.Cast<Project>().Select(proj => proj.Name).ToList();

        public ICollection<string> GetIterationPaths(string projectName)
        {
            var result = new List<string>();
            var project = _itemStore.Projects.Cast<Project>().FirstOrDefault(x => x.Name == projectName);

            if (project == null) return result;

            foreach (Node node in project.IterationRootNodes)
            {
                result.Add(node.Path);
                RecursiveAddIterationPath(node, result);
            }
            return result;
        }

        public string GetChangesetTitleById(int id)
        {
            try
            {
                return _changesetServer.GetChangeset(id).Comment;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        private static void RecursiveAddIterationPath(Node node, ICollection<string> result)
        {
            foreach (Node item in node.ChildNodes)
            {
                result.Add(item.Path);
                if (item.HasChildNodes)
                {
                    RecursiveAddIterationPath(item, result);
                }
            }
        }

        public List<ClientWorkItem> GetWorkItemsInIterationPath(string iterationPath, List<string> stateFilter)
        {
            var workItemsFromQuery = _itemStore.Query(string.Format(_workItemsForIteration, iterationPath))
                .Cast<WorkItem>()
                .Where(x => x.Type.ToString() != "Code Review Request" && x.Type.ToString() != "Task").ToList()
                .Where(x => stateFilter.Contains(x.State)).ToList();

            var workItems = workItemsFromQuery.Select(workItem => workItem.ToClientWorkItem()).ToList();

            return workItems;
        }

        public ReleaseData GetChangesetsAndWorkItems(string iterationPath, string queryLocation, string changesetFrom,
            string changesetTo, List<string> categories, List<string> stateFilter)
        {
            var changes = GetChangesets(queryLocation, changesetFrom, changesetTo);
            var workItems = GetWorkItemsFromChangesets(changes, stateFilter);
            var workItems2 = GetWorkItemsInIterationPath(iterationPath,stateFilter);

            workItems2.AddRange(workItems.Where(x => !workItems2.Exists(w => w.Id == x.Id)));

            var allWorkItems = workItems2.OrderBy(x => x.ClientProject).ThenBy(x => x.Id).ToList();


            var categorized = GetChangesWithWorkItemsAndCategories(queryLocation, changes, categories, allWorkItems);
            var releaseData = new ReleaseData
            {
                CategorizedChanges = new ObservableCollection<ChangesetInfo>(categorized),
                WorkItems = allWorkItems
            };

            return releaseData;
        }

        public List<Changeset> GetChangesets(string queryLocation, string changesetFrom, string changesetTo)
        {
            var versionSpecFrom = changesetFrom.IsNullOrEmpty()
                ? new ChangesetVersionSpec(1)
                : new ChangesetVersionSpec(changesetFrom);
            var versionSpecTo = changesetTo.IsNullOrEmpty()
                ? VersionSpec.Latest
                : new ChangesetVersionSpec(changesetTo);
            var list = _changesetServer.QueryHistory(queryLocation, VersionSpec.Latest, 0, RecursionType.Full, null, versionSpecFrom,
                versionSpecTo, int.MaxValue, true, false).OfType<Changeset>().OrderBy(x => x.ChangesetId).ToList();
            return list;
        }

        public List<ClientWorkItem> GetWorkItemsFromChangesets(List<Changeset> changes, List<string> stateFilter)
        {
            var asd = changes.Select(x => x.ArtifactUri.ToString()).ToList();
            var linkFilters = new[]
            {
                new LinkFilter
                {
                    FilterType = FilterType.ToolType,
                    FilterValues = new[] {"WorkItemTracking"}
                }
            };
            var artifacts = _linkingServer.GetReferencingArtifacts(asd.ToArray(), linkFilters);
            var workItemInfos = artifacts.ToClientWorkItems();
            var workItemIds = workItemInfos.Select(x => x.Id).Distinct().ToList();
               
            var joinedWorkItems = string.Join(",", workItemIds);
            var workItems = _itemStore.Query(string.Format(_workItemsByIds, joinedWorkItems)).Cast<WorkItem>().ToList()
                .Where(x => x.Type.ToString() != "Code Review Request" && x.Type.ToString() != "Task").ToList()
                .Where(x => stateFilter.Contains(x.State)).ToList();

            return workItems.Select(workItem => workItem.ToClientWorkItem()).ToList();
        }


        public List<ChangesetInfo> GetChangesWithWorkItemsAndCategories(string queryLocation, List<Changeset> changes,
            List<string> categories, List<ClientWorkItem> allWorkItems)
        {
            var workItems = new List<ClientWorkItem>();

            var categoryQueryLocation = categories.Select(x => new Tuple<string, string>(x, $"{queryLocation}/{x}")).ToList();
            var changesets = new List<ChangesetInfo>();

            foreach (var change in changes)
            {
                var changeLocations = change.Changes.Select(x => x.Item.ServerItem).ToList();
                var changeCategories = categoryQueryLocation.Where(x => changeLocations.Any(c => c.StartsWith(x.Item2))).Select(x => x.Item1).ToList();
                var changesetInfo = new ChangesetInfo
                {
                    Id = change.ChangesetId,
                    CommitedBy = change.CommitterDisplayName,
                    Created = change.CreationDate,
                    Comment = change.Comment,
                    Categories = changeCategories
                };
                var workItemList = change.AssociatedWorkItems.Where(x => x.WorkItemType != "Code Review Request" && x.WorkItemType != "Task").Select(x=>x.Id).ToList();
                var workItemWithoutCodeReview = allWorkItems.Where(x => workItemList.Contains(x.Id)).ToList();
                if (!workItemWithoutCodeReview.Any())
                {
                    changesetInfo.WorkItemId = "N/A";
                    changesetInfo.WorkItemTitle = "N/A";

                    changesets.Add(changesetInfo);
                }
                foreach (var info in workItemWithoutCodeReview)
                {
                    changesetInfo.WorkItemId = info.Id.ToString();
                    changesetInfo.WorkItemTitle = info.Title;
                    changesets.Add(changesetInfo);
                }

            }
            return changesets;
        }
    }
}
