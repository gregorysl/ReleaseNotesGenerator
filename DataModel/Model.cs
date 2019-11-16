using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace DataModel
{
    public class ReleaseData : INotifyPropertyChanged
    {
        [JsonIgnore]
        public string ErrorMessgage { get; set; }
        [JsonIgnore]
        public bool DownloadButtonEnabled =>
            !string.IsNullOrWhiteSpace(TfsProject) && !string.IsNullOrWhiteSpace(TfsBranch);

        [JsonIgnore]
        public bool GenerateDocButtonEnabled =>
            !string.IsNullOrWhiteSpace(ReleaseName) &&
            !string.IsNullOrWhiteSpace(QaBuildName) &&
            PsRefresh != null &&
            CoreChange != null;

        [JsonIgnore]
        public tfs tfs {get;set;} = new tfs();

        private Change _psRefresh;
        private Change _coreChange;
        private string _releaseName;
        private string _tfsBranch;
        private string _qaBuildName;
        private string _tfsProject;
        private bool _workItemsDownloaded;

        public string TfsProject
        {
            get => _tfsProject;
            set
            {
                _tfsProject = value;
                OnPropertyChanged(nameof(TfsProject));
                OnPropertyChanged(nameof(DownloadButtonEnabled));
            }
        }

        public string TfsBranch { get => _tfsBranch;
            set
            {
                _tfsBranch= value;
                OnPropertyChanged(nameof(TfsBranch));
                OnPropertyChanged(nameof(DownloadButtonEnabled));
            }
        }
        public string ProjectSelected { get; set; }
        public string IterationSelected { get; set; }
        public string ChangesetFrom { get; set; }
        public bool ChangesetFromInclude { get; set; }
        public string ChangesetTo { get; set; }
        public bool ChangesetToInclude { get; set; }

        public string ReleaseName
        {
            get => _releaseName;
            set
            {
                _releaseName = value;
                OnPropertyChanged(nameof(ReleaseName));
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }

        public string ReleaseDate { get; set; }

        public string QaBuildName
        {
            get => _qaBuildName;
            set
            {
                _qaBuildName = value;
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }

        public string QaBuildDate { get; set; }
        
        [JsonIgnore]
        public Change PsRefresh
        {
            get => _psRefresh;
            set
            {
                _psRefresh = value;
                OnPropertyChanged(nameof(PsRefresh));
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }
        [JsonIgnore]
        public Change CoreChange
        {
            get => _coreChange;
            set
            {
                _coreChange = value;
                OnPropertyChanged(nameof(CoreChange));
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }






        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ClientWorkItem
    {
        [JsonProperty(PropertyName = "System.Id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "System.Title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "System.State")]
        public string State { get; set; }

        [DefaultValue("N/A")]
        [JsonProperty(PropertyName = "client.project", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ClientProject { get; set; }

        [DefaultValue("N/A")]
        [JsonProperty("Custom.14b2b676-45e8-46bf-891e-b9f5dbaebce0", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ClientProject2 { get; set; }

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
