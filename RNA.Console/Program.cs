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
        static async Task Main(string[] args)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ChangeAzure, Change>()
                    .ForMember(dest => dest.checkedInBy, src => src.MapFrom(x => x.author))
                    .ForMember(dest => dest.createdDate, src => src.MapFrom(x => x.author.date))
                    .ForMember(dest => dest.changesetId, src=>src.MapFrom(x=>x.commitId));
            });
            
            IMapper mapper = config.CreateMapper();

            var executableLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            if (executableLocation == null) return;

            var settingsLocation = Path.Combine(executableLocation, "settings.json");
            var settingsContent = File.ReadAllText(settingsLocation);

            System.Console.WriteLine("Creating patch notes using following settings:");
            System.Console.WriteLine(settingsContent);

            var settings = JsonConvert.DeserializeObject<Settings>(settingsContent);

            //var changesetConnector = new TfsConnector(settings.Tfs, mapper);
            var changesetConnector = new AzureConnector(settings.Tfs, mapper);
            var workItemConnector = new AzureConnector(settings.Azure, mapper);
            var generator = new Generator(changesetConnector, workItemConnector);

            var releaseData = settings.Data;
            var downloadedData = await generator.DownloadData(releaseData);

            System.Console.WriteLine($"Downloaded data: {downloadedData.WorkItems.Count} work items {downloadedData.Changes.Count} changesets");

            var psRefresh = downloadedData.Changes.First(x => releaseData.ChangesetTo == x.changesetId.ToString());

            var message = generator.CreateDoc(downloadedData, psRefresh, settings.WorkItemStateInclude, releaseData, settings.DocumentLocation, settings.TestReport);

            if (string.IsNullOrWhiteSpace(message)) return;

            System.Console.WriteLine(message);
        }
    }
}