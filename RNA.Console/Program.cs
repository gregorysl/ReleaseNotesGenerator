using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ReleaseNotesService;
using RNA.Model;
using TfsData;

namespace RNA.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            if (executableLocation == null) return;

            var settingsLocation = Path.Combine(executableLocation, "settings.json");
            var settingsContent = File.ReadAllText(settingsLocation);

            System.Console.WriteLine("Creating patch notes using following settings:");
            System.Console.WriteLine(settingsContent);

            var settings = JsonConvert.DeserializeObject<Settings>(settingsContent);

            var tfs = new TfsConnector(settings.Tfs, settings.Azure);
            var generator = new Generator(tfs);

            var releaseData = settings.Data;
            var downloadedData = generator.DownloadData(releaseData);

            System.Console.WriteLine($"Downloaded data: {downloadedData.WorkItems.Count} work items {downloadedData.Changes.Count} changesets");

            var psRefresh = downloadedData.Changes.First(x => releaseData.ChangesetTo == x.changesetId.ToString());

            var message = generator.CreateDoc(downloadedData, psRefresh, settings.WorkItemStateInclude, releaseData, settings.DocumentLocation, settings.TestReport);

            if (string.IsNullOrWhiteSpace(message)) return;

            System.Console.WriteLine(message);
        }
    }
}
