using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RNA.Model;

namespace TfsData
{
    public class TfsConnector
    {
        private readonly string _workItemsForIteration = "SELECT [System.Id] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        private static HttpClient _changesetsClient;
        private static HttpClient _workItemClient;
        private readonly string _tfsurl;
        private readonly string _adourl;

        public TfsConnector(ServerDetails tfsSettings, ServerDetails azureSettings)
        {
            _tfsurl = tfsSettings.Url;
            _adourl = azureSettings.Url;
            _changesetsClient = SetupClient(tfsSettings.Pat);
            _workItemClient = SetupClient(azureSettings.Pat);
        }

        public HttpClient SetupClient(string key)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{key}")));
            return client;
        }

        public List<ClientWorkItem> GetWorkItems(string iterationPath, List<int> additional, string apiVersion = "5.1")
        {
            var response = _workItemClient.PostWithResponse<Rootobject>($"{_adourl}/_apis/wit/wiql?api-version={apiVersion}", new { query = string.Format(_workItemsForIteration, iterationPath) });
            var ids = response.workItems.Select(x => x.id).ToList();
            ids.AddRange(additional);
            if (!ids.Any()) return new List<ClientWorkItem>();

            var joinedWorkItems = string.Join(",", ids.Distinct().ToList());
            var changesets = _workItemClient.GetWithResponse<DataWrapper<WrappedWi>>($"{_adourl}/_apis/wit/WorkItems?ids={joinedWorkItems}&api-version={apiVersion}");
            changesets.value.ForEach(x => x.fields.Id = x.id);
            var changeset = changesets.value.Select(x => x.fields).ToList();


            var clientWorkItems = changeset.DistinctBy(x => x.Id)
                .OrderBy(x => x.ClientProject)
                .ThenBy(x => x.Id).ToList();

            return clientWorkItems;
        }

        public DownloadedItems GetChangesetsRest(string queryLocation, ReleaseData data, string apiVersion = "5.1")
        {
            var cats = _changesetsClient
                .GetWithResponse<DataWrapper<Item>>(
                    $"{_tfsurl}/_apis/tfvc/items?scopePath={queryLocation}&api-version={apiVersion}&recursionLevel=OneLevel")
                .value.Where(x => x.isFolder && x.path != queryLocation).ToList();

            var tfsClass = new DownloadedItems();
            string from = !string.IsNullOrEmpty(data.ChangesetFrom) ? "&searchCriteria.fromId=" + data.ChangesetFrom : "";
            string to = !string.IsNullOrEmpty(data.ChangesetTo) ? "&searchCriteria.toId=" + data.ChangesetTo : "";
            var list = new List<Change>();
            Parallel.ForEach(cats, category =>
            {
                var itemPath = $"searchCriteria.itemPath={category.path}";
                var response = _changesetsClient
                    .GetWithResponse<DataWrapper<Change>>(
                        $"{_tfsurl}/_apis/tfvc/changesets?{itemPath}{from}{to}&$top=1000&api-version=1.0").value;
                if (!response.Any()) return;
                list.AddRange(response);
                tfsClass.Categorized.Add(category.path.Replace(queryLocation, "").Trim('/'), response.Select(x => x.changesetId).ToList());

            });
            var changesList = list.DistinctBy(x => x.changesetId).OrderByDescending(x => x.changesetId).ToList();
            tfsClass.Changes = changesList;
            return tfsClass;

        }
    }
}
