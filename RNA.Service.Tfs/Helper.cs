using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RNA.Service.Tfs
{
    public static class Helper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
        }

        public static async Task<T> PostAsync<T>(this HttpClient client, string url, object obj, string apiVersion)
        {
            var finalUrl = url.AppendApiVersion(apiVersion);
            using (var response = await client.PostAsJsonAsync(finalUrl, obj))
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
        public static async Task<HttpResponseMessage> PostAsJsonAsync(this HttpClient client, string url, object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }


        public static async Task<T> GetAsync<T>(this HttpClient client, string url, string apiVersion)
        {
            var finalUrl = url.AppendApiVersion(apiVersion);
            using (var response = await client.GetAsync(finalUrl))
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
        public static T CloneJson<T>(this T source)
        {
            if (source == null)
            {
                return default;
            }
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        public static string AppendApiVersion(this string url, string apiVersion)
        {
            var connector = url.Contains('?') ? "&" : "?";
            return $"{url}{connector}api-version={apiVersion}";
        }
    }

}
