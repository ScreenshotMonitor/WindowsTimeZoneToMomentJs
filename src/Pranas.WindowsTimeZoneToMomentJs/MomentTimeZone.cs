using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Pranas.WindowsTimeZoneToMomentJs
{
    /// <summary>
    /// The zone object in unpacked format 
    /// <a href="http://momentjs.com/timezone/docs/#/data-formats/">http://momentjs.com/timezone/docs/#/data-formats/</a> 
    /// </summary>
    public class MomentTimeZone
    {
        public string name { get; set; }
        public string descr { get; set; }
        public List<string> abbrs { get; set; }
        public List<long> untils { get; set; }
        public List<long> offsets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MomentTimeZone()
        {
            untils = new List<long>();
            offsets = new List<long>();
            abbrs = new List<string>();
        }
    }

    /// <summary>
    /// <c>MomentTimeZone</c> converter. 
    /// </summary>
    public static class MomentTimeZoneJsonConverter
    {
        /// <summary>
        /// Convert <c>MomentTimeZone</c> to json
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static string ToJson(this MomentTimeZone zone)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{ ");
            sb.Append("name : '").Append(Quote(zone.name))
              .AppendLine("',");
            sb.Append("abbrs : ").Append(ToJsonArray(zone.abbrs, s => string.Format("'{0}'", Quote(s))))
              .AppendLine(",");
            sb.Append("untils : ")
              .Append(ToJsonArray(zone.untils, x => x.ToString(CultureInfo.InvariantCulture)))
              .AppendLine(",");
            sb.Append("offsets : ")
              .Append(ToJsonArray(zone.offsets, x => x.ToString(CultureInfo.InvariantCulture)))
              .AppendLine("");
            sb.AppendLine(" ");
            return sb.ToString();
        }

        private static string Quote(string s)
        {
            return s.Replace("'", "''");
        }

        /// <summary>
        /// Convert the list to the json array.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="converter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Json string</returns>
        public static string ToJsonArray<T>(IEnumerable<T> list, Func<T, string> converter)
        {
            return "[" + string.Join(",", list.Select(converter)) + "]";
        }
    }
}