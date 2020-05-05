using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace TfsData
{
    public class ReleaseData
    {
        public string TfsProject { get; set; }
        public string TfsBranch { get; set; }
        public string ProjectSelected { get; set; }
        public string IterationSelected { get; set; }
        public string ChangesetFrom { get; set; }
        public string ChangesetTo { get; set; }
        public string ReleaseName { get; set; }
        public string ReleaseDate { get; set; }
        public string QaBuildName { get; set; }
        public string QaBuildDate { get; set; }
    }

    public class ClientWorkItem
    {
        [JsonProperty(PropertyName = "System.Id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "System.Title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "System.State")]
        public string State { get; set; }
        [JsonProperty(PropertyName = "Custom.ClientProject", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ClientProject { get; set; }
        [JsonProperty(PropertyName = "System.WorkItemType")]
        public string WorkItemType { get; set; }
        [JsonProperty(PropertyName = "System.BoardColumn")]
        public string BoardColumn { get; set; }
        public override string ToString()
        {
            return $"{Id} {Title} {ClientProject} {State}";
        }
    }

    public class ChangesetInfo : INotifyPropertyChanged
    {
        private bool _selected = true;

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        public int Id { get; set; }
        public string CommitedBy { get; set; }
        public DateTime Created { get; set; }
        [DefaultValue("")]
        public string Comment { get; set; }
        public List<string> Categories { get; set; }
        public override string ToString()
        {
            return $"{Id} {Comment} {CommitedBy} {Created.ToShortDateString()} ";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
