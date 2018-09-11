using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatak.Simulator.DNP3.Commons.Configuration
{
    public class Counter
    {
        public string frozenCounterExists { get; set; }
        public string frozenCounterEventDefaultClass { get; set; }
        public string pointIndex { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public uint value { get; set; }
        public int quality { get; set; }
        public bool enable { get; set; }
    }
}
