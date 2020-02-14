using System;
using System.Globalization;

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
    }
}
