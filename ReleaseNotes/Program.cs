using Microsoft.TeamFoundation.Client;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace ReleaseNotes
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = ConfigurationManager.AppSettings;
            var tfsUrl = settings["url"];
            var colletionName = settings["collection"];
            var tfsProjectName = settings["project"];
            var tfsBranchName = settings["branch"];
            var changesetFrom = settings["changesetFrom"];
            var changesetTo = settings["changesetTo"];

            var queryLocation = $"$/{tfsProjectName}/{tfsBranchName}";
            var versionSpecFrom = changesetFrom.IsNullOrEmpty()
                ? new ChangesetVersionSpec(1)
                : new ChangesetVersionSpec(changesetFrom);

            var versionSpecTo = changesetTo.IsNullOrEmpty()
                ? VersionSpec.Latest
                : new ChangesetVersionSpec(changesetTo);

            var path = $"c:\\PatchNotes\\{tfsBranchName}\\";
            Directory.CreateDirectory(path);

            var excludedDirs = new[] {"CodeAnalysis", "QA", "_AutomatedBuild"};
            var categories = new[] { "UIFramework", "Framework", "Infrastructure", "WebApp", "PSSolution" };

            var configurationServer = TfsConfigurationServerFactory.GetConfigurationServer(new Uri(tfsUrl));
            var collectionId = FindCollectionId(configurationServer, colletionName);

            TfsTeamProjectCollection collection = configurationServer.GetTeamProjectCollection(collectionId);
            var svc = collection.GetService<VersionControlServer>();

            var list = svc.QueryHistory(queryLocation, VersionSpec.Latest, 0, RecursionType.Full, null, versionSpecFrom,
                versionSpecTo, Int32.MaxValue, true, false).Cast<Changeset>().OrderBy(x => x.ChangesetId).ToList();
            var changes = list.Where(x => x.CommitterDisplayName != "TFS Service")
                .OrderBy(x => x.ChangesetId).ToList();
            foreach (var category in categories)
            {
                var categorySb = new StringBuilder();
                var categoryPath = $"$/{tfsProjectName}/{tfsBranchName}/{category}";
                var catList = changes.Where(x => x.Changes.Any(c => c.Item.ServerItem.Contains(categoryPath)));
                foreach (var item in catList)
                {
                    var changesetInfo = $"{item.ChangesetId},{item.CommitterDisplayName},{item.CreationDate},{item.Comment}";
                    var workItemWithoutCodeReview = item.AssociatedWorkItems.Where(x => x.WorkItemType != "Code Review Request").ToList();
                    if (!workItemWithoutCodeReview.Any())
                    {
                        categorySb.Append($"{changesetInfo},N/A,N/A\n");
                    }
                    foreach (var info in workItemWithoutCodeReview)
                    {
                        categorySb.Append($"{changesetInfo},{info.Id},{info.Title}\n");
                    }

                }

                if (categorySb.Length != 0)
                {
                    var savePath = $"{path}\\{category}_{DateTime.Now.ToFileTime()}.csv";
                    using (var writetext = new StreamWriter(savePath))
                    {
                        writetext.Write(categorySb.ToString());
                    }
                }
            }

            var workItems = changes.SelectMany(x => x.WorkItems).ToList().Where(x=>x.Type.Name != "Code Review Request").GroupBy(x=>x.Id).Select(x=>x.First()).OrderBy(x=>x.Id).ToList();
            //WorkItemStore workItemStore = new WorkItemStore(collection);
            //workItemStore.
            //var sv1c = collection.GetService<Microsoft.TeamFoundation.>();
            var asddadddddd = workItems.Where(HasClientProject).OrderBy(x=> x.Fields["client.project"].Value).ToList();

            var projectItemsSb = new StringBuilder();
            asddadddddd.ForEach(x => projectItemsSb.Append($"{x.Id} {x.Title} {x.Fields["client.project"].Value}\n"));
            using (StreamWriter writetext = new StreamWriter($"{path}\\ProjectItems_{DateTime.Now.ToFileTime()}.csv"))
            {
                writetext.Write(projectItemsSb.ToString());
            }
            var asdasda = 6;
        }

        private static bool HasClientProject(WorkItem x)
        {
            foreach (Field field in x.Fields)
            {
                if (field.ReferenceName == "client.project")
                    return true;
            }

            return false;
        }

        private static Guid FindCollectionId(TfsConfigurationServer configurationServer, string colletionName)
        {
            var nodes = configurationServer.CatalogNode.QueryChildren(null, false, CatalogQueryOptions.None);
            var node = nodes.First(x => x.Resource.DisplayName == colletionName);
            var id = new Guid(node.Resource.Properties["InstanceId"]);
            return id;
        }

        //public static bool FilterExcludedDirs(Change[] list)
        //{
        //    var excludedDirs = new[] { "QA", "_AutomatedBuild"};
        //    return list.Any(x => excludedDirs.Any(x.Item.ServerItem.Contains));
        //}
    }
}
