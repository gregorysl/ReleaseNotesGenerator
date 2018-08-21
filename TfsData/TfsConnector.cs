using System;
using System.Collections.Generic;
using System.Linq;
using DataModel;
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
        private readonly string _workItemsForIteration = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private readonly string _workItemsByIds = "SELECT * FROM WorkItems WHERE [System.Id] in ({0})";


        public TfsConnector(string url)
        {
            //var excludedDirs = new[] { "CodeAnalysis", "QA", "_AutomatedBuild" };

            _tfsTeamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(url, UriKind.Absolute));
            _itemStore = _tfsTeamProjectCollection.GetService<WorkItemStore>();
            _changesetServer = _tfsTeamProjectCollection.GetService<VersionControlServer>();
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

        public List<ClientWorkItem> GetWorkItemsInIterationPath(string iterationPath)
        {
            var workItemsFromQuery = _itemStore.Query(string.Format(_workItemsForIteration, iterationPath)).Cast<WorkItem>().ToList();
            var workItems = new List<ClientWorkItem>();
            foreach (var workItem in workItemsFromQuery)
            {
                workItems.Add(new ClientWorkItem
                {
                    Id = workItem.Id,
                    Title = workItem.Title,
                    ClientProject = workItem.Fields["client.project"].Value.ToString()
                });
            }

            return workItems;
        }

        public ReleaseData GetChangesetsAndWorkItems(string iterationPath, string queryLocation, string changesetFrom,
            string changesetTo, List<string> categories, List<string> stateFilter)
        {
            var changes = GetChangesets(queryLocation, changesetFrom, changesetTo);
            var workItems = GetWorkItemsFromChangesets(changes, stateFilter);
            var workItems2 = GetWorkItemsInIterationPath(iterationPath);

            workItems2.AddRange(workItems.Where(x => !workItems2.Exists(w => w.Id == x.Id)));

            var categorized = GetCategorizedChanges(queryLocation, changes, categories);
            var releaseData = new ReleaseData
            {
                CategorizedChanges = categorized,
                WorkItems = workItems2
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
            return list.Where(x => x.CommitterDisplayName != "TFS Service").ToList();
        }

        public List<ClientWorkItem> GetWorkItemsFromChangesets(List<Changeset> changes, List<string> stateFilter)
        {
            var workItemIds = changes.SelectMany(x => x.AssociatedWorkItems)
                .Where(x => x.WorkItemType != "Code Review Request").ToList()
                .Where(x => stateFilter.Contains(x.State))
                .Select(x => x.Id.ToString()).Distinct().ToList();
            var joinedWorkItems = string.Join(",", workItemIds);
            var workItems = _itemStore.Query(string.Format(_workItemsByIds, joinedWorkItems)).Cast<WorkItem>();

            var workItemChanges = new List<ClientWorkItem>();
            foreach (var workItem in workItems)
            {
                workItemChanges.Add(new ClientWorkItem
                {
                    Id = workItem.Id,
                    Title = workItem.Title,
                    ClientProject = workItem.Fields["client.project"].Value.ToString()
                });
            }

            return workItemChanges;
        }


        public List<CategoryChanges> GetCategorizedChanges(string queryLocation, List<Changeset> changes, List<string> categories)
        {

            var categoryChangesList = new List<CategoryChanges>();

            foreach (var category in categories)
            {
                var categoryChanges = new CategoryChanges { Name = category };
                var categoryPath = $"{queryLocation}/{category}";
                var catList = changes.Where(x => x.Changes.Any(c => c.Item.ServerItem.Contains(categoryPath))).OrderBy(x => x.ChangesetId);
                foreach (var item in catList)
                {
                    var changesetInfo = new ChangesetInfo
                    {
                        Id = item.ChangesetId,
                        CommitedBy = item.CommitterDisplayName,
                        Created = item.CreationDate,
                        Comment = item.Comment
                    };
                    var workItemWithoutCodeReview = item.AssociatedWorkItems.Where(x => x.WorkItemType != "Code Review Request").ToList();
                    if (!workItemWithoutCodeReview.Any())
                    {
                        changesetInfo.WorkItemId = "N/A";
                        changesetInfo.WorkItemTitle = "N/A";

                        categoryChanges.Changes.Add(changesetInfo);
                    }
                    foreach (var info in workItemWithoutCodeReview)
                    {
                        changesetInfo.WorkItemId = info.Id.ToString();
                        changesetInfo.WorkItemTitle = info.Title;
                        categoryChanges.Changes.Add(changesetInfo);
                    }
                }

                if (categoryChanges.Changes.Count != 0)
                {
                    categoryChangesList.Add(categoryChanges);
                }
            }

            return categoryChangesList;
        }
    }
}
