using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using DataModel;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly WorkItemStore _itemStore;
        private readonly TfsTeamProjectCollection _tfsTeamProjectCollection;
        private readonly string _workItemsForIteration = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private readonly string _workItemsByIds = "SELECT * FROM WorkItems WHERE [System.Id] in ({0})";
        private static HttpClient client;
        private string uri;

        public TfsConnector(string url, string username, string key)
        {
            uri = url;
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, key))));

            _tfsTeamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(url, UriKind.Absolute));
            _itemStore = _tfsTeamProjectCollection.GetService<WorkItemStore>();
        }

        public bool IsConnected => _tfsTeamProjectCollection.HasAuthenticated;

        //https://secure.fenergo.com/tfs/Product/_apis/projects
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

                using (HttpResponseMessage response = client.GetAsync($"{uri}/_apis/tfvc/changesets/{id}?api-version=1.0").Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    var changeset = JsonConvert.DeserializeObject<DataModel.Change>(responseBody);
                    return changeset.comment;
                }
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


        public List<WorkItem> QueryWorkItems(string query, string parameter)
        {
            return _itemStore.Query(string.Format(query, parameter)).Cast<WorkItem>().ToList();
        }

        public List<ClientWorkItem> GetWorkItemsByIdAndIteration(string joinedWi, string iterationPath, List<string> workItemStateInclude, List<string> workItemTypeExclude)
        {
            var changesetItems = QueryWorkItems(_workItemsByIds, joinedWi);
            var iterationPathItems = QueryWorkItems(_workItemsForIteration, iterationPath);
            var allItems = new List<WorkItem>();
            allItems.AddRange(changesetItems);
            allItems.AddRange(iterationPathItems);

            var clientWorkItems = allItems.DistinctBy(x => x.Id)
                .Where(x => !workItemTypeExclude.Contains(x.Type.Name))
                .Where(x => workItemStateInclude.Contains(x.State))
                .Select(workItem => workItem.ToClientWorkItem())
                .OrderBy(x => x.ClientProject)
                .ThenBy(x => x.Id).ToList();

            return clientWorkItems;
        }

        public tfs GetChangesetsRest(string queryLocation, string changesetFrom, string changesetTo,
            List<string> categories)
        {
            var tfs = new tfs();
            var versionSpecFromi = "&searchCriteria.fromId=" + (changesetFrom.IsNullOrEmpty()
                ? "1"
                : changesetFrom);
            var versionSpecTois = changesetTo.IsNullOrEmpty()
                ? ""
                : $"&searchCriteria.toId={changesetTo}";


            var categoryQueryLocation = categories.Select(x => new Tuple<string, string>(x, $"{queryLocation}/{x}")).ToList();
            var list = new List<DataModel.Change>();
            foreach (var tuple in categoryQueryLocation)
            {
                using (HttpResponseMessage response = client.GetAsync($"{uri}/_apis/tfvc/changesets?searchCriteria.itemPath={tuple.Item2}{versionSpecFromi}{versionSpecTois}&api-version=1.0").Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    var deserializedList = JsonConvert.DeserializeObject<DataModel.TfsData<DataModel.Change>>(responseBody).value;
                    list.AddRange(deserializedList);
                    tfs.Categorized.Add(tuple.Item1, deserializedList.Select(x => x.changesetId).ToList());
                }
            }
            var changesList = list.DistinctBy(x => x.changesetId).OrderBy(x => x.changesetId).ToList();
            tfs.Changes = new ObservableCollection<DataModel.Change>(changesList);
            return tfs;

        }

        public List<Work> GetChangesetWorkItemsRest(DataModel.Change change)
        {
            using (HttpResponseMessage response = client.GetAsync(change.url + "/workItems").Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var list = JsonConvert.DeserializeObject<DataModel.TfsData<DataModel.Work>>(responseBody).value;
                return list;
            }
        }

    }
}
