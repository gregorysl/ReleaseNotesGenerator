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

    public class Work
    {
        public string webUrl { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string workItemType { get; set; }
        public string state { get; set; }
    }

    public class TfsData<T> where T : class
    {
        public int count { get; set; }
        public List<T> value { get; set; }
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
        public List<Work> Works { get; set; } = new List<Work>();
        public int wokcount => Works.Count;
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
