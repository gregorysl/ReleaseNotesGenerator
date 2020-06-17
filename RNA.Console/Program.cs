using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using ReleaseNotesService;
using RNA.Model;
using TfsData;

namespace RNA.Console
{
    class Program
    {
        public static readonly string ConnectorFailText =
            "{0} connector authentication failed. Make sure your PAT is up to date.\nResponse:{1}";
        static async Task Main(string[] args)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ChangeAzure, Change>()
                    .ForMember(dest => dest.checkedInBy, src => src.MapFrom(x => x.author))
                    .ForMember(dest => dest.createdDate, src => src.MapFrom(x => x.author.date))
                    .ForMember(dest => dest.changesetId, src=>src.MapFrom(x=>x.commitId));
            });
            
            var mapper = config.CreateMapper();

            var executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            if (executableLocation == null) return;

            var settingsLocation = Path.Combine(executableLocation, "settings.json");
            var settingsContent = File.ReadAllText(settingsLocation);

            System.Console.WriteLine("Creating patch notes using following settings:");
            System.Console.WriteLine(settingsContent);

            var settings = JsonConvert.DeserializeObject<Settings>(settingsContent);

            var changesetConnector = GetConnector(settings.ChangesetsServer, mapper);
            var ccStatus = await changesetConnector.TestConnection();
            if (ccStatus != "OK")
            {
                System.Console.WriteLine(ConnectorFailText, "Changeset", ccStatus);
                return;
            }
            var workItemConnector = GetConnector(settings.WorkItemServer, mapper);
            var wiStatus = await workItemConnector.TestConnection();
            if (wiStatus != "OK")
            {
                System.Console.WriteLine(ConnectorFailText, "Work Item", wiStatus);
                return;
            }

            var iterationTest =await workItemConnector.TestIteration("", settings.Data.Iteration);
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

            var message = generator.CreateDoc(downloadedData, psRefresh, settings.WorkItemStateInclude, releaseData, settings.DocumentLocation, settings.TestReport);

            if (string.IsNullOrWhiteSpace(message)) return;

            System.Console.WriteLine(message);
        }

        private static Connector GetConnector(ServerDetails server, IMapper mapper)
        {
            switch (server.ServerType)
            {
                case ServerTypeEnum.Azure:
                    return new AzureConnector(server, mapper);
                case ServerTypeEnum.Tfs:
                    return new TfsConnector(server, mapper);
                default:
                    throw new KeyNotFoundException($"Server type {server.ServerType} is not supported.");
            }
        }
    }
}