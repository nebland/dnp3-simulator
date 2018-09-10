using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatak.Simulator.DNP3.Commons.Configuration
{
    public class BinaryOutput
    {
        public string latchOff { get; set; }
        public string cdc { get; set; }
        public string dataObject { get; set; }
        public string directOperateNoAck { get; set; }
        public string close { get; set; }
        public string trip { get; set; }
        public string defaultEventClass { get; set; }
        public string countGreaterThanOne { get; set; }
        public string pulseOff { get; set; }
        public string latchOn { get; set; }
        public string defaultCmdClass { get; set; }
        public string uniqueString { get; set; }
        public string cancelCurrentOperation { get; set; }
        public string lnInst { get; set; }
        public string description { get; set; }
        public string state1 { get; set; }
        public string state0 { get; set; }
        public string pointIndex { get; set; }
        public string directOperate { get; set; }
        public string lnClass { get; set; }
        public string pulseOn { get; set; }
        public string name { get; set; }
        public string selectOperate { get; set; }

        public bool value { get; set; }
        public int quality { get; set; }
        public bool enable { get; set; }
    }
}
