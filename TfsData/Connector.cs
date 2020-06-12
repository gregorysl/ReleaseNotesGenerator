﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using RNA.Model;

namespace TfsData
{
    public abstract class Connector
    {
        private const string WorkItemsForIteration = "SELECT [System.Id] FROM WorkItems WHERE [System.IterationPath] UNDER '{0}'";
        protected readonly string Url;
        protected HttpClient Client;
        protected readonly IMapper Mapper;
        public abstract Task<DownloadedItems> GetChangesetsAsync(ReleaseData data, string apiVersion = "5.1");

        protected Connector(ServerDetails settings, IMapper mapper)
        {
            Url = settings.Url; 
            Client = SetupClient(settings.Pat);
            Mapper = mapper;
        }

        public HttpClient SetupClient(string key)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($":{key}")));
            return client;
        }

        public List<ClientWorkItem> GetWorkItems(string iterationPath, List<string> additional, string apiVersion = "5.1")
        {
            var baseUrl = $"{Url}/_apis/wit/";
            var response = Client.PostWithResponse<Rootobject>($"{baseUrl}wiql?api-version={apiVersion}", new { query = string.Format(WorkItemsForIteration, iterationPath) });
            var ids = response.workItems.Select(x => x.id).ToList();
            ids.AddRange(additional);
            if (!ids.Any()) return new List<ClientWorkItem>();

            var joinedWorkItems = string.Join(",", ids.Distinct().ToList());
            var changesets = Client.GetWithResponse<Wrapper<WrappedWi>>($"{baseUrl}WorkItems?ids={joinedWorkItems}&api-version={apiVersion}");
            changesets.value.ForEach(x => x.fields.Id = x.id);
            var changeset = changesets.value.Select(x => x.fields).ToList();


            var clientWorkItems = changeset.DistinctBy(x => x.Id)
                .OrderBy(x => x.ClientProject)
                .ThenBy(x => x.Id).ToList();

            return clientWorkItems;
        }

    }
}
