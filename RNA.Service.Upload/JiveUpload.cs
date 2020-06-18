using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RNA.Model;

namespace RNA.Service.Upload
{
    public class JiveUpload
    {
        private const string ApiPath = "/api/core/v3/";
        private readonly Jive _settings;
        private readonly string _location;
        public JiveUpload(Jive settings, string location)
        {
            _settings = settings;
            _location = location;
        }
        public async Task Upload()
        {
            var fileInfo = new FileInfo(_location);
            if (!fileInfo.Exists)
            {
                Console.WriteLine($"{_location} file doesn't exist!");
                return;
            }
            var client = SetupHttpClient(_settings);

            var name = Path.GetFileNameWithoutExtension(_location);

            var requestContent = PrepareMultipartContent(_settings, fileInfo, name);

            HttpResponseMessage response;
            var searchResult = await SearchForFile(_settings, name, client);
            if (!searchResult.List.Any())
            {
                var contentUri = $"{_settings.Url}{ApiPath}contents";
                response = await client.PostAsync(contentUri, requestContent);
            }
            else
            {
                var contentUri = $"{_settings.Url}{ApiPath}contents/{searchResult.List.First().ContentId}";
                response = await client.PutAsync(contentUri, requestContent);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<Result>(responseJson);
            var outcomeText = responseData?.Resources == null
                ? $"Error uploading document:{responseJson}"
                : $"Document successfully uploaded. You can find it here:{responseData.Resources.Html}";
            Console.WriteLine(outcomeText);
        }

        private async Task<SearchResponse> SearchForFile(Jive settings, string name, HttpClient client)
        {
            var url =
                $"{settings.Url}{ApiPath}search/contents/?filter=place(/places/{settings.PlaceId})&filter=search(\"{name}\")";
            var searchResponse = await client.GetAsync(url);

            var searchResponseContent = await searchResponse.Content.ReadAsStringAsync();
            var searchResult = JsonConvert.DeserializeObject<SearchResponse>(searchResponseContent);
            return searchResult;
        }

        private HttpClient SetupHttpClient(Jive settings)
        {
            var client = new HttpClient();
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}"));

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);
            return client;
        }

        private MultipartFormDataContent PrepareMultipartContent(Jive settings, FileInfo fileInfo, string name)
        {
            var data = new
            {
                subject = name,
                type = "file",
                parent = $"{settings.Url}{ApiPath}places/{settings.PlaceId}",
                categories = new[] { "Release Notes" },
                tags = CreateTags(settings.Version)
            };
            var requestContent = new MultipartFormDataContent();

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            requestContent.Add(stringContent, "json");

            var fileBytes = File.ReadAllBytes(fileInfo.FullName);
            var staticFileContent = new ByteArrayContent(fileBytes);
            requestContent.Add(staticFileContent, fileInfo.Name);
            return requestContent;
        }

        private string[] CreateTags(string settings)
        {
            var versionSplit = settings.Split('.');
            var tags = new[]
            {
                NewTag(versionSplit, 2, false, "" ),
                NewTag(versionSplit, 2, true, ""  ),
                NewTag(versionSplit, 2, false, "R"),
                NewTag(versionSplit, 2, true, "R"  ),
                NewTag(versionSplit, 3, false, ""),
                NewTag(versionSplit, 3, true, ""),
                NewTag(versionSplit, 3, false, "R"),
                NewTag(versionSplit, 3, true, "R"),
                NewTag(versionSplit, 4, false, ""),
                NewTag(versionSplit, 4, true, ""),
                NewTag(versionSplit, 4, false, "R"),
                NewTag(versionSplit, 4, true, "R"),
                "patch notes",
                "release notes"
            };
            return tags;
        }

        private string NewTag(string[] input, int parts, bool removeDot, string toRemove)
        {
            var versionSubstring = string.Join(".", input.Take(parts));
            var versionWithoutChar = string.IsNullOrWhiteSpace(toRemove) ? versionSubstring : versionSubstring.Replace(toRemove, "");
            return removeDot ? versionWithoutChar.Replace(".", string.Empty) : versionWithoutChar;
        }
    }
}
