using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Commons.Curve
{
    public class CurveException : Exception
    {
        public CurveException(CommandStatus commandStatus, string message) : base(message)
        {
            this.CommandStatus = commandStatus;
        }

        public CommandStatus CommandStatus { get; private set; }
    }
}
