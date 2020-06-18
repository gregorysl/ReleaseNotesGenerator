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
        public static string FormatData(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));
        }

        public static string ParseAndFormatData(this string input, string inputFormat, string outputFormat)
        {
            DateTime parsedDate;
            if (string.IsNullOrWhiteSpace(input))
            {
                parsedDate = DateTime.Today;
            }
            else
            {
                var isParsed = DateTime.TryParseExact(input, inputFormat, null, DateTimeStyles.None, out parsedDate);
                if (!isParsed) return $"Failed to parse {input} with format {inputFormat}";
            }

            return parsedDate.ToString(outputFormat);
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
