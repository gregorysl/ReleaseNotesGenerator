using System.IO;
using System.Reflection;
using System.Windows;
using DataModel;
using Newtonsoft.Json;

namespace Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ReleaseData Data;

        private string _saveLocation => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "settings.json");

        protected override void OnStartup(StartupEventArgs e)
        {
            Data = File.Exists(_saveLocation)
                ? JsonConvert.DeserializeObject<ReleaseData>(File.ReadAllText(_saveLocation))
                : new ReleaseData();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            JsonSerialization.WriteToJsonFile(_saveLocation, Data);
            base.OnExit(e);
        }
    }
}
