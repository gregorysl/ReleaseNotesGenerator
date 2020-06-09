using System;
using System.Linq;
using System.Threading.Tasks;
using RNA.Model;

namespace TfsData
{
    public class AzureConnector : Connector
    {
        public AzureConnector(ServerDetails settings) : base(settings)
        {
        }

        public override async Task<DownloadedItems> GetChangesetsAsync(ReleaseData data, string apiVersion = "5.1")
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
                return new Tuple<string, Wrapper<ChangeAzure>>(category.relativePath, wrapper);
            });
            var categoryChangesResponse = await Task.WhenAll(categoryChangesTasks);

            var changesByCategory = categoryChangesResponse.Where(x => x.Item2.value.Any())
                .ToDictionary(x => x.Item1, y => y.Item2.value.Select(z => z.commitId).ToList());

            var changesList = categoryChangesResponse.SelectMany(x => x.Item2.value).ToList().DistinctBy(x => x.commitId)
                .OrderByDescending(x => x.commitId).Select(x => new Change
                {
                    comment = x.comment,
                    checkedInBy = x.author,
                    createdDate = x.author.date,
                    changesetId = x.commitId
                }).ToList();
            var tfsClass = new DownloadedItems { Categorized = changesByCategory, Changes = changesList };
            return tfsClass;
        }

        private static GitVersion GitVersionFromCommit(string changeset)
        {
            return !string.IsNullOrEmpty(changeset)
                ? new GitVersion { versionType = "commit", version = changeset }
                : null;
        }
    }
}
