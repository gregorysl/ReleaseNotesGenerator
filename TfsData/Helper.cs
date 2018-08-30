using System;
using System.Collections.Generic;
using System.Linq;
using DataModel;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using ClientWorkItem = DataModel.ClientWorkItem;

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
        public static List<ClientWorkItem> ToClientWorkItems(this IEnumerable<Artifact> artifacts)
        {
            if (artifacts == null)
                return new List<ClientWorkItem>();
            var clientWorkItems = new List<ClientWorkItem>();
            foreach (var artifact in artifacts)
            {
                if (artifact == null) continue;

                var workItem = new ClientWorkItem();
                foreach (var extendedAttribute in artifact.ExtendedAttributes)
                {
                    if (string.Equals(extendedAttribute.Name, "System.Id", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(extendedAttribute.Value, out var result))
                            workItem.Id = result;
                    }
                    else if (string.Equals(extendedAttribute.Name, "System.Title", StringComparison.OrdinalIgnoreCase))
                        workItem.Title = extendedAttribute.Value;
                }
                if (workItem.Id != 0)
                    clientWorkItems.Add(workItem);
            }
            return clientWorkItems;
        }
    }

}
