using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace MySqlDAL.Extensions
{
    /// <summary>
    /// Extension Methods for String
    /// </summary>
    public static class StringExtensions
    {
        #region Methods for string

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool HasValue(this string value)
        {
            return !IsNullOrWhiteSpace(value);
        }
        /// <summary>
        /// Replaces Only the First Instance Of a search criteria ona string.
        /// </summary>
        /// <param name="text">The looong string that contains the match</param>
        /// <param name="search">The string to search for a match.</param>
        /// <param name="replace">The replacement string.</param>
        /// <returns></returns>
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        public static string TruncateString(this string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        public static bool SafeEquals(this string source, string value, StringComparison option = StringComparison.Ordinal)
        {
            return source != null && source.Equals(value, option);

        }

        public static bool ContainsInsensitive(this string str, string value)
        {
            if (str == null)
            {
                return false;
            }

            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(str, value, CompareOptions.IgnoreCase) != -1;
        }

        public static string RemoveLastInstanceOfWord(this string value, string wordToRemove)
        {
            if (!String.IsNullOrEmpty(value) && value.Length > 0 && value.ToLower().Contains(wordToRemove.ToLower()))
            {
                int lastIndexOfWord = value.LastIndexOf(wordToRemove, StringComparison.OrdinalIgnoreCase);

                if (lastIndexOfWord >= 0)
                {
                    value = value.Remove(lastIndexOfWord, wordToRemove.Length);
                }
            }

            return value.TrimSafely();
        }

        public static IEnumerable<string> ToListFromString(this string value, char separator = ',')
        {
            List<string> returnValue = new List<string> { value };
            if (value.Contains(','))
            {
                returnValue = value.Split(separator).ToList();
            }
            return returnValue;

        }

        /// <summary>
        /// Checks if a string is null or empty
        /// </summary>
        /// <param name="value" />
        /// <returns>True if null or empty, false otherwise</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Checks if a string is null or white space or empty
        /// </summary>
        /// <param name="value" />
        /// <returns>True if null or white space, or empty</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return value.TrimSafely().IsNullOrEmpty();
        }

        public static string TakeFirstChars(this string value, int amount)
        {
            if (value.IsNullOrEmpty())
            {
                return String.Empty;
            }

            return value.Length > amount ? value.Substring(0, amount) + "..." : value;
        }

        public static string TrimSafely(this string value, params char[] delims)
        {
            if (delims == null)
            {
                delims = new[] { ' ' };
            }

            return String.IsNullOrEmpty(value) ? value : value.Trim(delims);
        }

        /// <summary>
        /// Checks if the string contained in this StringBuilder is null or white space
        /// </summary>
        /// <param name="builder" />
        /// <returns />
        public static bool IsNullOrWhiteSpace(this StringBuilder builder)
        {
            return builder != null && builder.ToString().IsNullOrWhiteSpace();
        }

        public static string StringBeforeDelimiter(this string exp, params char[] delimiters)
        {
            var i = exp.IndexOfAny(delimiters);
            if (i > 0)
                exp = exp.Substring(0, i);
            return exp;
        }

        public static string StringBeforeDelimiter(this string exp)
        {
            return StringBeforeDelimiter(exp, ',', ';', ':');
        }

        public static string RemoveHtmlComments(this string str)
        {
            return Regex.Replace(str, "<!--.*?-->", String.Empty, RegexOptions.Singleline | RegexOptions.Compiled);
        }

        public static string RemoveNonWordsChars(this string str)
        {
            Regex reg = new Regex(@"\W");
            return reg.Replace(str, " ");
        }

        public static string RemoveEndOfLineCharacter(this string str)
        {
            return Regex.Replace(str, "(\r\n|\r|\n)", String.Empty, RegexOptions.Singleline | RegexOptions.Compiled);
        }
        public static string ReplaceTabsForSingleWhiteSpace(this string str)
        {
            return Regex.Replace(str, "(\t)", " ", RegexOptions.Singleline | RegexOptions.Compiled);

        }
        public static string ReplaceHtmlSpaceForSingleWhiteSpace(this string str)
        {
            return Regex.Replace(str, "&nbsp;", " ", RegexOptions.Singleline | RegexOptions.Compiled);
        }


        public static HtmlNode ConcatenateTables(this string fullHtml, string tableSelector = "table", string rowSelector = "tbody tr", bool throwResultNotFound = true, bool skipFirstRow = false)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(fullHtml);

            List<HtmlNode> allTables = htmlDocument.DocumentNode.QuerySelectorAll(tableSelector).ToList();
            if (!allTables.SafeAny())
            {
                if (throwResultNotFound)
                    throw new Exception("Could not find " + tableSelector);
                return null;
            }

            HtmlNode table = allTables.First();
            HtmlNode tBody = table.QuerySelectorAll("tbody").SingleOrDefault() ?? table;
            int skip = skipFirstRow ? 1 : 0;
            foreach (HtmlNode t in allTables.Skip(1))
            {
                List<HtmlNode> rows = t.QuerySelectorAll(rowSelector).ToList();
                if (!rows.SafeAny())
                {
                    if (throwResultNotFound)
                        throw new Exception("Could not find " + rowSelector);
                    return null;
                }

                foreach (HtmlNode r in rows.Skip(skip))
                {
                    tBody.AppendChild(r);
                }
            }
            return table;
        }

        public static string HtmlTrim(this string @this)
        {
            return (@this ?? "").Replace("&nbsp;", " ").Trim();
        }

        public static string FormatIfNotNullOrEmpty(this string value, string format)
        {
            if (String.IsNullOrEmpty(value))
                return value;
            return String.Format(format, value);
        }
        /// <summary>
        /// Converts to the corresponding Tick value. If no match then it uses weekly.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long ConvertToTimeSpanTicks(this string value)
        {
            long ticks;
            switch (value.ToLower())
            {
                case "daily":
                    ticks = TimeSpan.TicksPerDay - 1;
                    break;
                case "weekly":
                    ticks = TimeSpanExtensions.WeeklyTicks;
                    break;
                case "biweekly":
                    ticks = TimeSpanExtensions.BiWeeklyTicks;
                    break;
                case "monthly":
                    ticks = TimeSpanExtensions.MonthlyTicks;
                    break;
                case "quarterly":
                    ticks = TimeSpanExtensions.QuarterlyTicks;
                    break;
                case "yearly":
                    ticks = TimeSpanExtensions.YearlyTicks;
                    break;
                default:
                    ticks = TimeSpanExtensions.WeeklyTicks;
                    break;
            }
            return ticks;
        }


        public static Guid ExtractFirstGuid(this string value)
        {
            var forecastGuid = Regex.Match(value, @"[0-9A-F]{8}-([0-9A-F]{4}-){3}[0-9A-F]{12}", RegexOptions.IgnoreCase | RegexOptions.Multiline).Value;
            return Guid.Parse(forecastGuid);
        }


        public static DateTime ParseToDateTimeExact(this string value, string format)
        {
            string[] formatConverted = { format };
            DateTime date;
            if (DateTime.TryParseExact(value,
                       formatConverted,
                       System.Globalization.CultureInfo.InvariantCulture,
                       System.Globalization.DateTimeStyles.None,
                       out date))
            {
                return date;
            }
            throw new ArgumentException("Invalid argument", nameof(value));
        }

        #endregion
    }

}