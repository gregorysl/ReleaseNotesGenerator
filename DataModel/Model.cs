using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;


namespace DataModel
{
    public class ReleaseData : INotifyPropertyChanged
    {
        public bool DownloadButtonEnabled =>
            !string.IsNullOrWhiteSpace(TfsProject) && !string.IsNullOrWhiteSpace(TfsBranch);

        public bool GenerateDocButtonEnabled =>
            !string.IsNullOrWhiteSpace(ReleaseName) && !string.IsNullOrWhiteSpace(QaBuildName) &&
            !string.IsNullOrWhiteSpace(CoreBuildName) && PsRefresh != null && CoreChange != null;

        private ChangesetInfo _psRefresh;
        private ChangesetInfo _coreChange;
        private string _releaseName;
        private string _tfsBranch;
        private string _qaBuildName;
        private string _coreBuildName;
        private string _tfsProject;
        
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

        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        public string ReleaseDateFormated => ReleaseDate.ToString("d-MMMM-yyyy", new CultureInfo("en-US"));

        public string QaBuildName
        {
            get => _qaBuildName;
            set
            {
                _qaBuildName = value;
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }

        public DateTime QaBuildDate { get; set; } = DateTime.Now;

        public string QaBuildDateFormated => QaBuildDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));

        public string CoreBuildName
        {
            get => _coreBuildName;
            set
            {
                _coreBuildName = value;
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }

        public DateTime CoreBuildDate { get; set; } = DateTime.Now;

        public string CoreBuildDateFormated => CoreBuildDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));

        public ChangesetInfo PsRefresh
        {
            get => _psRefresh;
            set
            {
                _psRefresh = value;
                OnPropertyChanged(nameof(PsRefresh));
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }

        public ChangesetInfo CoreChange
        {
            get => _coreChange;
            set
            {
                _coreChange = value;
                OnPropertyChanged(nameof(CoreChange));
                OnPropertyChanged(nameof(GenerateDocButtonEnabled));
            }
        }



        public List<ClientWorkItem> WorkItems { get; set; }
        public List<ChangesetInfo> CategorizedChanges { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ClientWorkItem
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
        public List<string> Categories { get; set; }
        public override string ToString()
        {
            return $"{Id} {Comment} {CommitedBy} {Created.ToShortDateString()} ";
        }
    }
}
