using System.Collections.Generic;

namespace Pranas.WindowsTimeZoneToMomentJs
{
    // ReSharper disable InconsistentNaming
    public class MomentTimeZone
    {
        public string name { get; set; }
        public List<string> abbrs { get; set; }
        public List<long> untils { get; set; }
        public List<long> offsets { get; set; }

        public MomentTimeZone()
        {
            untils = new List<long>();
            offsets = new List<long>();
            abbrs = new List<string>();
        }
    }
}