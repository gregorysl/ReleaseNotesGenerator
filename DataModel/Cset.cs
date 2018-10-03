using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataModel
{



    public class WrappedWi
    {
        public int id { get; set; }
        public int rev { get; set; }
        public Fields fields { get; set; }
        public string url { get; set; }
    }

    public class Fields
    {
        [JsonProperty(PropertyName = "System.Id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "System.WorkItemType")]
        public string SystemWorkItemType { get; set; }
        
        [JsonProperty(PropertyName = "System.State")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "System.Title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "client.project")]
        public string ClientProject { get; set; }
        public override string ToString()
        {
            return $"{Id} {Title} {ClientProject}";
        }
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

    public class tfs
    {
        public Dictionary<string, List<int>> Categorized { get; set; } = new Dictionary<string, List<int>>();
        public ObservableCollection<Change> Changes { get; set; } = new ObservableCollection<Change>();
        public List<ClientWorkItem> WorkItems { get; set; }

    }
    class CSet
    {
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class Iteration
    {
        public int id { get; set; }
        public string name { get; set; }
        public Iteration[] children { get; set; }
        public string url { get; set; }
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
        public List<int> Works { get; set; } = new List<int>();
        public int wokcount => Works.Count;

        public override string ToString()
        {
            return $"{changesetId} {comment} {checkedInBy.displayName} {createdDate.ToShortDateString()} WIC{wokcount} ";
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

    public class Checkedinby
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string uniqueName { get; set; }
        public string url { get; set; }
        public string imageUrl { get; set; }
    }

}
