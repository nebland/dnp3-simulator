using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Automatak.Simulator.DNP3.Commons.Configuration
{
    public class Configuration
    {
        public List<AnalogInput> analogInputs { get; set; }
        public List<AnalogOutput> analogOutputs { get; set; }
        public List<BinaryInput> binaryInputs { get; set; }
        public List<BinaryOutput> binaryOutputs { get; set; }
        public List<Counter> counters { get; set; }
        public List<AnalogIndexMap> analogIndexMap { get; set; }
        public List<BinaryIndexMap> binaryIndexMap { get; set; }

        public static Configuration LoadConfiguration()
        {
            string text = System.IO.File.ReadAllText(@"config\indexes-config.json");

            Configuration configuration = JsonConvert.DeserializeObject<Configuration>(text);

            return configuration;
        }
    }
}
