using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        public static void Write<TItem>(IConsole console,
                                        IEnumerable<TItem> collection,
                                        IEnumerable<string> headers,
                                        string? footnotes,
                                        params Expression<Func<TItem, string>>[] values)
        {
            int columnsCount = values.Length;

            Func<TItem, string>[] columnFunctions = values.Select(x => x.Compile())
                                                          .ToArray();

            int[] columnWidths = (from cf in columnFunctions
                                  let x = collection.Select(cf).Max(x => x.Length)
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
            foreach (TItem item in collection)
            {
                for (int i = 0; i < columnsCount; ++i)
                {
                    Func<TItem, string> column = columnFunctions[i];
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
    }
}
