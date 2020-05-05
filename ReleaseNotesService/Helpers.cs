using System;
using System.Globalization;
using DataModel;

namespace ReleaseNotesService
{
    public static class Helpers
    {
        public static string FormatData(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm", new CultureInfo("en-US"));
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