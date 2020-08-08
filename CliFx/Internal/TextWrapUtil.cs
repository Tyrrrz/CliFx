using System;
using System.Linq;
using System.Text;

namespace CliFx.Internal
{
    internal static class TextWrapUtil
    {
        /// <summary>
        /// https://stackoverflow.com/questions/17586/best-word-wrap-algorithm
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lineWidth"></param>
        /// <param name="tabSize"></param>
        /// <returns></returns>
        public static string WrapText(string text, int lineWidth, int tabSize = 4)
        {
            string[] lines = text.Replace("\r\n", "\n")
                                 .Replace("\r", "\n")
                                 .Split('\n');

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                if (line.Length < 1)
                {
                    sb.AppendLine(); //empty lines
                    continue;
                }

                int indent = line.TakeWhile(c => c == '\t').Count(); //tab indents
                line = line.Replace("\t", new string(' ', tabSize)); //need to expand tabs here
                string lead = new string(' ', indent * tabSize); //create the leading space
                do
                {
                    //get the string that fits in the window
                    string subline = line.Substring(0, Math.Min(line.Length, lineWidth));
                    if (subline.Length < line.Length && subline.Length > 0)
                    {
                        //grab the last non white character
                        int lastword = subline.LastOrDefault() == ' ' ? -1 : subline.LastIndexOf(' ', subline.Length - 1);
                        if (lastword >= 0)
                            subline = subline.Substring(0, lastword);
                        sb.AppendLine(subline);

                        //next part
                        line = lead + line.Substring(subline.Length).TrimStart();
                    }
                    else
                    {
                        sb.AppendLine(subline); //everything fits
                        break;
                    }
                }
                while (true);
            }

            return sb.ToString();
        }
    }
}
