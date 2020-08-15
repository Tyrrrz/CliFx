using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CliFx.Internal.Extensions;

namespace CliFx.Utilities
{
    /// <summary>
    /// Simple table utils for console.
    /// </summary>
    public static class TableUtils
    {
        /// <summary>
        /// Writes a table to the console.
        /// </summary>
        public static void Write<TElement>(IConsole console,
                                           IEnumerable<TElement> collection,
                                           IEnumerable<string> headers,
                                           string? footnotes,
                                           params Expression<Func<TElement, string>>[] values)
        {
            int columnsCount = values.Length;

            Func<TElement, string>[] columnFunctions = values.Select(x => x.Compile())
                                                          .ToArray();

            int[] columnWidths = (from cf in columnFunctions
                                  let x = collection.Select(cf)
                                                    .Max(x => x.Length)
                                  select x).ToArray();

            //Update column widths for smaller than header length
            for (int i = 0; i < columnsCount; ++i)
            {
                string header = headers.ElementAtOrDefault(i) ?? string.Empty;

                if (columnWidths[i] < header.Length)
                    columnWidths[i] = header.Length;
            }

            int totalWidth = columnWidths.Sum() - 1 + (columnsCount) * 3;

            //Write top border
            console.Output.WriteLine(new string('=', totalWidth));

            //Write table header
            if (headers.Count() > 0)
            {
                for (int i = 0; i < columnsCount; ++i)
                {
                    string header = headers.ElementAtOrDefault(i) ?? string.Empty;
                    int targetWidth = columnWidths[i];

                    console.Output.Write(' ');
                    console.Output.Write(header.PadRight(targetWidth));

                    if (i + 1 < columnsCount)
                        console.Output.Write(" |");
                }
                console.Output.WriteLine();

                //Write middle line
                console.Output.WriteLine(new string('=', totalWidth));
            }

            //Write table body
            foreach (TElement item in collection)
            {
                for (int i = 0; i < columnsCount; ++i)
                {
                    Func<TElement, string> column = columnFunctions[i];
                    console.Output.Write(' ');

                    string value = columnFunctions[i].Invoke(item);
                    int targetWidth = columnWidths[i];

                    console.Output.Write(value.PadRight(targetWidth));

                    if (i + 1 < columnsCount)
                        console.Output.Write(" |");
                }

                console.Output.WriteLine();
            }

            // Write bottom border
            console.Output.WriteLine(new string('=', totalWidth));

            // Write footnotes
            if (!string.IsNullOrWhiteSpace(footnotes))
            {
                console.Output.WriteLine(TextUtils.AdjustNewLines(footnotes));
                console.Output.WriteLine(new string('=', totalWidth));
            }
        }

        /// <summary>
        /// Writes a table to the console.
        /// </summary>
        public static void Write<TKey, TElement>(IConsole console,
                                                 IEnumerable<IGrouping<TKey, TElement>> collection,
                                                 IEnumerable<string> headers,
                                                 string? footnotes,
                                                 params Expression<Func<TElement, string>>[] values)
        {
            int columnsCount = values.Length;

            Func<TElement, string>[] columnFunctions = values.Select(x => x.Compile())
                                                          .ToArray();

            int[] columnWidths = (from cf in columnFunctions
                                  let x = collection.SelectMany(x => x)
                                                    .Select(cf)
                                                    .Max(x => x.Length)
                                  select x).ToArray();

            //Update column widths for smaller than header length
            for (int i = 0; i < columnsCount; ++i)
            {
                string header = headers.ElementAtOrDefault(i) ?? string.Empty;

                if (columnWidths[i] < header.Length)
                    columnWidths[i] = header.Length;
            }

            int totalWidth = columnWidths.Sum() - 1 + (columnsCount) * 3;

            //Write top border
            console.Output.WriteLine(new string('=', totalWidth));

            //Write table header
            if (headers.Count() > 0)
            {
                for (int i = 0; i < columnsCount; ++i)
                {
                    string header = headers.ElementAtOrDefault(i) ?? string.Empty;
                    int targetWidth = columnWidths[i];

                    console.Output.Write(' ');
                    console.Output.Write(header.PadRight(targetWidth));

                    if (i + 1 < columnsCount)
                        console.Output.Write(" |");
                }
                console.Output.WriteLine();

                //Write middle line
                console.Output.WriteLine(new string('=', totalWidth));
            }

            foreach (IGrouping<TKey, TElement> group in collection)
            {
                TKey groupKey = group.Key;
                int countInGroup = group.Count();

                console.Output.WriteLine(new string('-', totalWidth));
                console.Output.WriteLine($" {groupKey} ({countInGroup}) ".PadBoth(totalWidth));
                console.Output.WriteLine(new string('-', totalWidth));

                //Write table body
                foreach (TElement item in group)
                {
                    for (int i = 0; i < columnsCount; ++i)
                    {
                        Func<TElement, string> column = columnFunctions[i];
                        console.Output.Write(' ');

                        string value = columnFunctions[i].Invoke(item);
                        int targetWidth = columnWidths[i];

                        console.Output.Write(value.PadRight(targetWidth));

                        if (i + 1 < columnsCount)
                            console.Output.Write(" |");
                    }

                    console.Output.WriteLine();
                }
            }

            // Write bottom border
            console.Output.WriteLine(new string('=', totalWidth));

            // Write footnotes
            if (!string.IsNullOrWhiteSpace(footnotes))
            {
                console.Output.WriteLine(TextUtils.AdjustNewLines(footnotes));
                console.Output.WriteLine(new string('=', totalWidth));
            }
        }
    }
}
