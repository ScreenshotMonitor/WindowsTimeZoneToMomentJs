using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Pranas.WindowsTimeZoneToMomentJs;

namespace WinTzToMoment.Controllers
{
    public class DefaultController : Controller
    {
        // /default/index
        public ActionResult Index()
        {
            return View();
        }

       

        // /default/index
        public ActionResult WindowsTimeZoneToMomentJs()
        {
            var zones = TimeZoneInfo.GetSystemTimeZones().ToList();
            zones = zones.Where(x => TimeZoneToMomentConverter.Problems.Contains(x.Id)).ToList();
            //zones = zones.Where(x => x.GetAdjustmentRules().Any()).ToList();
            var list = zones.Select(TimeZoneToMomentConverter.ToMoment).ToList();
            var arr = MomentTimeZoneJsonConverter.ToJsonArray(list, x => x.ToJson());
            var script = "window.windowsTimezones = " + arr;
            return Content(script, "text/javascript");
        }

        public const string DateTimeFmt = "yyyy-MM-ddTHH:mm:ssK";

        public ActionResult Convert(string id, string dt)
        {
            var dto = DateTimeOffset.ParseExact(dt, DateTimeFmt, Thread.CurrentThread.CurrentCulture);
            var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
            var d = TimeZoneInfo.ConvertTimeFromUtc(dto.DateTime, tz);
            return Content("{" + string.Format(" \"value\" : \"{0}\", \"offset\": \"{1}\"", new DateTimeOffset(d, tz.GetUtcOffset(d)).ToString(DateTimeFmt), tz.BaseUtcOffset) + "}", "application/json");
        }
    }
}