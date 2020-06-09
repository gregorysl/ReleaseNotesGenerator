using System;
using System.Collections.Generic;

namespace RNA.Model
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
        public Dictionary<string, List<string>> Categorized { get; set; } = new Dictionary<string, List<string>>();
        public List<Change> Changes { get; set; } = new List<Change>();
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

    public class Wrapper<T> where T : class
    {
        public List<T> value { get; set; }
        public List<T> treeEntries { get; set; }
    }

    public class Change
    {
        public string changesetId { get; set; }
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
        public string name
        {
            set => displayName = value;
        }

        public DateTime date { get; set; }
        public override string ToString()
        {
            return displayName;
        }
    }


    public class ItemsObject
    {
        public string objectId { get; set; }
        public string relativePath { get; set; }
        public string gitObjectType { get; set; }
        public string commitId { get; set; }
        public string path { get; set; }
        public bool isFolder { get; set; }
        public string url { get; set; }
    }




    public class ChangeAzure
    {
        public string commitId { get; set; }
        public Author author { get; set; }
        public string comment { get; set; }
        public DateTime createdDate { get; set; }
        public List<Workitem> workItems { get; set; }
    }





    public class GitQueryCommitsCriteria
    {
        public GitVersion itemVersion { get; set; }
        public GitVersion compareVersion { get; set; }
        public bool includeWorkItems { get; set; }
        public string itemPath { get; set; }
    }

    public class GitVersion
    {
        public string versionType { get; set; }
        public string version { get; set; }
    }

}
