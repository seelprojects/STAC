using SourceCodeIndexer.Indexer.Enum;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace SourceCodeIndexer.UI
{
    public class EnumUtility
    {
        /// <summary>
        /// Returns list of <see cref="EnumItem{T}"/> for all values of given enum of type T
        /// </summary>
        /// <typeparam name="T">Enum to get list</typeparam>
        /// <returns>List of <see cref="EnumItem{T}"/> for given enum</returns>
        public static List<EnumItem<T>> GetEnumItems<T>() where T : struct 
        {
            if ((typeof(T).BaseType != typeof(Enum)))
            {
                throw new Exception("GetEnumItems can only be called on enums");
            }

            return Enum.GetValues(typeof(T)).Cast<T>().Select(x => new EnumItem<T> { Value = x, Text = x.GetNameFromAttribute() }).ToList();
        }

        /// <summary>
        /// Returns list of <see cref="SelectEnumItem{T}"/> for all values of given enum of type T
        /// </summary>
        /// <typeparam name="T">Enum to get list</typeparam>
        /// <returns>List of <see cref="SelectEnumItem{T}"/> for given enum</returns>
        public static List<SelectEnumItem<T>> GetSelectEnumItems<T>() where T : struct 
        {
            return GetEnumItems<T>().Select(x => new SelectEnumItem<T>() { IsSelected = false, Text = x.Text, Value = x.Value }).ToList();
        }
    }

    public static class EnumExtension
    {
        /// <summary>
        /// Gets value specified by Name Attribute for an enum
        /// </summary>
        /// <typeparam name="T">Should be enum type</typeparam>
        /// <param name="myEnum">enum to get name from</param>
        /// <returns>Text as specified in <see cref="NameAttribute"/> of an enum value.</returns>
        public static string GetNameFromAttribute<T>(this T myEnum)
        {
            if (myEnum == null)
                return null;

            MemberInfo[] memberInfo = myEnum.GetType().GetMember(myEnum.ToString());
            if (memberInfo.Length == 0)
                return null;

            NameAttribute nameAttribute = Attribute.GetCustomAttribute(memberInfo[0], typeof(NameAttribute), false) as NameAttribute;

            return nameAttribute?.ToString();
        }
    }

    /// <summary>
    /// Holds Value of enum and Text
    /// </summary>
    public class EnumItem<T> where T : struct
    {
        public T Value { get; set; }

        public string Text { get; set; }
    }

    /// <summary>
    /// Holds <see cref="EnumItem{T}"/> including IsSelected
    /// </summary>
    public class SelectEnumItem<T> : EnumItem<T> where T : struct
    {
        public bool IsSelected { get; set; }
    }
}
