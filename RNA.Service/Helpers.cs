using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RNA.Model;

namespace RNA.Service
{
    public static class Helpers
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
        }
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static void FilterTfsChanges(this DownloadedItems downloadedData, bool include = false)
        {
            foreach (var change in downloadedData.Changes)
            {
                if (change.checkedInBy.displayName == "TFS Service" || change.checkedInBy.displayName == "Project Collection Build Service (Product)" || change.comment.Contains("Automatic refresh", StringComparison.OrdinalIgnoreCase))
                {
                    change.Selected = include;
                }
            }
        }
    }
}
