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
            var queryLocation = $"$/{data.TfsProject}/{data.TfsBranch}";
            var cats = Client
                .GetWithResponse<Wrapper<Item>>(
                    $"{Url}/_apis/tfvc/items?scopePath={queryLocation}&api-version={apiVersion}&recursionLevel=OneLevel")
                .value.Where(x => x.isFolder && x.path != queryLocation).ToList();

            var tfsClass = new DownloadedItems();
            string from = !string.IsNullOrEmpty(data.ChangesetFrom) ? "&searchCriteria.fromId=" + data.ChangesetFrom : "";
            string to = !string.IsNullOrEmpty(data.ChangesetTo) ? "&searchCriteria.toId=" + data.ChangesetTo : "";
            var list = new List<Change>();
            Parallel.ForEach(cats, category =>
            {
                var itemPath = $"searchCriteria.itemPath={category.path}";
                var response = Client
                    .GetWithResponse<Wrapper<Change>>(
                        $"{Url}/_apis/tfvc/changesets?{itemPath}{from}{to}&$top=1000&api-version=1.0").value;
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
