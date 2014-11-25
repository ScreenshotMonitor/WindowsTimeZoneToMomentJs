using System;
using System.Collections.Generic;
using System.Linq;

namespace Pranas.WindowsTimeZoneToMomentJs
{
    /// <summary>
    /// Windows TimeZoneInfo to moment.js time zone converter
    /// </summary>
    public static class TimeZoneToMoment
    {
        #region Interface

        private const int YearDelta = 10;

        /// <summary>
        /// Converts <c>TimeZoneInfo</c> to the zone object for the default period in years (+/- 10 years to the current date).
        /// </summary>
        /// <param name="tz">Windows time zone</param>
        /// <returns><c>MomentTimeZone</c> - the zone object in unpacked format</returns>
        /// <example>  
        /// This sample shows how to use the <see cref="ToMoment(TimeZoneInfo)"/> method.
        /// <code> 
        /// class TestClass  
        /// { 
        ///     static void Main()  
        ///     { 
        ///         var tz = TimeZoneToMoment.ToMoment(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
        ///         var json = Newtonsoft.Json.JsonConvert.SerializeObject(tz);
        ///         Console.WriteLine(json);
        ///     } 
        /// } 
        /// </code> 
        /// </example> 
        /// <seealso cref="ToMoment(TimeZoneInfo, int, int)"/>
        public static MomentTimeZone ToMoment(TimeZoneInfo tz)
        {
            var yearNow = DateTime.UtcNow.Year;
            return ToMoment(tz, yearNow - YearDelta, yearNow + YearDelta);
        }

        /// <summary>
        /// Converts <c>TimeZoneInfo</c> to the zone object for the given period in years.
        /// </summary>
        /// <param name="tz">Windows time zone</param>
        /// <param name="from">Year from.</param>
        /// <param name="to">Year to.</param>
        /// <returns><c>MomentTimeZone</c> - the zone object in unpacked format</returns>
        /// <example>  
        /// This sample shows how to use the <see cref="ToMoment(TimeZoneInfo, int, int)"/> method.
        /// <code> 
        /// class TestClass  
        /// { 
        ///     static void Main()  
        ///     { 
        ///         var tz = TimeZoneToMoment.ToMoment(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"), 1990, 2030);
        ///         var json = Newtonsoft.Json.JsonConvert.SerializeObject(tz);
        ///         Console.WriteLine(json);
        ///     } 
        /// } 
        /// </code> 
        /// </example> 
        /// <seealso cref="ToMoment(TimeZoneInfo)"/>
        public static MomentTimeZone ToMoment(TimeZoneInfo tz, int from, int to)
        {
            var result = new MomentTimeZone
                {
                    name = tz.Id
                };
            var untils = GetUntils(tz, from, to);
            if (!untils.Any())
            {
                return result;
            }
            DateTime? dt = null;
            var maxDate = untils.Max();
            var n = 0;
            while (untils.Any())
            {
                n++;
                if (!dt.HasValue) dt = untils.Pop();
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (dt.HasValue)
                    // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    if (dt.Value.Year > to) break;
                    dt = HandleDate(result, tz, dt.Value, maxDate);
                }
                if (n > 1000) throw new OverflowException("Error to convert timezone " + tz.Id + " (too long cycle)");
            }
            return result;
        }

        #endregion

        #region Helpers

        private static bool IsBefore(this DateTime dt, DateTime other)
        {
            return dt.CompareTo(other) < 0;
        }

        private static bool IsAfter(this DateTime dt, DateTime other)
        {
            return dt.CompareTo(other) > 0;
        }

        private static bool IsBeforeOrEq(this DateTime dt, DateTime other)
        {
            return dt.CompareTo(other) <= 0;
        }

        private static bool IsAfterOrEq(this DateTime dt, DateTime other)
        {
            return dt.CompareTo(other) >= 0;
        }

        private static long ToUnix(this DateTime time)
        {
            return (long) (time - UnixEpoch).TotalMilliseconds;
        }

        #endregion

        #region Implemantation

