using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RNA.Model;

namespace TfsData
{
    public class AzureConnector : Connector
    {
        public AzureConnector(ServerDetails settings, IMapper mapper) : base(settings, mapper)
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
                var mappedData = Mapper.Map<List<ChangeAzure>,List<Change>>(wrapper.value);
                return new Tuple<string, List<Change>>(category.relativePath, mappedData);
            });
            var categoryChangesResponse = await Task.WhenAll(categoryChangesTasks);

            return categoryChangesResponse;
        }

        private static GitVersion GitVersionFromCommit(string changeset)
        {
            return !string.IsNullOrEmpty(changeset)
                ? new GitVersion { versionType = "commit", version = changeset }
                : null;
        }
    }
}
