using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly WorkItemStore _itemStore;
        private readonly VersionControlServer _changesetServer;
        private readonly TfsTeamProjectCollection _tfsTeamProjectCollection;
        private string _workItemsForIteration = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private string _workItemsByIds = "SELECT * FROM WorkItems WHERE [System.Id] in ({0})";

        public TfsConnector(string url)
        {
            //var versionSpecFrom = changesetFrom.IsNullOrEmpty()
            //    ? new ChangesetVersionSpec(1)
            //    : new ChangesetVersionSpec(changesetFrom);

            //var versionSpecTo = changesetTo.IsNullOrEmpty()
            //    ? VersionSpec.Latest
            //    : new ChangesetVersionSpec(changesetTo);

            //var excludedDirs = new[] { "CodeAnalysis", "QA", "_AutomatedBuild" };
            //var categories = new[] { "UIFramework", "Framework", "Infrastructure", "WebApp", "PSSolution" };

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
            return _changesetServer.GetChangeset(id).Comment;
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

        public List<string> GetWorkItemsInIterationPath(string iterationPath)
        {
            var workItems = _itemStore.Query(string.Format(_workItemsForIteration, iterationPath)).Cast<WorkItem>().ToList();
            var result = new List<string>();
            foreach (var item in workItems)
            {
                result.Add(item.Fields["System.Id"].Value + ":" + item.Fields["System.Title"].Value);
            }

            return result;
        }

        public List<Changeset> GetChangesets(string queryLocation, VersionSpec versionSpecFrom, VersionSpec versionSpecTo)
        {
            var list = _changesetServer.QueryHistory(queryLocation, VersionSpec.Latest, 0, RecursionType.Full, null, versionSpecFrom,
                versionSpecTo, int.MaxValue, true, false).OfType<Changeset>().OrderBy(x => x.ChangesetId).ToList();
            return list.Where(x => x.CommitterDisplayName != "TFS Service").ToList();
        }

        public List<Model.WorkItemDetails> GetWorkItemsFromChangesets(List<Changeset> changes)
        {
            var workItemIds = changes.SelectMany(x => x.AssociatedWorkItems).ToList().Select(x => x.Id.ToString()).Distinct().ToList();
            var joinedWorkItems = string.Join(",", workItemIds);
            var workItems = _itemStore.Query(String.Format(_workItemsByIds, joinedWorkItems));

            var workItemChanges = new List<Model.WorkItemDetails>();
            foreach (WorkItem workItem in workItems)
            {
                workItemChanges.Add(new Model.WorkItemDetails
                {
                    Id = workItem.Id,
                    Title = workItem.Title,
                    ClientProject = workItem.Fields["client.project"].Value.ToString()
                });
            }

            return workItemChanges;
        }


        public void GetCategorizedChanges(List<Changeset> changes, List<string> categories)
        {

            //var categoryChangesList = new List<Model.CategoryChanges>();

            //foreach (var category in categories)
            //{
            //    var categoryChanges = new Model.CategoryChanges { Name = category };
            //    var categoryPath = $"$/{tfsProjectName}/{tfsBranchName}/{category}";
            //    var catList = changes.Where(x => x.Changes.Any(c => c.Item.ServerItem.Contains(categoryPath)));
            //    foreach (var item in catList)
            //    {
            //        var changesetInfo = new Model.ChangesetInfo
            //        {
            //            Id = item.ChangesetId,
            //            CommitedBy = item.CommitterDisplayName,
            //            Created = item.CreationDate,
            //            Comment = item.Comment
            //        };
            //        var workItemWithoutCodeReview = item.AssociatedWorkItems.Where(x => x.WorkItemType != "Code Review Request").ToList();
            //        if (!workItemWithoutCodeReview.Any())
            //        {
            //            changesetInfo.WorkItemId = "N/A";
            //            changesetInfo.WorkItemTitle = "N/A";

            //            categoryChanges.Changes.Add(changesetInfo);
            //        }
            //        foreach (var info in workItemWithoutCodeReview)
            //        {

            //            changesetInfo.WorkItemId = info.Id.ToString();
            //            changesetInfo.WorkItemTitle = info.Title;
            //            categoryChanges.Changes.Add(changesetInfo);
            //        }
            //    }

            //    if (categoryChanges.Changes.Count != 0)
            //    {
            //        categoryChangesList.Add(categoryChanges);
            //    }
            //}
        }
    }
}
