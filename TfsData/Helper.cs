using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TfsData
{
    public static class Helper
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
        }

        public static T GetWithResponse<T>(this HttpClient client, string url)
        {
            using (var response = client.GetAsync(url).Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
        public static T PostWithResponse<T>(this HttpClient client, string url, object p)
        {
            using (var response = client.PostAsJsonAsync(url, p).Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
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
            return $"{connector}api-version={apiVersion}";
        }
    }

}
