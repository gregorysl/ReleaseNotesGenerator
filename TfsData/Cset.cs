using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TfsData
{



    public class WrappedWi
    {
        public int id { get; set; }
        public int rev { get; set; }
        public ClientWorkItem fields { get; set; }
        public string url { get; set; }
    }



    public class Rootobject
    {
        public Workitem[] workItems { get; set; }
    }

    public class Workitem
    {
        public int id { get; set; }
        public string url { get; set; }
    }

    public class DownloadedItems
    {
        public Dictionary<string, List<int>> Categorized { get; set; } = new Dictionary<string, List<int>>();
        public ObservableCollection<Change> Changes { get; set; } = new ObservableCollection<Change>();
        public List<ClientWorkItem> WorkItems { get; set; }

    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class Item
    {
        public string path { get; set; }
        public bool isFolder { get; set; }
        public override string ToString()
        {
            return $"{path} folder:{isFolder}";
        }
    }
    public class Iteration
    {
        public int id { get; set; }
        public string name { get; set; }
        public Iteration[] children { get; set; }
        public string url { get; set; }
    }

    public class DataWrapper<T> where T : class
    {
        public int count { get; set; }
        public List<T> value { get; set; }
    }

    public class Change
    {
        public int changesetId { get; set; }
        public string url { get; set; }
        public Author author { get; set; }
        public Author checkedInBy { get; set; }
        public DateTime createdDate { get; set; }
        public string comment { get; set; }
        public bool Selected { get; set; } = true;
        public List<int> Works { get; set; } = new List<int>();

        public override string ToString()
        {
            return $"{changesetId} {comment} {checkedInBy.displayName} {createdDate.ToShortDateString()}";
        }
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
}
