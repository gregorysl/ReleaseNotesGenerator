using System.Linq;
using DataModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TfsData
{
    public static class Helper
    {
        private static readonly string ClientProjectField = "client.project";
        public static string GetClientProject(this WorkItem item)
        {
            var fieldExists = item.Fields.Contains(ClientProjectField);
            return fieldExists ? item.Fields[ClientProjectField].Value.ToString() : "";
        }
        public static ClientWorkItem ToClientWorkItem(this WorkItem item)
        {
            return new ClientWorkItem
            {
                Id = item.Id,
                Title = item.Title,
                ClientProject = item.GetClientProject()
            };
        }

    }
}
