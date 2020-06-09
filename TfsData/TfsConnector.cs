using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RNA.Model;

namespace TfsData
{
    public class TfsConnector : Connector
    {
        public TfsConnector(ServerDetails settings) : base(settings)
        {
        }

        public override async Task<DownloadedItems> GetChangesetsAsync(ReleaseData data, string apiVersion = "5.1")
        {
            var baseurl = $"{Url}/_apis/tfvc/";
            var queryLocation = $"$/{data.TfsProject}/{data.TfsBranch}";
            var itemQueryUrl = $"{baseurl}items?scopePath={queryLocation}&api-version={apiVersion}&recursionLevel=OneLevel";
            var itemQueryResponse = await Client.GetAsync<Wrapper<Item>>(itemQueryUrl, apiVersion);

            var from = !string.IsNullOrEmpty(data.ChangesetFrom) ? $"&searchCriteria.fromId={data.ChangesetFrom}" : "";
            var to = !string.IsNullOrEmpty(data.ChangesetTo) ? $"&searchCriteria.toId={data.ChangesetTo}" : "";

            var categoryChangesTasks = itemQueryResponse.value.Where(x => x.isFolder && x.path != queryLocation).Select(async category =>
            {
                var urls = $"{baseurl}changesets?searchCriteria.itemPath={category.path}{from}{to}&$top=1000";
                var wrapper = await Client.GetAsync<Wrapper<Change>>(urls, apiVersion);
                return new Tuple<string, List<Change>>(category.path, wrapper.value);
            });
            var categoryChangesResponse = await Task.WhenAll(categoryChangesTasks);

            var changesByCategory = categoryChangesResponse
                .Where(x => x.Item2.Any())
                .OrderBy(x => x.Item1)
                .ToDictionary(x => x.Item1, y => y.Item2.Select(z => z.changesetId).ToList());


            var changesList = categoryChangesResponse
                .SelectMany(x => x.Item2)
                .DistinctBy(x => x.changesetId)
                .OrderByDescending(x => x.checkedInBy.date)
                .ToList();
            var tfsClass = new DownloadedItems { Changes = changesList, Categorized = changesByCategory };
            return tfsClass;

        }
    }
}
