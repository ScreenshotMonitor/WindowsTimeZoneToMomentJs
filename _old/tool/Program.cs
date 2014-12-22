using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pranas.WindowsTimeZoneToMomentJs;

namespace WinTzToMomentJsTzTool
{
    public class MomentTimeZoneExt : MomentTimeZone
    {
        public MomentTimeZoneExt(string ianaId, MomentTimeZone tz)
        {
            name = tz.name;
            descr = tz.descr;
            abbrs = tz.abbrs;
            untils = tz.untils;
            offsets = tz.offsets;
            IanaId = ianaId;
        }

        public string IanaId { get; private set; }
    }

    internal class Program
    {
        // This will return the "primary" IANA zone that matches the given windows zone.
        // If the primary zone is a link, it then resolves it to the canonical ID.
        public static string ConvertWindowsToIana(string windowsZoneId)
        {
            if (windowsZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
                return "Etc/UTC";

            switch (windowsZoneId)
            {
                case "E. Europe Standard Time":
                    return "Europe/Bucharest";
                case "Belarus Standard Time":
                    return "Europe/Minsk";
                case "Mid-Atlantic Standard Time":
                    return "Etc/GMT+2";
            }
            var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(windowsZoneId);
            if (tzi == null) return null;
            var tzid = tzdbSource.MapTimeZoneId(tzi);
            if (tzid == null) return null;
            return tzdbSource.CanonicalIdMap[tzid];
        }

        private static string FmtIso8601(DateTimeOffset dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ssK");
        }

        private static List<object> GetDates(string tzId, int from, int to)
        {
            var utcOffset = new TimeSpan(0, 0, 0, 0, 0);
            var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            var list = new List<object>();
            for (var y = from; y < to; y++)
            {
                for (var m = 1; m < 13; m++)
                {
                    var d = 15;
                    for (var h = 1; h < 24; h = h + 6)
                    {
                        var utcTime = new DateTimeOffset(y, m, d, h, 30, 0, 0, utcOffset);
                        var tzTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime.UtcDateTime, tz);
                        list.Add(new
                            {
                                u = FmtIso8601(utcTime),
                                z = FmtIso8601(tzTime)
                            });
                    }
                }
            }
            return list;
        }

        
        private static void GenTest(int from, int to)
        {
            var zones = TimeZoneInfo.GetSystemTimeZones().ToList();
            zones = zones.Where(x => TimeZoneToMomentConverter.Problems.Contains(x.Id)).ToList();
            var list = zones.Select(wtz =>
                {
                    var ianaId = ConvertWindowsToIana(wtz.Id);
                    var mtz = TimeZoneToMomentConverter.ToMoment(wtz, from, to);
                    return new MomentTimeZoneExt(ianaId, mtz);
                }).ToList();
            var r = list.Select(x => new
                {
                    tz = x,
                    dates = GetDates(x.name, from, to)
                });
            Console.WriteLine(JsonConvert.SerializeObject(r, Formatting.None));
        }


        private static void ExportZones(int from, int to)
        {
            var zones = TimeZoneInfo.GetSystemTimeZones().ToList();
            var list = zones.Select(wtz =>
                {
                    var ianaId = ConvertWindowsToIana(wtz.Id);
                    var mtz = TimeZoneToMomentConverter.ToMoment(wtz, from, to);
                    return new MomentTimeZoneExt(ianaId, mtz);
                }).ToList();

            Console.WriteLine(JsonConvert.SerializeObject(list, Formatting.None));
        }

        public static void RunMain(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage : WinTzToMomentJsTzTool.exe [gentest|export] [year_from] [year_to]");
            }
            else
            {
                var from = Convert.ToInt32(args[1]);
                var to = Convert.ToInt32(args[2]);
                switch (args[0])
                {
                    case "gentest":
                        GenTest(from, to);
                        break;
                    case "export":
                        ExportZones(from, to);
                        break;
                }
            }
        }

        private static void Test()
        {
            var zones = TimeZoneInfo.GetSystemTimeZones().Where(x => x.Id == "Samoa Standard Time");
            var list = zones.Select(wtz =>
            {
                var ianaId = ConvertWindowsToIana(wtz.Id);
                var mtz = TimeZoneToMomentConverter.ToMoment(wtz, 1990, 2030);
                return new MomentTimeZoneExt(ianaId, mtz);
            }).ToList();

            Console.WriteLine(JsonConvert.SerializeObject(list, Formatting.None));
        }

        public static void Main(string[] args)
        {
            RunMain(args);
            //Test();
        }
    }
}