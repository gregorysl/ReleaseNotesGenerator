using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly string _workItemsForIteration = "SELECT [System.Id] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private static HttpClient _tfs;
        private static HttpClient _ado;
        private readonly string _tfsurl;
        private readonly string _adourl;

        public TfsConnector(TfsSettings tfsSettings, TfsSettings azureSettings)
        {
            _tfsurl = tfsSettings.Url;
            _adourl = azureSettings.Url;
            _tfs = SetupClient(tfsSettings.Pat);
            _ado = SetupClient(azureSettings.Pat);
        }

        public HttpClient SetupClient(string key)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{key}")));
            return client;
        }

        public List<string> Projects()
        {
            var tfsData = _ado.GetWithResponse<DataWrapper<Project>>(($"{_adourl}/_apis/projects?$top=999"));
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

        public List<ClientWorkItem> GetWorkItemsAdo(string iterationPath, List<int> additional)
        {
            var response = _ado.PostWithResponse<Rootobject>($"{_adourl}/_apis/wit/wiql?api-version=5.1", new { query = string.Format(_workItemsForIteration, iterationPath) });
            var ids = response.workItems.Select(x => x.id).ToList();
            ids.AddRange(additional);
            if (!ids.Any()) return new List<ClientWorkItem>();

            var joinedWorkItems = string.Join(",", ids.Distinct().ToList());
            var changesets = _ado.GetWithResponse<DataWrapper<WrappedWi>>($"{_adourl}/_apis/wit/WorkItems?ids={joinedWorkItems}&api-version=5.1");
            changesets.value.ForEach(x => x.fields.Id = x.id);
            var changeset = changesets.value.Select(x => x.fields).ToList();


            var clientWorkItems = changeset.DistinctBy(x => x.Id)
                .OrderBy(x => x.ClientProject)
                .ThenBy(x => x.Id).ToList();

            return clientWorkItems;
        }

        public DownloadedItems GetChangesetsRest(string queryLocation, string changesetFrom, string changesetTo)
        {
            var cats = _tfs
                .GetWithResponse<DataWrapper<Item>>(
                    $"{_tfsurl}/_apis/tfvc/items?scopePath={queryLocation}&api-version=3.1&recursionLevel=OneLevel")
                .value.Where(x => x.isFolder && x.path != queryLocation).ToList();

            var tfsClass = new DownloadedItems();
            string from = !string.IsNullOrEmpty(changesetFrom) ? "&searchCriteria.fromId=" + changesetFrom : "";
            string to = !string.IsNullOrEmpty(changesetTo) ? "&searchCriteria.toId=" + changesetTo : "";
            var list = new List<Change>();
            Parallel.ForEach(cats, category =>
            {
                var itemPath = $"searchCriteria.itemPath={category.path}";
                var response = _tfs
                    .GetWithResponse<DataWrapper<Change>>(
                        $"{_tfsurl}/_apis/tfvc/changesets?{itemPath}{from}{to}&$top=1000&api-version=1.0").value;
                if (!response.Any()) return;
                list.AddRange(response);
                tfsClass.Categorized.Add(category.path.Replace(queryLocation,"").Trim('/'), response.Select(x => x.changesetId).ToList());

            });
            var changesList = list.DistinctBy(x => x.changesetId).OrderByDescending(x => x.changesetId).ToList();
            tfsClass.Changes = new ObservableCollection<Change>(changesList);
            return tfsClass;

        }
    }
}
