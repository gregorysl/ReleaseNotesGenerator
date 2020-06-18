using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RNA.Model;

namespace TfsData
{
    public abstract class Connector
    {
        private const string WorkItemsForIteration = "SELECT [System.Id] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        protected readonly string Url;
        protected HttpClient Client;
        public abstract Task<Tuple<string, List<Change>>[]> GetChangesetsAsync(ReleaseData data,
            string apiVersion = "5.1");

        public abstract Task<string> TestConnection();

        protected Connector(ServerDetails settings)
        {
            Url = settings.Url; 
            Client = SetupClient(settings.Pat);
        }

        public HttpClient SetupClient(string key)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{key}")));
            return client;
        }

        public async Task<string> TestIteration(string project, string iteration, string apiVersion = "5.1-preview")
        {
            var baseUrl = $"{Url}/_apis/wit/";
            var response = await Client.PostAsJsonAsync($"{baseUrl}wiql?api-version={apiVersion}", new { query = string.Format(WorkItemsForIteration, iteration) });
            var json = await response.Content.ReadAsStringAsync();
            var workItemsResponse = JsonConvert.DeserializeObject<Rootobject>(json);
            if (workItemsResponse?.workItems==null)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorObject>(json);
                return errorResponse.message;
            }
            
            return "OK";
        }

        public async Task<List<ClientWorkItem>> GetWorkItems(string iterationPath, List<string> additional, string apiVersion = "5.1")
        {
            var baseUrl = $"{Url}/_apis/wit/";
            var response = await Client.PostAsync<Rootobject>($"{baseUrl}wiql", new { query = string.Format(WorkItemsForIteration, iterationPath) }, apiVersion);
            var ids = response.workItems.Select(x => x.id).ToList();
            ids.AddRange(additional);
            if (!ids.Any()) return new List<ClientWorkItem>();

            var joinedWorkItems = string.Join(",", ids.Distinct().ToList());
            var changesets = await Client.GetAsync<Wrapper<WrappedWi>>($"{baseUrl}WorkItems?ids={joinedWorkItems}", apiVersion);
            changesets.value.ForEach(x => x.fields.Id = x.id);
            var changeset = changesets.value.Select(x => x.fields).ToList();


            var clientWorkItems = changeset.DistinctBy(x => x.Id)
                .OrderBy(x => x.ClientProject)
                .ThenBy(x => x.Id).ToList();

            return clientWorkItems;
        }

    }
}
