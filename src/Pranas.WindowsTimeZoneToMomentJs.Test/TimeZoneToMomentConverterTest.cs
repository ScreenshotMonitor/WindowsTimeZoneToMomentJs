using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NodaTime;
using NodaTime.TimeZones;

using Noesis.Javascript;
using NUnit.Framework;

namespace Pranas.WindowsTimeZoneToMomentJs.Test
{
    [TestFixture]
    public class TimeZoneToMomentConverterTest
    {
        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        [Test]
        public void TestTimeZones()
        {
            var zones = TimeZoneInfo.GetSystemTimeZones();

            foreach (var tz in zones)
            {
                TestTimeZone(tz);
            }
        }

        private void TestTimeZone(TimeZoneInfo tz)
        {
            const int yearFrom = 2010;
            int yearTo = DateTime.Now.Year + 10;

            var script = TimeZoneToMomentConverter.GenerateAddMomentZoneScript(tz, yearFrom, yearTo);

            Trace.WriteLine(tz.Id);
            Trace.WriteLine(script);

            var data = EnumerateTestDateTime(tz, yearFrom, yearTo);

            using (var context = new JavascriptContext())
            {
                context.Run(File.ReadAllText(@"..\..\moment\moment.js"));
                context.Run(File.ReadAllText(@"..\..\moment\moment-timezone.js"));

                context.Run(script);

                string windowsValue = null;

                context.SetParameter("test", new Action<string>(s =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        Assert.That(s, Is.EqualTo(windowsValue));
                        //Trace.WriteLine(s);
                    })
                );

                foreach (var time in data)
                {
                    // time should convert equally from UTC in both JavaScript and .NET
                    var localTime = Instant.FromDateTimeOffset(time).InZone(BclDateTimeZone.FromTimeZoneInfo(tz)).ToDateTimeOffset();
                    // so it will look like 2010-03-14T03:00:00-08:00
                    windowsValue = localTime.ToString("O").Replace(".0000000", string.Empty);

                    long utcUnix = (long)(time - UnixEpoch).TotalMilliseconds;
                    //Trace.WriteLine("");
                    //Trace.WriteLine(windowsValue);
                    context.Run(string.Format("test(moment.utc({0}).tz('{1}').format());", utcUnix, tz.Id));
                }

            }
        }

        private static IEnumerable<DateTimeOffset> EnumerateTestDateTime(TimeZoneInfo timeZone, int yearFrom, int yearTo)
        {
            // return utc test times
            int maxStep = (int)TimeSpan.FromDays(60).TotalMinutes;
            Func<DateTimeOffset, int> offset = t => (int)TimeZoneInfo.ConvertTime(t, timeZone).Offset.TotalMinutes;

            var t1 = new DateTimeOffset(yearFrom, 1, 1, 0, 0, 0, TimeSpan.Zero);

            while (t1.Year <= yearTo)
            {
                int step = maxStep;

                var t2 = t1.AddMinutes(step);
                while (offset(t1) != offset(t2) && step > 1)
                {
                    step = step / 2;
                    t2 = t1.AddMinutes(step);
                }

                yield return t1;
                if (step == 1 && offset(t1) != offset(t2))
                {
                    yield return t2;
                }
                t1 = t2;
            }
        }
    }
}
