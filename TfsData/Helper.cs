﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using DataModel;
using Newtonsoft.Json;
using ClientWorkItem = DataModel.ClientWorkItem;

namespace TfsData
{
    public static class Helper
    {
        public static ClientWorkItem ToClientWorkItem(this Fields item)
        {
            return new ClientWorkItem
            {
                Id = item.Id,
                Title = item.Title,
                ClientProject = item.ClientProject ?? "N/A"
            };
        }
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
        }

        public static T GetWithResponse<T>(this HttpClient client, string url)
        {
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
        public static T PostWithResponse<T>(this HttpClient client, string url, object p)
        {
            using (HttpResponseMessage response = client.PostAsJsonAsync(url, p).Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
    }

}
