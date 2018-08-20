using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gui
{
    public class DataViewModel : INotifyPropertyChanged
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
        public string IterationSelected { get; set; }
        public string ChangesetFrom { get; set; }
        public bool ChangesetFromInclude { get; set; }
        public string ChangesetTo { get; set; }
        public bool ChangesetToInclude { get; set; }





        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
