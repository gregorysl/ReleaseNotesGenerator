using System;
using System.Collections.Generic;


namespace DataModel
{

    public class WorkItemDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ClientProject { get; set; }
    }
    public class CategoryChanges
    {
        public string Name { get; set; }
        public List<ChangesetInfo> Changes { get; set; } = new List<ChangesetInfo>();
    }

    public class ChangesetInfo
    {
        public int Id { get; set; }
        public string CommitedBy { get; set; }
        public DateTime Created { get; set; }
        public string Comment { get; set; }
        public string WorkItemId { get; set; }
        public string WorkItemTitle { get; set; }
        public override string ToString()
        {
            return $"{Id}-{WorkItemTitle}-{WorkItemId}";
        }
    }
}
