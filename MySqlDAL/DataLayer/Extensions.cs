using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Microsoft.SqlServer.Server;

namespace Common.DataLayer
{
    public static class Extensions
    {
     
        public static string Right(this string s, int n)
        {
            s = s ?? "";
            return s.Length >= n
                ? s.Substring(s.Length - n, n)
                : s;
        }

        public static string CollapseWhiteSpace(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            return Regex.Replace(s, @"\s+", " ");
        }

        public static T ThrowIf<T>(this T value, Expression<Func<T, bool>> condition, Func<Exception> exception = null)
        {
            if (!condition.Compile()(value)) return value;
            var ex = exception == null ? new InvalidOperationException("Condition failed: " + condition.ToString()) : exception();
            throw ex;
        }
        
        public static T? Unless<T>(this T value, Func<T, bool> condition) where T : struct
        {
            return condition(value) ? (T?)null : value;
        }
        public static T? Unless<T>(this T value, bool condition) where T : struct
        {
            return condition ? (T?)null : value;
        }
        public static T? If<T>(this T value, Func<T, bool> condition) where T : struct
        {
            return condition(value) ? value : (T?)null;
        }
        public static T? If<T>(this T value, bool condition) where T : struct
        {
            return condition ? value : (T?)null;
        }

        public static bool HasKey(this NameValueCollection nvc, string key)
        {
            // Check for key or contained in the null key (eg.., ?key&abc=123)
            return nvc.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase) || (nvc.GetValues(null) ?? new string[] { }).Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        public static Uri ClearQueryString(this Uri uri)
        {
            var uriB = new UriBuilder(uri);
            uriB.Query = null;
            return uriB.Uri;
        }

        public static string ToString<T>(this Nullable<T> @this, string format, string defaultValue = null) where T : struct, IFormattable
        {
            return @this.HasValue ? @this.Value.ToString(format, null) : defaultValue;
        }

        public static Uri AddQueryString(this Uri uri, string key, string value)
        {
            var uriB = new UriBuilder(uri);
            uriB.AddQueryString(key, value);
            return uriB.Uri;
        }
        public static UriBuilder AddQueryString(this UriBuilder uriB, string key, string value)
        {
            if (string.IsNullOrEmpty(uriB.Query))
            {
                if (!string.IsNullOrEmpty(value)) uriB.Query = string.Format("{0}={1}", key, HttpUtility.UrlEncode(value));
            }
            else
            {
                var q = uriB.Query.Substring(1); // Remove leading ?
                var nvc = HttpUtility.ParseQueryString(q);
                if (string.IsNullOrEmpty(value))
                {
                    if (nvc.AllKeys.Contains(key)) nvc.Remove(key);
                }
                else
                {
                    nvc[key] = HttpUtility.UrlEncode(value);
                }
                uriB.Query = nvc.ToString(); // Overridden HttpValueCollection
            }
            return uriB;
        }

