using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MySqlDAL.Extensions
{
    /// <summary>
    /// Extension methods for IEnumerable
    /// </summary>
    public static class EnumerableMethods
    {
        /// <summary>
        /// Checks if an IEnumerable collection is initialized or contains items
        /// </summary>
        /// <param name="list">The array to check</param>
        /// <returns>True if the list is null or empty, false otherwise</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.ToList().Any();
        }

        public static IEnumerable<T> Safe<T>(this IEnumerable<T> c)
        {
            return c ?? new T[] { };
        }

        /// <summary>
        /// Returns Byte[] value From IEnumerable Object
        /// </summary>
        /// <param name="list">An IEnumerable list</param>
        /// <returns>Byte[]</returns>
        public static byte[] ReturnBinary<T>(this IEnumerable<T> list)
        {
            byte[] binary;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter bFormatter = new BinaryFormatter();
                stream.Position = 0;
                bFormatter.Serialize(stream, list.ToList());

                stream.Position = 0;

                using (BinaryReader br = new BinaryReader(stream))
                {
                    binary = br.ReadBytes((int)stream.Length);
                }
            }

            return binary;
        }

        /// <summary>
        /// Creates a string from Key-Value pair
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="listValues" />
        /// <param name="keyValueSeparator" />
        /// <param name="secondSeparator" />
        /// <returns>String value of key-value pair</returns>
        public static string ToStringFromKeyValuePair<T>(this IEnumerable<T> listValues, string keyValueSeparator = "=", string secondSeparator = "#")
        {
            StringBuilder srt = new StringBuilder();
            foreach (KeyValuePair<string, string>? convertedValue in listValues.Select(variable => variable as KeyValuePair<string, string>?)
                                                                               .Where(convertedValue => convertedValue.HasValue))
            {
                srt.AppendFormat("{0}{1}{2}{3}", convertedValue.Value.Key, keyValueSeparator, convertedValue.Value.Value, secondSeparator);
            }

            return srt.ToString();
        }

        /// <summary>
        /// Checks if an IEnumerable collection has elements safely without the risk of null exception
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <returns>True if there are any items in this IEnumerable</returns>
        public static bool SafeAny<T>(this IEnumerable<T> list)
        {
            return list != null && list.ToList().Any();
        }

        /// <summary>
        /// Checks if an IEnumerable collection has elements safely without the risk of null exception
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <param name="predicate" />
        /// <returns>True if there are any items in this IEnumerable which match the predicate</returns>
        public static bool SafeAny<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return list != null && list.ToList().Any(predicate);
        }

        /// <summary>
        /// Returns a list containing the elements that exist in both lists
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <param name="otherList" />
        /// <returns />
        public static IEnumerable<T> In<T>(this IEnumerable<T> list, IEnumerable<T> otherList)
        {
            return list.ToList().Where(dd => otherList.ToList().SafeAny(dd2 => dd2.Equals(dd)));
        }

        /// <summary>
        /// Returns a list containing the elements not in the source list
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <param name="otherList" />
        /// <returns />
        public static IEnumerable<T> NotIn<T>(this IEnumerable<T> list, IEnumerable<T> otherList)
        {
            return list.ToList().Where(dd => !otherList.ToList().SafeAny(dd2 => dd2.Equals(dd)));
        }

        /// <summary>
        /// Gets the number of elements in a list, safely
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <returns>The number of elements in the list</returns>
        public static int SafeCount<T>(this IEnumerable<T> list)
        {
            if (list == null)
                return 0;

            var enumerable = list as T[] ?? list.ToArray();
            return enumerable.ToList().SafeAny() ? enumerable.Count() : 0;
        }

        /// <summary>
        /// Determines whether a list of elements has more than 1 element
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <returns>True, if list has more than one record</returns>
        public static bool HasMoreThanOne<T>(this IEnumerable<T> list)
        {
            return list != null && list.Count() > 1;
        }

        public static string ToSeparatedString<T>(this IEnumerable<T> items, string separator = ",", string propertyToUse = null , string wrapper = null)
        {
            StringBuilder builder = new StringBuilder();

            if (items == null)
            {
                return null;
            }
            foreach (T listItem in items)
            {
                string displayValue = listItem.ToString();

                if (propertyToUse != null)
                {
                    try
                    {
                        displayValue = listItem.GetType().GetProperty(propertyToUse).GetValue(listItem, null).ToString();
                    }
                    catch (Exception)
                    {
                        displayValue = listItem.ToString();
                    }
                }

                builder.AppendFormat("{2}{0}{2}{1}", displayValue, separator, wrapper);

                //if (listItem is Enum)
                //{
                //    builder.AppendFormat("{2}{0}{2}{1}", Convert.ToInt32(listItem).ToString().Replace(replaceSource, replaceTarget), separator, wrapper);
                //}
                //else
                //{
                //    builder.AppendFormat("{2}{0}{2}{1}", displayValue.Replace(replaceSource, replaceTarget), separator, wrapper);
                //}
            }

            return builder.ToString().RemoveLastInstanceOfWord(separator);
        }


        //public static string ToSeparatedString<T>(this IEnumerable<T> list, string separator = ",", string propertyToUse = "", string wrapper = "", string replaceSource = "", string replaceTarget ="")
        //{
        //    StringBuilder builder = new StringBuilder();

        //    if (list.SafeAny())
        //    {
        //        foreach (T listItem in list)
        //        {
        //            string displayValue = listItem.ToString();

        //            if (propertyToUse.HasValue())
        //            {
        //                try
        //                {
        //                    displayValue = listItem.GetType().GetProperty(propertyToUse).GetValue(listItem, null).ToString();
        //                }
        //                catch (Exception)
        //                {
        //                    displayValue = listItem.ToString();
        //                }
        //            }

        //            if (listItem is Enum)
        //            {
        //                builder.AppendFormat("{2}{0}{2}{1}", Convert.ToInt32(listItem).ToString().Replace(replaceSource, replaceTarget), separator, wrapper);
        //            }
        //            else
        //            {
        //                builder.AppendFormat("{2}{0}{2}{1}", displayValue.Replace(replaceSource, replaceTarget), separator, wrapper);
        //            }
        //        }
        //    }

        //    return builder.ToString().RemoveLastInstanceOfWord(separator);
        //}


        /// <summary>
        /// Converts a list to a string separating fields with a given character. Useful for exporting to CSV.
        /// </summary>
        /// <remarks>
        /// The output of this method is different from that of method ToSeparatedString.
        /// This method outputs property names as headers and property values in subsequent rows (suitable for use in Excel)
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="aList"></param>
        /// <param name="seperatorChar">character to use as field separator</param>
        /// <returns></returns>
        public static string ConvertListToCharSeparatedString<T>(this IEnumerable<T> aList, string seperatorChar = ",")
        {
            StringBuilder sbOut = new StringBuilder();
            try
            {
                string seperator = String.Empty;
                StringBuilder builder = new StringBuilder();

                foreach (T dataItem in aList)
                {
                    PropertyInfo[] allProperties = dataItem.GetType().GetProperties();
                    if (sbOut.Length == 0)
                    {
                        //write column names
                        foreach (PropertyInfo col in allProperties)
                        {
                            builder.Append(seperator).Append(col.Name);
                            seperator = seperatorChar;
                        }
                        sbOut.AppendLine(builder.ToString());
                    }

                    seperator = String.Empty;
                    StringBuilder builderTmp = new StringBuilder();
                    // ReSharper disable LoopCanBePartlyConvertedToQuery
                    foreach (PropertyInfo thisProperty in allProperties)
                    {
                        object value = thisProperty.GetValue(dataItem, null);
                        String propetyValue = (value == null ? String.Empty : value.ToString());
                        builderTmp.Append(seperator).Append(propetyValue);
                        seperator = seperatorChar;
                    }
                    // ReSharper restore LoopCanBePartlyConvertedToQuery

                    sbOut.AppendLine(builderTmp.ToString());
                }

                return sbOut.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(String.Format("Failed to convert list to string. List type:{0}.", aList.GetType()), ex);
            }
        }

        /// <summary>
        /// Overload for Contains which can take a StrongComparison parameter
        /// </summary>
        /// <param name="value" />
        /// <param name="valueToCheck" />
        /// <param name="comparison" />
        /// <returns />
        public static bool Contains(this IEnumerable<string> value, string valueToCheck, StringComparison comparison)
        {
            bool contains = false;

            IEnumerable<string> enumerable = value as IList<string> ?? value.ToList();
            if (enumerable.SafeAny() && valueToCheck != null)
            {
                switch (comparison)
                {
                    case StringComparison.CurrentCultureIgnoreCase:
                    case StringComparison.InvariantCultureIgnoreCase:
                    case StringComparison.OrdinalIgnoreCase:
                        contains = enumerable.SafeAny(v => v.ToUpper().TrimSafely().Contains(valueToCheck.ToUpper().TrimSafely()));
                        break;

                    default:
                        contains = enumerable.SafeAny(v => v.TrimSafely().Contains(valueToCheck.TrimSafely()));
                        break;
                }
            }

            return contains;
        }

       

        /// <summary>
        /// Splits an IEnumerable into equally sized IEnumerable lists
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <param name="numberOfChunks" />
        /// <returns />
        public static IEnumerable<IEnumerable<T>> SplitIntoChunks<T>(this IEnumerable<T> list, int numberOfChunks) where T : class
        {
            List<List<T>> splitList = new List<List<T>>();

            for (int currentChunk = 0; currentChunk < numberOfChunks; currentChunk++)
            {
                splitList.Add(new List<T>());
            }

            list = list.ToList();
            if (list.SafeAny())
            {
                int numberOfElements = list.Count();
                for (int i = 0; i < numberOfElements; i++)
                {
                    T currentElement = list.ElementAtOrDefault(i);
                    if (currentElement != null)
                    {
                        List<T> currentList = splitList.ElementAtOrDefault(i % numberOfChunks);
                        if (currentList != null)
                        {
                            currentList.Add(currentElement);
                        }
                    }
                }
            }

            return splitList;
        }

        /// <summary>
        /// Gets an Object from Serialized Byte Stream
        /// </summary>
        /// <param name="value">The byte enumerable to Deserialize</param>
        /// <returns />
        public static object DeserializeObjectFromBinary(this IEnumerable<byte> value)
        {
            object obj;
            using (MemoryStream stream = new MemoryStream(value.ToArray()))
            {
                BinaryFormatter bFormatter = new BinaryFormatter();
                stream.Position = 0;
                obj = bFormatter.Deserialize(stream);
            }
            return obj;
        }

        /// <summary>
        /// Coalesces List (Removes all Nulls)
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="source" />
        /// <returns />
        public static IEnumerable<T> Coalesce<T>(this IEnumerable<T?> source) where T : struct
        {
            return source.ToList().Where(x => x.HasValue).Select(x => x.GetValueOrDefault());
        }

        /// <summary>
        /// Coalesces List (Removes all Nulls)
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="source" />
        /// <returns />
        public static IEnumerable<T> Coalesce<T>(this IEnumerable<T> source) where T : class
        {
            return source.ToList().Where(x => x != null).Select(x => x);
        }

        /// <summary>
        /// Gets the Mode from Enumerable list of primitive values
        /// Mode: the value that occurs most often
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <returns>The Mode of list</returns>
        public static T Mode<T>(this IEnumerable<T> list) where T : struct
        {
            T mode = default(T);

            IEnumerable<T> enumerable = list as IList<T> ?? list.ToList();
            if (enumerable.SafeAny())
            {
                mode = enumerable.GroupBy(t => t).OrderByDescending(g => g.Count()).First().Key;
            }

            return mode;
        }

        /// <summary>
        /// Gets the Mode from Enumerable list of strings
        /// Mode: the string that occurs most often
        /// </summary>
        /// <param name="list" />
        /// <returns>The Mode of list</returns>
        public static string Mode(this IEnumerable<string> list)
        {
            string mode = String.Empty;

            IEnumerable<string> enumerable = list as IList<string> ?? list.ToList();
            if (enumerable.SafeAny())
            {
                mode = enumerable.GroupBy(t => t).OrderByDescending(g => g.Count()).First().Key;
            }

            return mode;
        }

        /// <summary>
        /// Gets the Median from Enumerable list of primitive values
        /// Median: middle value in the list
        /// NOTE: If list is even, the element that will be returned will be element at index ((Count - 1) / 2)
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list" />
        /// <returns>The Median of list</returns>
        public static T Median<T>(this IEnumerable<T> list) where T : struct
        {
            T median = default(T);

            IEnumerable<T> enumerable = list as IList<T> ?? list.ToList();
            if (enumerable.SafeAny())
            {
                int count = enumerable.Count();
                int itemIndex = count / 2;

                list = enumerable.OrderBy(t => t);

                median = list.ElementAt(itemIndex);
            }

            return median;
        }

        /// <summary>
        /// Gets the Median from Enumerable list of strings
        /// Median: middle string in the list
        /// NOTE: If list is even, the string that will be returned will be string at index ((Count - 1) / 2)
        /// </summary>
        /// <param name="list" />
        /// <returns>The Median of list</returns>
        public static string Median(this IEnumerable<string> list)
        {
            string median = String.Empty;

            IEnumerable<string> enumerable = list as IList<string> ?? list.ToList();
            if (enumerable.SafeAny())
            {
                int count = enumerable.Count();
                int itemIndex = count / 2;

                list = enumerable.OrderBy(t => t);

                median = list.ElementAt(itemIndex);
            }

            return median;
        }

        /// <summary>
        /// Converts Enumerable collection into DataTable
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="collection" />
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection)
        {
            DataTable dataTable = new DataTable();
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            //Create the columns in the DataTable
            foreach (PropertyInfo property in properties)
            {
                Type propertyType = property.PropertyType;

                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyType = new NullableConverter(propertyType).UnderlyingType;
                }

                dataTable.Columns.Add(property.Name, propertyType);
            }

            //Populate the table
            foreach (T item in collection.ToList())
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow.BeginEdit();

                foreach (PropertyInfo property in properties)
                {
                    dataRow[property.Name] = property.GetValue(item, null);
                }

                dataRow.EndEdit();
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        /// <summary>
        /// Determines if two lists are mutually exclusive
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list1" />
        /// <param name="list2" />
        /// <returns>True, if the lists are mutually exclusive</returns>
        public static bool MutuallyExclusive<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            IEnumerable<T> enumerable = list1 as T[] ?? list1.ToArray();
            IEnumerable<T> enumerable1 = list2 as T[] ?? list2.ToArray();
            return !enumerable.SafeAny(enumerable1.Contains) && !enumerable1.SafeAny(enumerable.Contains);
        }

        /// <summary>
        /// Determines if two lists are equal (same elements in both lists)
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list1" />
        /// <param name="list2" />
        /// <returns>True, if the lists are equal</returns>
        public static bool ListEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            IEnumerable<T> first = list1 as T[] ?? list1.ToArray();
            IEnumerable<T> second = list2 as T[] ?? list2.ToArray();
            return list1 != null &&
                   list2 != null &&
                   first.Except(second).IsNullOrEmpty() &&
                   second.Except(first).IsNullOrEmpty();
        }

        /// <summary>
        /// Determines if two lists are equal (same elements in both lists)
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="list1" />
        /// <param name="list2" />
        /// <param name="comparer"></param>
        /// <returns>True, if the lists are equal</returns>
        public static bool ListEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
        {
            IEnumerable<T> first = list1 as T[] ?? list1.ToArray();
            IEnumerable<T> second = list2 as T[] ?? list2.ToArray();
            return list1 != null &&
                   list2 != null &&
                   first.Except(second, comparer).IsNullOrEmpty() &&
                   second.Except(first, comparer).IsNullOrEmpty();
        }
    }
}
