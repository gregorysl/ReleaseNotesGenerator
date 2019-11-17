using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DataModel;
using Newtonsoft.Json;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly string _workItemsForIteration = "SELECT [System.Id] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private static HttpClient _tfs;
        private static HttpClient _ado;
        private readonly string _tfsurl;
        private readonly string _adourl;

        public TfsConnector(string tfsuri, string tfsusername, string tfskey, string adouri, string adousername, string adokey)
        {
            _tfsurl = tfsuri;
            _adourl = adouri;
            _tfs = SetupClient(_tfsurl, tfsusername, tfskey);
            _ado = SetupClient(_adourl, adousername, adokey);
        }

        public HttpClient SetupClient(string url, string username, string key)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{key}")));
            return client;
        }

        public List<string> Projects()
        {
            var tfsData = _ado.GetWithResponse<TfsData<Project>>(($"{_adourl}/_apis/projects?$top=999"));
            var projects = tfsData.value.Select(x => x.name).OrderBy(x => x).ToList();
            return projects;
        }

        public ICollection<string> GetIterationPaths(string projectName)
        {
            var iterationData = _ado.GetWithResponse<Iteration>($"{_adourl}/{projectName}/_apis/wit/classificationNodes/Iterations?$depth=5");
            var iterations = new List<string>();
            getI(iterationData, "", iterations);
            return iterations;
        }

        private void getI(Iteration iteration, string v, List<string> list)
        {
            v = v + iteration.name + "\\";
            list.Add(v.TrimEnd('\\'));
            iteration.children?.OrderBy(x => x.name).ToList().ForEach(x => getI(x, v, list));
        }

        public string GetChangesetTitleById(int id)
        {
            try
            {
                var changeset = _tfs.GetWithResponse<Change>($"{_tfsurl}/_apis/tfvc/changesets/{id}?api-version=1.0");
                return changeset.comment;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public List<ClientWorkItem> GetWorkItemsAdo(string iterationPath)
        {
            var response = _ado.PostWithResponse<Rootobject>($"{_adourl}/_apis/wit/wiql?api-version=5.1", new { query = string.Format(_workItemsForIteration, iterationPath) });
            var ids = response.workItems.Select(x => x.id).ToList();
            if (!ids.Any()) return new List<ClientWorkItem>();

            var joinedWorkItems = string.Join(",", ids.Distinct().ToList());
            var changesets = _ado.GetWithResponse<TfsData<WrappedWi>>($"{_adourl}/_apis/wit/WorkItems?ids={joinedWorkItems}&api-version=5.1");
            changesets.value.ForEach(x => x.fields.Id = x.id);
            var changeset = changesets.value.Select(x => x.fields).ToList();


            var clientWorkItems = changeset.DistinctBy(x => x.Id)
                .OrderBy(x => x.ClientProject)
                .ThenBy(x => x.Id).ToList();

            return clientWorkItems;
        }

        public tfs GetChangesetsRest(string queryLocation, string changesetFrom, string changesetTo,
            List<string> categories)
        {
            var tfsClass = new tfs();
            var versionSpecFromi = "&searchCriteria.fromId=" + (string.IsNullOrEmpty(changesetFrom)
                ? "1"
                : changesetFrom);
            var versionSpecTois = string.IsNullOrEmpty(changesetTo)
                ? ""
                : $"&searchCriteria.toId={changesetTo}";


            var categoryQueryLocation = categories.Select(x => new Tuple<string, string>(x, $"{queryLocation}/{x}")).ToList();
            var list = new List<Change>();
            foreach (var tuple in categoryQueryLocation)
            {
                var response = _tfs.GetWithResponse<TfsData<Change>>($"{_tfsurl}/_apis/tfvc/changesets?searchCriteria.itemPath={tuple.Item2}{versionSpecFromi}{versionSpecTois}&api-version=1.0").value;
                list.AddRange(response);
                tfsClass.Categorized.Add(tuple.Item1, response.Select(x => x.changesetId).ToList());
            }
            var changesList = list.DistinctBy(x => x.changesetId).OrderByDescending(x => x.changesetId).ToList();
            tfsClass.Changes = new ObservableCollection<Change>(changesList);
            return tfsClass;

        }

        public List<Work> GetChangesetWorkItemsRest(Change change)
        {
            using (var response = _tfs.GetAsync(change.url + "/workItems").Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var list = JsonConvert.DeserializeObject<TfsData<Work>>(responseBody).value;
                return list;
            }
        }

    }
}
