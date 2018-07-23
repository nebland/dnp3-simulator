using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automatak.Simulator.Commons.Configuration
{
    public class Configuration
    {
        public List<AnalogInput> analogInputs { get; set; }
        public List<AnalogOutput> analogOutputs { get; set; }
        public List<BinaryInput> binaryInputs { get; set; }
        public List<BinaryOutput> binaryOutputs { get; set; }
        public List<IndexMap> map { get; set; }
    }
}
