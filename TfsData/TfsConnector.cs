using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using DataModel;
using Newtonsoft.Json;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly string _workItemsForIteration = "SELECT [System.Id] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private static HttpClient client;
        private string uri;

        public TfsConnector(string url, string username, string key)
        {
            uri = url;
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, key))));
        }


        public List<string> Projects()
        {
            var tfsData = client.GetWithResponse<TfsData<Project>>(($"{uri}/_apis/projects?$top=999"));
            var projects = tfsData.value.Select(x => x.name).OrderBy(x => x).ToList();
            return projects;
        }

        public ICollection<string> GetIterationPaths(string projectName)
        {
            var iterationData = client.GetWithResponse<Iteration>($"{uri}/{projectName}/_apis/wit/classificationNodes/Iterations?$depth=5");
            var iterations = new List<string>();
            getI(iterationData, "", iterations);
            return iterations;
        }

        private void getI(Iteration iteration, string v, List<string> list)
        {
            v = v + iteration.name + "\\";
            list.Add(v.TrimEnd('\\'));
            if (iteration.children != null)
            {
                iteration.children.OrderBy(x => x.name).ToList().ForEach(x => getI(x, v, list));
            }
        }

        public string GetChangesetTitleById(int id)
        {
            try
            {
                var changeset = client.GetWithResponse<Change>($"{uri}/_apis/tfvc/changesets/{id}?api-version=1.0");
                return changeset.comment;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public List<ClientWorkItem> GetWorkItemsByIdAndIteration(List<int> workitemsId, string iterationPath, List<string> workItemTypeExclude)
        {
            var response = client.PostWithResponse<Rootobject>($"{uri}/_apis/wit/wiql?api-version=1.0", new { query = string.Format(_workItemsForIteration, iterationPath) });
            var ids = response.workItems.Select(x => x.id).ToList();
            workitemsId.AddRange(ids);

            var joinedWorkItems = string.Join(",", workitemsId.Distinct().ToList());
            var changesets = client.GetWithResponse<TfsData<WrappedWi>>($"{uri}/_apis/wit/WorkItems?ids={joinedWorkItems}&fields=System.Id,System.WorkItemType,System.Title,System.State,client.project&api-version=1.0");
            var changeset = changesets.value.Select(x => x.fields).ToList();


            var clientWorkItems = changeset.DistinctBy(x => x.Id)
                .Where(x => !workItemTypeExclude.Contains(x.SystemWorkItemType))
                .Select(x => x.ToClientWorkItem())
                .OrderBy(x => x.ClientProject)
                .ThenBy(x => x.Id).ToList();

            return clientWorkItems;
        }

        public tfs GetChangesetsRest(string queryLocation, string changesetFrom, string changesetTo,
            List<string> categories)
        {
            var tfs = new tfs();
            var versionSpecFromi = "&searchCriteria.fromId=" + (string.IsNullOrEmpty(changesetFrom)
                ? "1"
                : changesetFrom);
            var versionSpecTois = string.IsNullOrEmpty(changesetTo)
                ? ""
                : $"&searchCriteria.toId={changesetTo}";


            var categoryQueryLocation = categories.Select(x => new Tuple<string, string>(x, $"{queryLocation}/{x}")).ToList();
            var list = new List<DataModel.Change>();
            foreach (var tuple in categoryQueryLocation)
            {
                var response = client.GetWithResponse<TfsData<Change>>($"{uri}/_apis/tfvc/changesets?searchCriteria.itemPath={tuple.Item2}{versionSpecFromi}{versionSpecTois}&api-version=1.0").value;
                list.AddRange(response);
                tfs.Categorized.Add(tuple.Item1, response.Select(x => x.changesetId).ToList());
            }
            var changesList = list.DistinctBy(x => x.changesetId).OrderByDescending(x => x.changesetId).ToList();
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
