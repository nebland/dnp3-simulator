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
        // containers populated from json configuration file
        public List<AnalogInput> analogInputs { get; set; }
        public List<AnalogOutput> analogOutputs { get; set; }
        public List<BinaryInput> binaryInputs { get; set; }
        public List<BinaryOutput> binaryOutputs { get; set; }
        public List<Counter> counters { get; set; }
        public List<AnalogIndexMap> analogIndexMap { get; set; }
        public List<BinaryIndexMap> binaryIndexMap { get; set; }

        // additional containers created for convenience
        public Dictionary<ushort, ushort> analogIndexOutputToInput { get; private set; }
        public Dictionary<ushort, ushort> binaryIndexOutputToInput { get; private set; }

        public static Configuration LoadConfiguration()
        {
            //
            // read json from disk
            //
            string text = System.IO.File.ReadAllText(@"config\indexes-config.json");

            //
            // populate configuration instance from json data
            //
            Configuration configuration = JsonConvert.DeserializeObject<Configuration>(text);

            //
            // populate convenience containers
            //
            configuration.analogIndexOutputToInput = new Dictionary<ushort, ushort>();

            foreach (AnalogIndexMap map in configuration.analogIndexMap)
            {
                string aiIndexNumber = map.aiIndex.Substring(2, map.aiIndex.Count() - 2);
                string aoIndexNumber = map.aoIndex.Substring(2, map.aoIndex.Count() - 2);

                configuration.analogIndexOutputToInput[ushort.Parse(aoIndexNumber)] = ushort.Parse(aiIndexNumber);
            }

            configuration.binaryIndexOutputToInput = new Dictionary<ushort, ushort>();

            foreach (BinaryIndexMap map in configuration.binaryIndexMap)
            {
                string biIndexNumber = map.biIndex.Substring(2, map.biIndex.Count() - 2);
                string boIndexNumber = map.boIndex.Substring(2, map.boIndex.Count() - 2);

                configuration.binaryIndexOutputToInput[ushort.Parse(boIndexNumber)] = ushort.Parse(biIndexNumber);
            }

            return configuration;
        }
    }
}
