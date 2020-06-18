using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RNA.Service;
using RNA.Model;
using RNA.Service.Tfs;

namespace RNA.Console
{
    class Program
    {
        public static readonly string ConnectorFailText =
            "{0} connector authentication failed. Make sure your PAT is up to date.\nResponse:{1}";
        static async Task Main(string[] args)
        {
            var settings = GetSettings();
            if (settings == null)
            {
                System.Console.WriteLine("Error getting setting");
                return;
            }


            var changesetConnector = GetConnector(settings.ChangesetsServer);
            var ccStatus = await changesetConnector.TestConnection();
            if (ccStatus != "OK")
            {
                System.Console.WriteLine(ConnectorFailText, "Changeset", ccStatus);
                return;
            }
            var workItemConnector = GetConnector(settings.WorkItemServer);
            var wiStatus = await workItemConnector.TestConnection();
            if (wiStatus != "OK")
            {
                System.Console.WriteLine(ConnectorFailText, "Work Item", wiStatus);
                return;
            }

            var iterationTest = await workItemConnector.TestIteration("", settings.Data.Iteration);
            if (iterationTest != "OK")
            {
                System.Console.WriteLine($"Error while testing iteration path:{iterationTest}");
                return;
            }
            var generator = new Generator(changesetConnector, workItemConnector);

            var releaseData = settings.Data;
            var downloadedData = await generator.DownloadData(releaseData);

            System.Console.WriteLine($"Downloaded data: {downloadedData.WorkItems.Count} work items {downloadedData.Changes.Count} changesets");

            var psRefresh = downloadedData.Changes.First(x => releaseData.ChangesetTo == x.changesetId.ToString());

            var message = generator.CreateDoc(downloadedData, psRefresh, settings);

            if (string.IsNullOrWhiteSpace(message)) return;

            System.Console.WriteLine(message);
        }

        private static Connector GetConnector(ServerDetails server)
        {
            switch (server.ServerType)
            {
                case ServerTypeEnum.Azure:
                    return new AzureConnector(server);
                case ServerTypeEnum.Tfs:
                    return new TfsConnector(server);
                default:
                    throw new KeyNotFoundException($"Server type {server.ServerType} is not supported.");
            }
        }
        private static Settings GetSettings()
        {
            var executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            if (executableLocation == null) return null;

            var settingsLocation = Path.Combine(executableLocation, "settings.json");
            var settingsContent = File.ReadAllText(settingsLocation);

            System.Console.WriteLine("Creating patch notes using following settings:");
            System.Console.WriteLine(settingsContent);

            var settings = JsonConvert.DeserializeObject<Settings>(settingsContent);
            return settings;
        }
    }
}