using System;
using System.Collections.Generic;

namespace DataModel
{
    public class tfs
    {
        public Dictionary<string, List<int>> categorized { get; set; } = new Dictionary<string, List<int>>();
        public List<Change> changes { get; set; } = new List<Change>();
    }
    class CSet
    {
    }
    public class ChangeList
    {
        public int count { get; set; }
        public List<Change> value { get; set; }
    }

    public class Change
    {
        public int changesetId { get; set; }
        public string url { get; set; }
        public Author author { get; set; }
        public Checkedinby checkedInBy { get; set; }
        public DateTime createdDate { get; set; }
        public string comment { get; set; }
        public bool commentTruncated { get; set; }
        public bool Selected { get; set; } = true;
    }

    public class Author
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }

        public override string ToString()
        {
            return displayName;
        }
    }

    public class Checkedinby
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
    }

}