        public static T GetSafe<T>(this System.Data.IDataReader dr, int column)
        {
            var o = dr[column];
            if (o == null || o == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)o;
            }
        }
        public static T GetSafe<T>(this System.Data.IDataReader dr, string column)
        {
            var o = dr[column];

            if (o == null || o == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)o;
            }
        }

        public static DateTime GetDateTime(this System.Data.IDataReader dr, string column)
        {
            var o = dr[column];

            if (o == null || o == DBNull.Value)
            {
                return DateTime.MinValue;
            }
            else
            {
                return (DateTime)o;
            }
        }

        public static string ToString(this DateTime? dateTime, string format)
        {
            return (dateTime.HasValue) ? dateTime.Value.ToString(format) : (string)null;
        }


        public static Nullable<T> SafeParseAsEnum<T>(this int? @this) where T : struct
        {
            if (!@this.HasValue) return null;
            return Enum.IsDefined(typeof(T), @this) ? (T?)ParseAsEnum<T>(@this.Value) : null;
        }
        public static T ParseAsEnum<T>(this int @this) where T : struct
        {
            return (T)Enum.Parse(typeof(T), @this.ToString(), true);
        }

        public static Nullable<T> SafeParseAsEnum<T>(this string @this) where T:struct
        {
            if (string.IsNullOrEmpty(@this))
            {
                return null;
            }
            else
            {
                return Enum.IsDefined(typeof(T), @this) ? (T?)ParseAsEnum<T>(@this) : null;
            }
        }
        public static T ParseAsEnum<T>(this string @this) where T : struct
        {
            return (T)Enum.Parse(typeof(T), @this, true);
        }

        public static T Get<T>(this System.Data.IDataReader dr, int column)
        {
            var o = dr[column];
            return (T)o;
        }
        public static T Get<T>(this System.Data.IDataReader dr, string column)
        {
            var o = dr[column];
            try
            {
                return (T)o;
            }
            catch (Exception ex)
            {
                throw new InvalidCastException("Exception casting " + column + " to " + typeof(T).Name, ex);
            }
        }

        public static bool IsDBNull(this IDataReader dr, string column)
        {
            return dr.IsDBNull(dr.GetOrdinal(column));
        }

        public static bool HasColumn(this IDataReader dr, string column)
        {
            var dv = dr.GetSchemaTable().DefaultView;
            dv.RowFilter = string.Format("ColumnName= '{0}'", column);
            return dv.Count > 0;
        }

        public static IEnumerable<Control> GetControlsRecursive(this Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (Control cc in c.GetControlsRecursive())
                {
                    yield return cc;
                }
            }
        }

        public static T FindControlRecursive<T>(this Control root, string id) where T : Control
        {
            if (root == null) return null;

            Control c;
            // Easy case - is child of root
            c = root.FindControl(id);
            if (c != null) return (T)c;

            // Recursively search child controls
            foreach (Control child in root.Controls)
            {
                c = child.FindControlRecursive<T>(id);
                if (c != null) return (T)c;
            }

            // Not found
            return null;
        }

        public static HttpResponse Write(this HttpResponse response, params string[] values)
        {
            foreach (var s in values)
            {
                response.Write(s);
            }
            return response;
        }

        public static object DBNullOrValue<T>(this T? o) where T:struct
        {
            if (o.HasValue) return o.Value;
            return DBNull.Value;
        }

        public static object DBNullIfNullOrEmpty(this string s)
        {
            if (string.IsNullOrEmpty(s)) return DBNull.Value;
            return s;
        }

        public static object DBNullIfNullOrEmpty(this DateTime? dt)
        {
            if (dt == null) return DBNull.Value;
            return dt;
        }
        public static object DBNullIfDefault<T>(this T o)
        {
            if (object.Equals(o, default(T))) return DBNull.Value;
            return o;
        }

        public static string NullIfEmpty(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            return s;
        }

        public static string FormatIfNotNull(this object o, string format) {
            if (o == null) return null;
            return string.Format(format, o);
        }
        public static string FormatIfNotNullOrEmpty(this string s, string format)
        {
            return FormatIf(s, format, s2 => !string.IsNullOrEmpty(s2));
        }
        public static string FormatIfNotNullOrWhiteSpace(this string s, string format)
        {
            return FormatIf(s, format, s2 => !string.IsNullOrWhiteSpace(s2));
        }
        public static string FormatIf(this string s, string format, Func<string, bool> condition)
        {
            return FormatIf(s, format, condition(s));
        }
        public static string FormatIf(this string s, string format, bool condition)
        {
            if (!condition) return s;
            return string.Format(format, s);
        }
 

        public static string StripFirstParaTag(this string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var removeStart = "<p>";
            var removeEnd = "</p>";
            s = s.Replace("<p></p>", string.Empty);
            s = s.Replace("<p>&nbsp;</p>", string.Empty);
            int index = s.IndexOf(removeStart);
            string cleanString = (index < 0) ? s : s.Remove(index, removeStart.Length);
            index = cleanString.IndexOf(removeEnd);
            cleanString = (index < 0) ? cleanString : cleanString.Remove(index, removeEnd.Length);
            return cleanString;
        }

        public static string TrimInsideSpaces(this string s)
        {
            return s.Replace(" ", string.Empty);
        }

     
        public static bool TrySetSelectedIndex(this DropDownList ddl, string valueOrText)
        {
            var selectedItem = ddl.Items.FindByValue(valueOrText);
            if (selectedItem == null)
            {
                selectedItem = ddl.Items.FindByText(valueOrText);
            }
            if (selectedItem == null)
            {
                return false;
            }
            else
            {
                ddl.SelectedIndex = ddl.Items.IndexOf(selectedItem);
                return true;
            }
        }

        public static T? TryParse<T>(this string s) where T : struct
        {
            T value;
            if (TryParse<T>(s, out value)) return (T?)value;
            return null;
        }

        public static bool TryParse<T>(this string s, out T value)
        {
            if (string.IsNullOrEmpty(s)) // DateTimeConverter converts empty string to DateTime.MinValue
            {
                value = default(T);
                return false;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                value = (T)converter.ConvertFromString(s);
                return true;
            }
            catch
            {
                value = default(T);
                return false;
            }
        }

        public static string SplitWordsToSentence(this string source)
        {
            return string.Join(" ", SplitWords(source));
        }

        private static string[] SplitWords(this string source)
        {
            if (source == null) return new string[] { }; //Return empty array.
            if (source.Length == 0) return new string[] { "" };

            StringCollection words = new StringCollection();
            int wordStartIndex = 0;
            char[] letters = source.ToCharArray();
            char previousChar = char.MinValue;

            // Skip the first letter. we don't care what case it is.
            for (int i = 1; i < letters.Length; i++)
            {
                if (letters[i] == '_' || letters[i] == '-') letters[i] = ' ';
                
                if ((char.IsUpper(letters[i]) || char.IsDigit(letters[i])) && !char.IsWhiteSpace(previousChar))
                {
                    //Grab everything before the current character.
                    words.Add(new String(letters, wordStartIndex, i - wordStartIndex));
                    wordStartIndex = i;
                }
                previousChar = letters[i];
            }

            //We need to have the last word.
            words.Add(new String(letters, wordStartIndex, letters.Length - wordStartIndex));

            string[] wordArray = new string[words.Count];
            words.CopyTo(wordArray, 0);
            return wordArray;
        }

        public static string Render(this Control c)
        {
            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                c.RenderControl(htw);
                htw.Flush();
                sw.Flush();

                return sw.ToString();
            }
        }
        public static void Export(this Control c, string fileName, string contentType)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.ContentType = contentType;

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                PrepareControlForExport(c);
                c.RenderControl(htw);

                var response = HttpContext.Current.Response;
                response.ContentEncoding = System.Text.Encoding.UTF8;
                response.Write("\xFEFF"); // Give Excel a BOM so it knows it's Unicode; otherwise breaks on &nbsp;
                response.Write(sw.ToString());
                response.End();
            }
        }

        // GridView export culled from http://mattberseth.com/blog/2007/04/export_gridview_to_excel_1.html
        public static void Export(this GridView gv, string fileName)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader(
                "content-disposition", string.Format("attachment; filename={0}", fileName));
            HttpContext.Current.Response.ContentType = "application/ms-excel";

            using (StringWriter sw = new StringWriter())
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                //  Create a table to contain the grid
                Table table = new Table() { GridLines = gv.GridLines };

                //  add the header row to the table
                if (gv.HeaderRow != null)
                {
                    PrepareControlForExport(gv.HeaderRow);
                    table.Rows.Add(gv.HeaderRow);
                }
                //  add each of the data rows to the table
                foreach (GridViewRow row in gv.Rows)
                {
                    PrepareControlForExport(row);
                    table.Rows.Add(row);
                }
                //  add the footer row to the table
                if (gv.FooterRow != null)
                {
                    PrepareControlForExport(gv.FooterRow);
                    table.Rows.Add(gv.FooterRow);
                }

                //  render the table into the htmlwriter
                table.RenderControl(htw);

                //  render the htmlwriter into the response
                var response = HttpContext.Current.Response;
                response.ContentEncoding = System.Text.Encoding.UTF8;
                response.Write("\xFEFF"); // Give Excel a BOM so it knows it's Unicode; otherwise breaks on &nbsp;
                response.Write(sw.ToString());
                response.End();
            }
        }

        /// <summary>
        /// Replace any of the contained controls with literals
        /// </summary>
        /// <param name="control"></param>
        private static void PrepareControlForExport(Control control)
        {
            for (int i = 0; i < control.Controls.Count; i++)
            {
                Control current = control.Controls[i];
                if (current is LinkButton)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as LinkButton).Text));
                }
                else if (current is ImageButton)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as ImageButton).AlternateText));
                }
                else if (current is HyperLink)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as HyperLink).Text));
                }
                else if (current is DropDownList)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as DropDownList).SelectedItem.Text));
                }
                else if (current is CheckBox)
                {
                    control.Controls.Remove(current);
                    control.Controls.AddAt(i, new LiteralControl((current as CheckBox).Checked ? "True" : "False"));
                }

                if (current.HasControls())
                {
                    PrepareControlForExport(current);
                }
            }
        }

        public static double InWeeks(this TimeSpan t)
        {
            return t.TotalDays / 7;
        }

        public static double InYears(this TimeSpan t)
        {
            return t.TotalDays / 365;
        }

        public static string TruncateAt(this string s, int maxLength)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length <= maxLength) return s;

            if (s.Length > 3 && maxLength <= 3) return "...";
            return s.Substring(0, maxLength - 3) + "...";
        }

        public static string Safe(this string s)
        {
            return s ?? "";
        }
        public static T Safe<T>(this T o) where T : new()
        {
            if (o == null) return new T();
            return o;
        }
        
        public static IEnumerable<T> Safe<T>(this IEnumerable<T> e)
        {
            if (e == null) return new T[] { };
            return e;
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> e, T value)
        {
            foreach (var t in e)
            {
                if (object.Equals(value, t)) continue;
                yield return t;
            }
        }

        public static int FirstIndexOf<T>(this IEnumerable<T> e, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (var t in e)
            {
                if (predicate(t)) return i;
                i++;
            }
            return -1;
        }

        public static string TitleCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            if (s.IsAny(StringComparer.Ordinal, "TBD", "N/A")) return s;

            var ti = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
            // lowercase to remove any acronyms
            return ti.ToTitleCase(s.ToLower());
        }

        public static IHtmlString IfNullOrEmpty(this IHtmlString @this, string s)
        {
            if (@this == null || string.IsNullOrEmpty(@this.ToString())) return new HtmlString(s);
            return @this;
        }

        public static string IfNullOrEmpty(this string @this, string s)
        {
            return string.IsNullOrEmpty(@this) ? s : @this;
        }

        public static string NullIf(this string @this, string s, StringComparison comparisonType)
        {
            if (string.Equals(@this, s, comparisonType)) return null;
            return @this;
        }

        public static bool IsAssignableFromGeneric(this Type openGenericType, Type t)
        {
            foreach (var i in t.GetInterfaces())
            {
                if (!i.IsGenericType) continue;
                if (i.GetGenericTypeDefinition() == openGenericType) return true;
            }

            var openGenericArgs = openGenericType.GetGenericArguments();
            var tGenericArgs = t.GetGenericArguments();
            try
            {
                if (openGenericArgs.Length == tGenericArgs.Length && openGenericType.MakeGenericType(tGenericArgs).IsAssignableFrom(t))
                {
                    return true;
                }
                else
                {
                    return t.BaseType != null && IsAssignableFromGeneric(openGenericType, t.BaseType);
                }
            }
            catch (ArgumentException)
            {
                return false; // violates generic constraint
            }
        }

        public static void Replace<T>(this IList<T> list, T oldValue, T newValue)
        {
            Replace(list, oldValue, newValue, false);
        }
        public static void Replace<T>(this IList<T> list, T oldValue, T newValue, bool throwOnNotFound)
        {
            var i = list.IndexOf(oldValue);
            if (i < 0)
            {
                if (throwOnNotFound) throw new InvalidOperationException("Item not found");
                return;
            }

            list[i] = newValue;
        }
        public static string TitleCaseIngredient(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            const RegexOptions OPTIONS = RegexOptions.Compiled | RegexOptions.CultureInvariant |  RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
            if (Regex.IsMatch(s, "[a-z]", OPTIONS)) return s; // already title cased

            return s.TitleCase();
        }

        public static string TitleCaseDrugName(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            const string PATTERN = @"^(?<drug>[^\s\(]+\s?)(?<suffix>[^\(\)]+)?(?<dosage>\s?\(.+\))?$";
            const RegexOptions OPTIONS = RegexOptions.Compiled | RegexOptions.CultureInvariant |  RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
            var match = Regex.Match(s, PATTERN, OPTIONS);
            if (match.Success)
            {
                var drug = match.Groups["drug"].Value;
                var suffix = match.Groups["suffix"].Value;
                var dosage = match.Groups["dosage"].Value;

                if (Regex.IsMatch(drug, "[a-z]", OPTIONS)) return s; // Already title cased; leave as is
                if (string.IsNullOrEmpty(suffix) && string.IsNullOrEmpty(dosage)) return s.TitleCase(); // No suffix or dosage
                if (DRUG_NAME_SUFFIXES.Contains(suffix.Trim(), StringComparer.OrdinalIgnoreCase)) return drug.TitleCase() + suffix + dosage; // Upper case suffix
                return drug.TitleCase() + suffix.TitleCase() + dosage; // suffix is part of drug name
            }

            return s.TitleCase();
        }
        private static readonly string[] DRUG_NAME_SUFFIXES = new string[] {
            "CR", "XR", "ODT", "SR", "XL", "HFA", "HCT", "HD", "LA", "ER",
            "EC", "FE", "AQ", "ES", "CD", "IR", "DM", "IV", "OTC", "Z", "RFF"
        };

        public static string TitleCaseCompanyName(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            const string PATTERN = @"^(?<company>[^\s\(]+\s?)(?<suffix>[^\(\)]+)?$";
            const RegexOptions OPTIONS = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
            var match = Regex.Match(s, PATTERN, OPTIONS);
            if (match.Success)
            {
                var company = match.Groups["company"].Value;
                var suffix = match.Groups["suffix"].Value;
                
                if (Regex.IsMatch(company, "[a-z]", OPTIONS)) return s; // Already title cased; leave as is
                if (string.IsNullOrEmpty(suffix)) return s.TitleCase(); // No suffix
                var i = COMPANY_NAME_SUFFIXES.FirstIndexOf(c => string.Equals(suffix.Trim(), c, StringComparison.OrdinalIgnoreCase));
                if (i == -1)
                {
                    return company.TitleCase() + suffix.TitleCase();
                }
                else
                {
                    // use proper cased suffix
                    return company.TitleCase() + COMPANY_NAME_SUFFIXES[i];
                }
            }

            return s.TitleCase();
        }
        private static readonly string[] COMPANY_NAME_SUFFIXES = new string[] {
            "LLC", "LP", "AB", "USA", "AG", "S.A.", "SCE", "SE", "KGaA", "GmbH", "LLP", "S.p.A.", "CV", "C.V."
        };

        public static IHtmlString Join(this IEnumerable<IHtmlString> e, string separator)
        {
            if (e == null || !e.Any()) return new HtmlString(string.Empty);
            return new HtmlString(string.Join(
                HttpUtility.HtmlEncode(separator),
                e.Select(s => s.ToHtmlString()).Where(s => !string.IsNullOrEmpty(s)).ToArray()
            ));
        }

        public static string Join(this IEnumerable<string> e, string separator)
        {
            if (e == null || !e.Any()) return string.Empty;
            return string.Join(separator, e.Where(s => !string.IsNullOrEmpty(s)).ToArray());
        }
        public static string Join(this IEnumerable<string> e, string separator, string lastSeperator)
        {
            if (e == null || !e.Any()) return string.Empty;
            var value = string.Join(separator, e.Where(s => !string.IsNullOrEmpty(s)).ToArray());
            return Regex.Replace(value, @"([^" + separator + "]+)$", lastSeperator + "$1");
        }

        public static void Remove<T>(this IList<T> l, IList<T> removals)
        {
            foreach (var t in removals)
            {
                l.Remove(t);
            }
        }

        public static void Each<T>(this IEnumerable<T> e, Action<T> action)
        {
            foreach (var t in e)
            {
                action(t);
            }
        }
        public static void Each<T>(this IEnumerable<T> e, Action<T, int> action)
        {
            int i = 0;
            foreach (var t in e)
            {
                action(t, i++);
            }
        }

      

        public static string ReplaceIgnoreCase(this string originalString, string oldValue, string newValue)
        {
            int startIndex = 0;
            while (true)
            {
                startIndex = originalString.IndexOf(oldValue, startIndex, StringComparison.InvariantCultureIgnoreCase);
                if (startIndex == -1)
                    break;

                originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

                startIndex += newValue.Length;
            }

            return originalString;
        }
        public static string RemoveEmptyParens(this string s)
        {
            if (s != null)
                s = s.Replace("()", "");
            return s;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> @this, T o)
        {
            return o.ToEnumerable().Concat(@this);
        }
        public static IEnumerable<T> Append<T>(this IEnumerable<T> @this, T o)
        {
            return @this.Concat(o.ToEnumerable());
        }
        public static IEnumerable<T> ToEnumerable<T>(this T value)
        {
            if (value == null) return new T[] { };
            return new T[] { value };
        }

        public static bool IsAny<T>(this T @this, params T[] values)
        {
            if (@this == null) return false;
            return values.Contains(@this);
        }

        public static bool IsAny(this string @this, IEqualityComparer<string> stringComparer, params string[] values)
        {
            if (@this == null) return false;
            return values.Contains(@this, stringComparer);
        }

        public static string CombineWithWords(this IEnumerable<string> words, string wordSeperator)
        {
            if (words == null || !words.Any()) return "";
            var count = words.Count();
            switch (count)
            {
                case 1:
                    return words.Single();
                default:
                    return string.Join(", ", words.Take(count - 1).ToArray()) + " " + wordSeperator + " " + words.Last();
            }
        }

        public static SqlParameter AddAsTable<T>(this SqlParameterCollection c, string parameterName, IEnumerable<T> values)
        {
            return AddAsTable(c, parameterName, values, string.Format("dbo.{0}TableType", typeof(T).Name));
        }
        public static SqlParameter AddAsTable<T>(this SqlParameterCollection c, string parameterName, IEnumerable<T> values, string sqlTypeName)
        {
            var p = c.AddWithValue(parameterName, values.AsSqlDataRecord());
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = sqlTypeName;
            return p;
        }

        public static IEnumerable<SqlDataRecord> AsSqlDataRecord<T>(this IEnumerable<T> values)
        {
            if (values == null || !values.Any()) return null; // Annoying, but SqlClient wants null instead of 0 rows
            if (typeof(T) == typeof(string)) return AsSqlDataRecordString(values.Cast<string>());
            if (typeof(T).IsValueType) return AsSqlDataRecordStruct<T>(values);

            var props = typeof(T).GetProperties().Where(p => p.CanRead).Select(p => new { Name = p.Name, GetMethod = p.GetGetMethod() }).ToArray();
            var firstRecord = values.First();
            var metadata = props
                .Select(p => SqlMetaData.InferFromValue(p.GetMethod.Invoke(firstRecord, null), p.Name))
                .ToArray();
            return values.Select(v => 
            {
                var r = new SqlDataRecord(metadata);
                r.SetValues(
                    props.Select(p => p.GetMethod.Invoke(v, null)).ToArray()
                );
                return r;
            });
        }
        private static IEnumerable<SqlDataRecord> AsSqlDataRecordStruct<T>(this IEnumerable<T> values)
        {
            if (values == null || !values.Any()) return null; // Annoying, but SqlClient wants null instead of 0 rows
            var firstRecord = values.First();
            var metadata = SqlMetaData.InferFromValue(firstRecord, "Value");
            return values.Select(v => 
            {
                var r = new SqlDataRecord(metadata);
                r.SetValues(v);
                return r;
            });
        }
        private static IEnumerable<SqlDataRecord> AsSqlDataRecordString(this IEnumerable<string> values)
        {
            if (values == null || !values.Any()) return null; // Annoying, but SqlClient wants null instead of 0 rows
            var firstRecord = values.First();
            var metadata = SqlMetaData.InferFromValue(firstRecord, "Value");
            return values.Select(v => 
            {
                var r = new SqlDataRecord(metadata);
                r.SetValues(v);
                return r;
            });
        }

        //public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> values, Func<T, TKey> keyExtractor) where TKey : IEquatable<TKey>
        //{
        //    return values.Distinct(EqualityComparer.Create(keyExtractor));
        //}

        public static IEnumerable<string> GetSelectedTexts(this ListControl @this)
        {
            return @this.GetSelectedItems().Select(i => i.Text);
        }
        public static IEnumerable<string> GetSelectedValues(this ListControl @this)
        {
            return @this.GetSelectedItems().Select(i => i.Value);
        }
        public static IEnumerable<ListItem> GetSelectedItems(this ListControl @this)
        {
            return @this.Items.Cast<ListItem>().Where(i => i.Selected);
        }

        public static void SetSelectedIndices(this ListControl @this, IEnumerable<int> indices)
        {
            for (var i = 0; i < @this.Items.Count; i++)
            {
                @this.Items[i].Selected = indices.Contains(i);
            }
        }
        public static void SetSelectedValues(this ListControl @this, IEnumerable<string> values)
        {
            for (var i = 0; i < @this.Items.Count; i++)
            {
                var item = @this.Items[i];
                item.Selected = values.Contains(item.Value);
            }
        }

        public static IEnumerable<string> GetFields(this IDataReader dr) 
        {
            var dv = dr.GetSchemaTable().DefaultView;
            foreach (DataRowView r in dv)
            {
                yield return (string)r["ColumnName"];
            }
        }

        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, Func<TValue> getValue)
        {
            TValue v;
            if (d.TryGetValue(key, out v)) return v;
            v = getValue();

            var cd = d as System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>;
            if (cd == null)
            {
                d.Add(key, v);
            }
            else
            {
                cd.AddOrUpdate(key, v, (k, o) => v);
            }
            
            return v;
        }

        public static void AddRange<T>(this IList<T> list, params T[] values)
        {
            AddRange(list, (IEnumerable<T>)values);
        }
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> values)
        {
            var tList = list as List<T>;
            if (tList != null)
            {
                tList.AddRange(values);
            }
            else
            {
                foreach (var t in values)
                {
                    list.Add(t);
                }
            }
        }

        public static IEnumerable<T> PageIndex<T>(this IEnumerable<T> @this, int pageSize, int pageIndex)
        {
            return @this.PageNumber(pageSize, pageIndex + 1);
        }
        public static IEnumerable<T> PageNumber<T>(this IEnumerable<T> @this, int pageSize, int pageNumber)
        {
            return @this.Skip(
                (pageNumber - 1) * pageSize
            ).Take(pageSize);
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> @this, int numberOfGroups)
        {
            return @this.Select((t, i) => new { t, i })
                .GroupBy(x => x.i % numberOfGroups)
                .Select(x => x.Select(y => y.t));
        }

        public static bool Contains(this string @this, string value, StringComparison comparisonType)
        {
            return @this.IndexOf(value, comparisonType) >= 0;
        }

        //public static string Pluralize(this string s)
        //{
        //    var p = System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(new System.Globalization.CultureInfo("en-us"));
        //    if (p == null) return s + "s";
        //    return p.Pluralize(s);
        //}

        public static string Extract(this string s, string regEx)
        {
            var m = Regex.Match(s, regEx);
            if (!m.Success) return null;
            return m.Value;
        }

        public static IHtmlString HtmlEncode(this string @this)
        {
            return new HtmlString(HttpUtility.HtmlEncode(@this));
        }

        public static bool ContainsAny<T>(this IEnumerable<T> @this, IEnumerable<T> values)
        {
            return @this.ContainsAny(values, (t1, t2) => t1.Equals(t2));
        }
        public static bool ContainsAny<T1, T2>(this IEnumerable<T1> @this, IEnumerable<T2> values, Func<T1, T2, bool> comparer)
        {
            foreach (var t1 in @this)
            {
                foreach (var t2 in values)
                {
                    if (comparer(t1, t2)) return true;
                }
            }
            return false;
        }

        public static bool Remove<T>(this IList<T> @this, Func<T, bool> predicate)
        {
            var values = @this.Where(predicate).ToArray();
            bool removed = false;
            foreach (var v in values)
            {
                @this.Remove(v);
                removed = true;
            }
            return removed;
        }

        public static string ToCamelCase(this string @this)
        {
            if (string.IsNullOrEmpty(@this)) return @this;
            int i;
            for (i = 0; i < @this.Length; i++)
            {
                if (!Char.IsUpper(@this[i])) break;
            }
            if (i == 0) return @this;
            if (i == @this.Length) return @this.ToLower();
            return @this.Substring(0, i).ToLower() + @this.Substring(i);
        }

        public static string ReplaceAll(this string @this, IDictionary<string, string> replacements)
        {
            var patterns = replacements.Keys.Select(s => Regex.Escape(s));
            var regEx = new Regex(string.Join("|", patterns));
            return regEx.Replace(@this, m => replacements[m.Value]);
        }

        public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> d)
        {
            return new ReadOnlyDictionary<TKey, TValue>(d);
        }

        public static IReadOnlyCollection<T> AsReadOnly<T>(this T[] a)
        {
            return Array.AsReadOnly(a);
        }
    }



    public static class XmlDocumentExtensions
    {
        public static XmlDocument LoadXmlDocument(this XmlDocument xDoc, string xml)
        {
            xDoc.LoadXml(xml);
            return xDoc;
        }

        public static XmlAttribute AppendAttribute(this XmlNode xNode, string attributeName, string attributeValue)
        {
            var attribute = xNode.OwnerDocument.CreateAttribute(attributeName);
            attribute.Value = attributeValue;
            xNode.Attributes.Append(attribute);
            return attribute;
        }
    }
}
