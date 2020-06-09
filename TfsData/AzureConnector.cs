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
            var objectId = Client.GetWithResponse<Wrapper<ItemsObject>>(
                $"{baseurl}/items?recursionLevel=oneLevel&api-version={apiVersion}").value.First().objectId;

            var ur = $"{baseurl}/trees/{objectId}?api-version={apiVersion}";

            var cats = Client.GetWithResponse<Wrapper<ItemsObject>>(ur).treeEntries.Where(x => x.gitObjectType == "tree").ToList();

            var query = new GitQueryCommitsCriteria();
            if (!string.IsNullOrEmpty(data.ChangesetFrom))
            {
                query.compareVersion = new GitVersion { versionType = "commit", version = data.ChangesetFrom };
            }
            if (!string.IsNullOrEmpty(data.ChangesetTo))
            {
                query.itemVersion = new GitVersion { versionType = "commit", version = data.ChangesetTo };
            }

            var url = $"{baseurl}/commitsbatch?api-version={apiVersion}";
            var tasks = cats.Select(async category =>
            {
                var currentQuery = query.CloneJson();
                currentQuery.itemPath = category.relativePath;
                var wrapper = await Client.PostWithResponseAsync<Wrapper<ChangeAzure>>(url, currentQuery);
                return new Tuple<string, Wrapper<ChangeAzure>>(category.relativePath, wrapper);
            });
            var tasksResponse = await Task.WhenAll(tasks);
            var categorizedDictionary = tasksResponse.Where(x => x.Item2.value.Any())
                .ToDictionary(x => x.Item1, y => y.Item2.value.Select(z => z.commitId).ToList());
            var changesList = tasksResponse.SelectMany(x => x.Item2.value).ToList().DistinctBy(x => x.commitId)
                .OrderByDescending(x => x.commitId).Select(x => new Change
                {
                    comment = x.comment,
                    checkedInBy = x.author,
                    createdDate = x.author.date,
                    changesetId = x.commitId
                }).ToList();
            var tfsClass = new DownloadedItems { Categorized = categorizedDictionary, Changes = changesList };
            return tfsClass;
        }
    }
}
