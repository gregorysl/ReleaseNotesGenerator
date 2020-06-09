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
        public static async Task<T> PostWithResponseAsync<T>(this HttpClient client, string url, object p)
        {
            Console.WriteLine(JsonConvert.SerializeObject(p));
            using (var response = await client.PostAsJsonAsync(url, p))
            {
                string responseBody = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }

}
