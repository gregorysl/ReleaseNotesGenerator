using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly WorkItemStore _itemStore;
        private readonly VersionControlServer _changesetServer;
        private TfsTeamProjectCollection _tfsTeamProjectCollection;

        public TfsConnector(string url)
        {
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
            List<string> result = new List<string>();
            string query =
                $"SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.IterationPath] UNDER '{iterationPath}'";
            foreach (WorkItem item in _itemStore.Query(query))
            {
                result.Add(item.Fields["System.Id"].Value + ":" + item.Fields["System.Title"].Value);
            }

            return result;
        }
    }
}
