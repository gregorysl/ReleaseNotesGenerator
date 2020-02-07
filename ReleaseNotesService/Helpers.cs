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
    }
}
