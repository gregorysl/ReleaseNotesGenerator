using System;
using Xceed.Words.NET;

namespace Gui
{
    public static class Helper
    {
        public static Paragraph CreateHeadingSection(this InsertBeforeOrAfter paragraph, string title)
        {
            return paragraph.InsertParagraphAfterSelf(title)
                .Heading(HeadingType.Heading1);
        }

        public static Paragraph FillFirstParagraph(this Cell c, string text)
        {
            return c.Paragraphs[0].Append(text);
        }

        public static Cell GetCell(this Table t, int row, int col)
        {
            return t.Rows[row].Cells[col];
        }
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
