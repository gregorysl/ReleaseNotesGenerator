using System;
using System.Collections.Generic;
using System.Linq;
using DataModel;
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
    }

}
