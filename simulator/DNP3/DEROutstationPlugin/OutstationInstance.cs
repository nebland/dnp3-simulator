using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;
using Automatak.Simulator.DNP3.API;
using Automatak.Simulator.DNP3.Commons;

namespace Automatak.Simulator.DNP3.DerOutstationPlugin
{
    public class OutstationModule : IOutstationModule
    {
        const string m_name = "DerOutstation";

        private static OutstationModule instance = new OutstationModule();

        public static IOutstationModule Instance
        {
            get { return instance; }
        }

        private OutstationModule()
        { }

        public override string ToString()
        {
            return m_name;
        }

        public bool AllowTemplateEditing
        {
            get { return false; }
        }

        public Automatak.DNP3.Interface.OutstationStackConfig DefaultConfig
        {
            get
            {
                OutstationStackConfig config = new OutstationStackConfig();

                config.databaseTemplate = new DatabaseTemplate(76, 1, 74, 4, 0, 46, 917, 11);

                config.outstation.config.allowUnsolicited = true;

                config.outstation.config.unsolClassMask = ClassField.AllClasses;

                for (int index = 0; index < config.databaseTemplate.analogOutputStatii.Count; index++)
                {
                    config.databaseTemplate.analogOutputStatii[index].clazz = PointClass.Class0;

                    config.databaseTemplate.analogOutputStatii[index].staticVariation = StaticAnalogOutputStatusVariation.Group40Var3;
                }

                for (int index = 0; index < config.databaseTemplate.binaryOutputStatii.Count; index++)
                {
                    config.databaseTemplate.binaryOutputStatii[index].clazz = PointClass.Class0;

                    config.databaseTemplate.binaryOutputStatii[index].staticVariation = StaticBinaryOutputStatusVariation.Group10Var2;
                }

                for (int index = 0; index < config.databaseTemplate.analogs.Count; index++)
                {
                    config.databaseTemplate.analogs[index].clazz = PointClass.Class1;

                    config.databaseTemplate.analogs[index].eventVariation = EventAnalogVariation.Group32Var7;

                    config.databaseTemplate.analogs[index].staticVariation = StaticAnalogVariation.Group30Var5;
                }

                for (int index = 0; index < config.databaseTemplate.binaries.Count; index++)
                {
                    config.databaseTemplate.binaries[index].clazz = PointClass.Class1;

                    config.databaseTemplate.binaries[index].staticVariation = StaticBinaryVariation.Group1Var2;

                    config.databaseTemplate.binaries[index].eventVariation = EventBinaryVariation.Group2Var2;
                }

                return config;
            }
        }

        public string DefaultLogName
        {
            get { return "der-outstation"; }
        }

        public string Description
        {
            get { return "A der outstation simulator"; }
        }

        string IOutstationModule.Name
        {
            get { return m_name; }
        }

        public IOutstationFactory CreateFactory()
        {
            throw new NotImplementedException();
        }
    }
}
