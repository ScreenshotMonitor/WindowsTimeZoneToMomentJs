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

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage : WinTzToMomentJsTzTool.exe [year_from] [year_to]");
            }
            else
            {
                var from = Convert.ToInt32(args[0]);
                var to = Convert.ToInt32(args[1]);

                var zones = TimeZoneInfo.GetSystemTimeZones().Where(x => x.GetAdjustmentRules().Any());
                var list = zones.Select(wtz =>
                    {
                        var ianaId = ConvertWindowsToIana(wtz.Id);
                        var mtz = TimeZoneToMoment.ToMoment(wtz, from, to);
                        return new MomentTimeZoneExt(ianaId, mtz);
                    }).ToList();
                Console.WriteLine(JsonConvert.SerializeObject(list, Formatting.None));
            }
        }
    }
}