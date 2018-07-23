using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatak.Simulator.DNP3.Commons.Configuration
{
    public class AnalogInput
    {
        public string defaultEventClass { get; set; }
        public string lninst { get; set; }
        public string name { get; set; }
        public string reference { get; set; }
        public string cdc { get; set; }
        public string offSet { get; set; }
        public string maximum { get; set; }
        public string dataObject { get; set; }
        public string minimum { get; set; }
        public string pointIndex { get; set; }
        public string multiplier { get; set; }
        public string units { get; set; }
        public string uniqueString { get; set; }
        public string resolution { get; set; }
        public string lnClass { get; set; }
        public string description { get; set; }

    }
}
