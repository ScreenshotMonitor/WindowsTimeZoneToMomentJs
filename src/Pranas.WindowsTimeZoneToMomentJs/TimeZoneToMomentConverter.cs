// Copyright © ScreenshotMonitor 2015
// http://screenshotmonitor.com/
// 
// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using NodaTime;

namespace Pranas.WindowsTimeZoneToMomentJs
{
    /// <summary>
    ///     Tool to generates JavaScript that adds MomentJs timezone into moment.tz store.
    ///     As per http://momentjs.com/timezone/docs/
    /// </summary>
    public static class TimeZoneToMomentConverter
    {
        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        private static readonly ConcurrentDictionary<Tuple<string, int, int, string>, string> Cache =
            new ConcurrentDictionary<Tuple<string, int, int, string>, string>();

        /// <summary>
        ///     Generates JavaScript that adds MomentJs timezone into moment.tz store.
        ///     It caches the result by TimeZoneInfo.Id
        /// </summary>
        /// <param name="tz">TimeZone</param>
        /// <param name="yearFrom">Minimum year</param>
        /// <param name="yearTo">Maximum year (inclusive)</param>
        /// <param name="overrideName">Name of the generated MomentJs Zone; TimeZoneInfo.Id by default</param>
        /// <returns>JavaScript</returns>
        public static string GenerateAddMomentZoneScript(TimeZoneInfo tz, int yearFrom, int yearTo,
            string overrideName = null)
        {
            var key = new Tuple<string, int, int, string>(tz.Id, yearFrom, yearTo, overrideName);

            return Cache.GetOrAdd(key, x =>
            {
                var untils = GetZoneUntilsOffsets(tz, yearFrom, yearTo);

                return string.Format(
                    @"(function(){{
    var z = new moment.tz.Zone(); 
    z.name = {0}; 
    z.abbrs = {1}; 
    z.untils = {2}; 
    z.offsets = {3};
    moment.tz._zones[z.name.toLowerCase().replace(/\//g, '_')] = z;
}})();",
                    Serializer.Serialize(overrideName ?? tz.Id),
                    Serializer.Serialize(untils.Select(u => u.Item1)),
                    Serializer.Serialize(untils.Select(u => u.Item2)),
                    Serializer.Serialize(untils.Select(u => u.Item3)));
            });
        }

        private static Tuple<string, long, long>[] GetZoneUntilsOffsets(TimeZoneInfo timeZone, int yearFrom, int yearTo)
        {
            var intervals = DateTimeZoneProviders.Bcl[timeZone.Id].
                GetZoneIntervals(Instant.FromUtc(yearFrom, 1, 1, 0, 0), Instant.FromUtc(yearTo + 1, 1, 1, 0, 0));

            return intervals.Select(i => 
                new Tuple<string, long, long>(
                    // abbrs
                    i.Name,
                    // untils
                    i.End.Ticks / NodaConstants.TicksPerMillisecond,
                    // offsets
                    -i.WallOffset.Ticks / NodaConstants.TicksPerMinute
                )).ToArray();
        }
    }
}