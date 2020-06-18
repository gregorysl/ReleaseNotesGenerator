using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RNA.Model;

namespace ReleaseNotesService
{
    public static class Helpers
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector).Select(x => x.FirstOrDefault());
        }
        public static string FormatData(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));
        }
        public static string ParseAndFormatData(this string input, string inputFormat, string outputFormat)
        {
            return DateTime.TryParseExact(input, inputFormat, null, DateTimeStyles.None, out var parsedDate)
                ? parsedDate.ToString(outputFormat)
                : $"Failed to parse {input} with format {inputFormat}";
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
