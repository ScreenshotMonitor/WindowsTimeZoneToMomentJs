using System.Collections.Generic;

namespace Pranas.WindowsTimeZoneToMomentJs
{
    /// <summary>
    /// The zone object in unpacked format 
    /// <a href="http://momentjs.com/timezone/docs/#/data-formats/">http://momentjs.com/timezone/docs/#/data-formats/</a> 
    /// </summary>
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