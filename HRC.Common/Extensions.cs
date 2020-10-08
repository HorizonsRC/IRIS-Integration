using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;

namespace HRC.Common
{
    public static class Extensions
    {
        public static bool Contains(this string self, string value, StringComparison comparison)
        {
            return self.IndexOf(value, comparison) >= 0;
        }
        
        public static string SafeTrimOrEmpty(this string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
        
        public static string ToSvString<T>(this List<T> list)
        {
            return ToSvString<T>(list, false);
        }

        public static string ToSvString<T>(this List<T> list, bool stringValue)
        {
            return ToSvString<T>(list,                                 
                stringValue,
                System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator);
        }

        public static string ToSvString<T>(this List<T> list, bool stringValue, string listSeparator)
        {
            string result = string.Empty;
            if (list == null || list.Count == 0)
            {
                return result;
            }
            list.ForEach(new Action<T>((T t) => { result += string.Format("{1}{0}{1}{2}", t != null ? t.ToString() : string.Empty, stringValue ? "'" : string.Empty, listSeparator); }));
            return result.Substring(0, result.Length - listSeparator.Length);
        }

        public static string ToSqlString(this List<object> data, bool stringData)
        {
            int i = 0;
            string result = string.Empty;
            foreach (object value in data)
            {
                i++;
                string Id = string.Format("{1}{0}{1}", value.ToString(), stringData ? "'" : string.Empty);
                result += (i != data.Count) ? Id + "," : Id;
            }
            return string.Format("({0})", result);
        }

        public static string TranslateFields(this string value, string tablePrefix)
        {
            bool prefixTable = !string.IsNullOrEmpty(tablePrefix);
            return string.Format(value,
                (prefixTable ? tablePrefix : string.Empty),
                (prefixTable ? "." : string.Empty));
        }

        public static void Sort<T>(this ObservableCollection<T> list) where T : IComparable
        {
            List<T> sorted = list.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count(); i++)
            {
                list.Move(list.IndexOf(sorted[i]), i);
            }
        }

        public static string Truncate(this string value, int length)
        {
            return value.SafeSubstring(0, length);
        }

        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            return value == null ? string.Empty : ((value.Length >= (startIndex + length)) && (length > 0)) 
                ? value.Substring(startIndex, length) : (value.Length > startIndex) ? value.Substring(startIndex) : string.Empty;                            
        }

        public static string SafeSubstring(this string value, string searchString)
        {
            return value == null ? string.Empty : value.IndexOf(searchString) > 0 
                ? value.SafeSubstring(0, value.IndexOf(searchString))
                : value;
        }

        public static int FindCount<T>(this List<T> list, Predicate<T> action)
        {
            return list == null ? 0 : list.FindAll(action).Count;
        }

        public static void Update<T>(this List<T> list, Func<T, T> action)
        {
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = action(list[i]);
                }
            }
        }

        public static List<T> DistinctCollection<T>(this List<T> instance)
        {
            List<T> items = new List<T>();
            instance.ForEach(delegate(T t)
            {
                if (!items.Contains(t))
                {
                    items.Add(t);
                }
            });
            return items;
        }

        public static List<T> ListKeys<T>(this IDictionary instance)
        {
            List<T> items = new List<T>();
            foreach (T key in instance.Keys)
            {
                items.Add(key);
            }
            return items;
        }

        public static List<T> ListValues<T>(this IDictionary instance)
        {
            List<T> items = new List<T>();
            foreach (T value in instance.Values)
            {
                items.Add(value);
            }
            return items;
        }

        public static K FindKeyByValue<K, V>(this IDictionary<K, V> dictionary, V value)  
        {
           if (!(dictionary == null))
           {
               foreach (KeyValuePair<K, V> pair in dictionary)
               {
                   if (value.Equals(pair.Value)) return pair.Key;
               }

           }
           return default(K);
        }

        public static DateTime Midnight(this DateTime dateTime)
        {
            return dateTime.AddDays(1).AddSeconds(-1);
        }

        public static string ToReadableString(this TimeSpan span)
        {
            return string.Format("{0}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
        }

        public static void Perform(this Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        public static bool WithinBounds<T>(this T[] instance, int dimension, int index)
        {
            return index >= instance.GetLowerBound(dimension) && index <= instance.GetUpperBound(dimension);
        }
        
        public static T Assign<T>(this Enum e, params T[] args)
        {
            short result = 0;
            foreach (T t in args)
            {
                result += (short)Convert.ChangeType(t, typeof(short));
            }
            return (T)Enum.ToObject(typeof(T), result);
        }
    }
}