        private static Stack<DateTime> GetUntils(TimeZoneInfo tz, int from, int to)
        {
            var list = new List<DateTime>();
            foreach (var r in tz.GetAdjustmentRules())
            {
                list.Add(r.DateStart);
                list.Add(r.DateEnd);
            }
            var start = new DateTimeOffset(from, 1, 1, 0, 0, 0, 0, TsUtc).DateTime;
            var end = new DateTimeOffset(to, 1, 1, 0, 0, 0, 0, TsUtc).DateTime;

            if (list.Any())
            {
                if (start.IsAfter(list[0]) && start.IsBefore(list[1])) list[0] = start;
                if (end.IsBefore(list[list.Count - 1]) && end.IsAfter(list[list.Count - 2])) list[list.Count - 1] = end;
            }

            list.Reverse();
            return new Stack<DateTime>(list);
        }

        private static readonly TimeSpan TsUtc = new TimeSpan(0, 0, 0);

        private static TimeZoneInfo.AdjustmentRule FindRule(this TimeZoneInfo tz, DateTime dt)
        {
            return
                tz.GetAdjustmentRules().SingleOrDefault(x => dt.IsAfterOrEq(x.DateStart) && dt.IsBeforeOrEq(x.DateEnd));
        }

        private static DateTime GetDaylightTransition(TimeZoneInfo.TransitionTime tTime, int year)
        {
            if (tTime.IsFixedDateRule)
            {
                return
                    new DateTimeOffset(year, tTime.Month, tTime.Day, tTime.TimeOfDay.Hour, tTime.TimeOfDay.Minute,
                                       tTime.TimeOfDay.Second, tTime.TimeOfDay.Millisecond, TsUtc).DateTime;
            }
            var dates = new List<DateTime>();
            var d = new DateTimeOffset(year, tTime.Month, 1, tTime.TimeOfDay.Hour, tTime.TimeOfDay.Minute,
                                       tTime.TimeOfDay.Second, tTime.TimeOfDay.Millisecond, TsUtc).DateTime;
            while (d.Month == tTime.Month)
            {
                if (tTime.DayOfWeek == d.DayOfWeek) dates.Add(d);
                d = d.AddDays(1);
            }
            var r = (tTime.Week < 5) ? dates[tTime.Week - 1] : dates.Last();
            return r;
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private const string DeafultAbbr = "-";

        private static void Add(this MomentTimeZone mTz, DateTime? dtOpt, Int64 offset)
        {
            if (dtOpt.HasValue)
            {
                var dt = dtOpt.Value;
                var u = dt.AddMinutes(offset).ToUnix();
                if (mTz.untils.Any(x => x == u)) return;
                mTz.abbrs.Add(DeafultAbbr);
                mTz.untils.Add(u);
                mTz.offsets.Add(offset);
            }
            else
            {
                mTz.abbrs.Add(DeafultAbbr);
                mTz.untils.Add(0);
                mTz.offsets.Add(offset);
            }
        }

        private static void Swap(ref int x1, ref int x2)
        {
            var tmp = x1;
            x1 = x2;
            x2 = tmp;
        }

        private static DateTime? HandleDate(MomentTimeZone mTz, TimeZoneInfo tz, DateTime dt, DateTime maxDate)
        {
            var offset = Convert.ToInt32(tz.BaseUtcOffset.TotalMinutes);
            var rule = tz.FindRule(dt);

            if (rule != null)
            {
                var daylightDelta = Convert.ToInt32(rule.DaylightDelta.TotalMinutes);

                var transStart = GetDaylightTransition(rule.DaylightTransitionStart, dt.Year);
                var transEnd = GetDaylightTransition(rule.DaylightTransitionEnd, dt.Year);

                var isReverted = transStart.IsAfter(transEnd);
                var d1 = isReverted ? transEnd : transStart;
                var d2 = isReverted ? transStart : transEnd;

// ReSharper disable InconsistentNaming
                var _o = offset;
// ReSharper restore InconsistentNaming
                int o1, o2;

                if (daylightDelta > 0)
                {
                    o1 = -(_o);
                    o2 = -(_o + daylightDelta);
                }
                else
                {
                    o1 = -(_o - daylightDelta);
                    o2 = -(_o);
                }

                if (isReverted) Swap(ref o1, ref o2);

                mTz.Add(d1, o1);
                mTz.Add(d2, o2);

                var nextStart = d1.AddYears(1);
                var mx = maxDate < rule.DateEnd ? maxDate : rule.DateEnd;
                if (nextStart.IsBefore(mx)) return nextStart;
            }
            return null;
        }

        #endregion
    }
}