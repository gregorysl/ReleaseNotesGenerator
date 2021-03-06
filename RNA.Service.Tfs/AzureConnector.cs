﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RNA.Model;

namespace RNA.Service.Tfs
{
    public class AzureConnector : Connector
    {
        public AzureConnector(ServerDetails settings) : base(settings)
        {
        }

        public override async Task<Tuple<string, List<Change>>[]> GetChangesetsAsync(ReleaseData data,
            string apiVersion = "5.1")
        {
            var baseurl = $"{Url}/{data.TfsProject}/_apis/git/repositories/{data.TfsProject}";

            var itemUrl = $"{baseurl}/items?path=/&versionType=Branch&versionOptions=None&versionDescriptor.version={data.TfsBranch}";
            var itemResponse = await Client.GetAsync<ItemsObject>(itemUrl, apiVersion);

            var treeUrl = $"{baseurl}/trees/{itemResponse.objectId}";
            var treeResponse = await Client.GetAsync<Wrapper<ItemsObject>>(treeUrl, apiVersion);

            var query = new GitQueryCommitsCriteria
            {
                compareVersion = GitVersionFromCommit(data.ChangesetFrom),
                itemVersion = GitVersionFromCommit(data.ChangesetTo)
            };

            var changesUrl = $"{baseurl}/commitsbatch";
            var categoryChangesTasks = treeResponse.treeEntries.Where(x => x.gitObjectType == "tree").Select(async category =>
            {
                var currentQuery = query.CloneJson();
                currentQuery.itemPath = category.relativePath;
                var wrapper = await Client.PostAsync<Wrapper<ChangeAzure>>(changesUrl, currentQuery, apiVersion);
                var mappedData = wrapper.value.Select(x => new Change(x)).ToList();
                return new Tuple<string, List<Change>>(category.relativePath, mappedData);
            });
            var categoryChangesResponse = await Task.WhenAll(categoryChangesTasks);

            return categoryChangesResponse;
        }

        public override async Task<string> TestConnection()
        {
            var baseUrl = $"{Url}/_apis/connectionData?api-version=5.1-preview";
            var response = await Client.GetAsync(baseUrl);
            return response.StatusCode == HttpStatusCode.OK ? "OK" : await response.Content.ReadAsStringAsync();
        }


        private static GitVersion GitVersionFromCommit(string changeset)
        {
            return !string.IsNullOrEmpty(changeset)
                ? new GitVersion { versionType = "commit", version = changeset }
                : null;
        }
    }
}
