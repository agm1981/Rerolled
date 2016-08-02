using System;

namespace MySqlDAL.Extensions
{
    public static class GuidMethods
    {
        /// <summary>
        /// Checks if a guid is empty, new but improperly initialized
        /// </summary>
        /// <param name="value" />
        /// <returns>True if empty or initialized, false otherwise</returns>
        public static bool IsNullOrDefault(this Guid value)
        {
            return value == default(Guid) || value == Guid.Empty || value == new Guid();
        }

        public static bool IsNotNullOrDefault(this Guid value)
        {
            return !IsNullOrDefault(value);
        }

        public static Guid? GetNullIfDefault(this Guid value)
        {
            return value == new Guid() || value == default(Guid) ? (Guid?)null : value;
        }

        public static bool IsNullOrDefault(this Guid? value)
        {
            return !value.HasValue || (value.Value == default(Guid) || value.Value == Guid.Empty || value.Value == new Guid());
        }

        public static bool IsNotNullOrDefault(this Guid? value)
        {
            return !IsNullOrDefault(value);
        }
        /// <summary>
        /// Use only for guids that came from a int previously at it may overflow . int.ToGuid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt(this Guid value)
        {
            byte[] b = value.ToByteArray();
            int bint = BitConverter.ToInt32(b, 0);
            return bint;
        }
    }
}