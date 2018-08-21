using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;


namespace DataModel
{
    public class ReleaseData : INotifyPropertyChanged
    {
        private string _url;

        public string Url
        {
            get => _url;
            set
            {
                _url = value;

                OnPropertyChanged(nameof(Url));
            }
        }

        public string ProjectSelected { get; set; }
        public string TfsProject { get; set; }
        public string TfsBranch { get; set; }
        public string IterationSelected { get; set; }
        public string ChangesetFrom { get; set; }
        public bool ChangesetFromInclude { get; set; }
        public string ChangesetTo { get; set; }
        public bool ChangesetToInclude { get; set; }
        public string ReleaseName { get; set; }

        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        public string ReleaseDateFormated => ReleaseDate.ToString("d-MMMM-yyyy", new CultureInfo("en-US"));
        public string QaBuildName { get; set; }

        public DateTime QaBuildDate { get; set; } = DateTime.Now;

        public string QaBuildDateFormated => QaBuildDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));
        public string CoreBuildName { get; set; }

        public DateTime CoreBuildDate { get; set; } = DateTime.Now;

        public string CoreBuildDateFormated => CoreBuildDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));
        public string PsRefreshName { get; set; }
        public string PsRefreshChangeset { get; set; }
        public DateTime PsRefreshDate { get; set; } = DateTime.Now;

        public string PsRefreshDateFormated => PsRefreshDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));

        public string CoreChangeset { get; set; }
        public DateTime CoreDate { get; set; } = DateTime.Now;

        public string CoreDateFormated => CoreDate.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));




        public List<ClientWorkItem> WorkItems { get; set; }
        public List<CategoryChanges> CategorizedChanges { get; set; }




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
        public override string ToString()
        {
            return $"{Id}-{WorkItemTitle}-{WorkItemId}";
        }
    }
}
