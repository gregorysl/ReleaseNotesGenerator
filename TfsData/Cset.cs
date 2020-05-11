using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TfsData
{

    public class WrappedWi : Workitem
    {
        public ClientWorkItem fields { get; set; }
    }

    public class Rootobject
    {
        public Workitem[] workItems { get; set; }
    }

    public class Workitem
    {
        public int id { get; set; }
    }

    public class DownloadedItems
    {
        public Dictionary<string, List<int>> Categorized { get; set; } = new Dictionary<string, List<int>>();
        public ObservableCollection<Change> Changes { get; set; } = new ObservableCollection<Change>();
        public List<ClientWorkItem> WorkItems { get; set; }

    }

    public class Project
    {
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
        public string name { get; set; }
        public Iteration[] children { get; set; }
    }

    public class DataWrapper<T> where T : class
    {
        public List<T> value { get; set; }
    }

    public class Change
    {
        public int changesetId { get; set; }
        public Author checkedInBy { get; set; }
        public DateTime createdDate { get; set; }
        public string comment { get; set; }
        public bool Selected { get; set; } = true;

        public override string ToString()
        {
            return $"{changesetId} {comment} {checkedInBy.displayName} {createdDate.ToShortDateString()}";
        }
    }

    public class Author
    {
        public string displayName { get; set; }
        public override string ToString()
        {
            return displayName;
        }
    }
}
