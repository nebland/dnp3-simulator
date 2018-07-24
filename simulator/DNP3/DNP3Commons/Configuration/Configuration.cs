using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

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
        private static Dictionary<ushort, PointClass> pointClass { get; set; }

        /*
         * Convert index from format like "AI0" to a number
         */
        public static ushort covertIndex(string index)
        {
            string indexNumber = index.Substring(2, index.Count() - 2);

            return ushort.Parse(indexNumber);
        }

        public static PointClass convertPointClass(string pointIndexString)
        {
            if (String.IsNullOrEmpty(pointIndexString))
            {
                return PointClass.Class1;
            }

            ushort pointIndex = ushort.Parse(pointIndexString);

            if (!Configuration.pointClass.ContainsKey(pointIndex))
            {
                return PointClass.Class1;
            }

            return Configuration.pointClass[pointIndex];
        }

        static Configuration()
        {
            //
            // convert point class numbers to PointClass type
            //
            Configuration.pointClass = new Dictionary<ushort, PointClass>();

            Configuration.pointClass[0] = PointClass.Class0;
            Configuration.pointClass[1] = PointClass.Class1;
            Configuration.pointClass[2] = PointClass.Class2;
            Configuration.pointClass[3] = PointClass.Class3;
        }

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
                ushort aiIndexNumber = Configuration.covertIndex(map.aiIndex);
                ushort aoIndexNumber = Configuration.covertIndex(map.aoIndex);

                configuration.analogIndexOutputToInput[aoIndexNumber] = aiIndexNumber;
            }

            configuration.binaryIndexOutputToInput = new Dictionary<ushort, ushort>();

            foreach (BinaryIndexMap map in configuration.binaryIndexMap)
            {
                ushort biIndexNumber = Configuration.covertIndex(map.biIndex);
                ushort boIndexNumber = Configuration.covertIndex(map.boIndex);

                configuration.binaryIndexOutputToInput[boIndexNumber] = biIndexNumber;
            }

            return configuration;
        }
    }
}
