using System;
using System.Linq;

namespace MySqlDAL.Extensions
{
    public static class EnumerationsHelper
    {
        /// <summary>
        /// Converts Integer into an Enumeration
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="enumValue" />
        /// <returns>Enumeration equivalent of Integer</returns>
        public static T ConvertFromInteger<T>(this int enumValue)
        {
            T returnVal = default(T);

            Type baseType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            foreach (T val in Enum.GetValues(baseType).Cast<T>().ToList().Where(val => Convert.ToInt32(val) == enumValue))
            {
                returnVal = val;
            }

            return returnVal;
        }

        /// <summary>
        /// Converts String into an Enumeration
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="enumValue" />
        /// <returns>Enumeration equivalent of String</returns>
        public static T ConvertFromString<T>(this string enumValue)
        {
            enumValue = enumValue.TrimSafely();

            T defaultItem = default(T);

            Type baseType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            foreach (T item in Enum.GetValues(baseType).Cast<T>().Where(item => item.ToString().Equals(enumValue, StringComparison.CurrentCultureIgnoreCase)))
            {
                return item;
            }

            return defaultItem;
        }
    }
}
