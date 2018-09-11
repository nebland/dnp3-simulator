using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;
using Automatak.Simulator.DNP3.API;
using Automatak.Simulator.DNP3.Commons;
using Automatak.Simulator.DNP3.Commons.Configuration;
using Automatak.Simulator.DNP3.DEROutstationPlugin;

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
                Configuration configuration = Configuration.LoadConfiguration();

                OutstationStackConfig stackConfig = new OutstationStackConfig();

                //
                // count the number of "enabled" points for the DatabaseTemplate constructor
                //

                int enabledAnalogInputCount = 0;
                int enabledAnalogOutputCount = 0;
                int enabledBinaryInputCount = 0;
                int enabledBinaryOutputCount = 0;
                int enabledCountersCount = 0;

                foreach (AnalogInput analogInput in configuration.analogInputs)
                {
                    if (analogInput.enable)
                    {
                        enabledAnalogInputCount += 1;
                    }
                }

                foreach (AnalogOutput analogOutput in configuration.analogOutputs)
                {
                    if (analogOutput.enable)
                    {
                        enabledAnalogOutputCount += 1;
                    }
                }

                foreach (BinaryInput binaryInput in configuration.binaryInputs)
                {
                    if (binaryInput.enable)
                    {
                        enabledBinaryInputCount += 1;
                    }
                }

                foreach (BinaryOutput binaryOutput in configuration.binaryOutputs)
                {
                    if (binaryOutput.enable)
                    {
                        enabledBinaryOutputCount += 1;
                    }
                }

                foreach (Commons.Configuration.Counter counter in configuration.counters)
                {
                    if (counter.enable)
                    {
                        enabledCountersCount += 1;
                    }
                }

                stackConfig.databaseTemplate = new DatabaseTemplate(
                    (ushort) enabledBinaryInputCount, 
                    1, 
                    (ushort) enabledAnalogInputCount, 
                    (ushort) enabledCountersCount, 
                    0, 
                    (ushort) enabledBinaryOutputCount, 
                    (ushort) enabledAnalogOutputCount, 
                    11);

                //
                // initialize DatabaseTemplate from Configuration, manually assigning 
                // the points to their corresponding arrays (this will automatically 
                // make the DatabaseTemplate IndexMode Discontiguous
                //

                int arrayIndex = 0;

                foreach (AnalogInput analogInput in configuration.analogInputs)
                {
                    if (analogInput.enable)
                    {
                        ushort pointIndex = Configuration.covertIndex(analogInput.pointIndex);

                        // config.databaseTemplate.analogs[arrayIndex].clazz = PointClass.Class0;
                        // config.databaseTemplate.analogs[arrayIndex].staticVariation = StaticAnalogOutputStatusVariation.Group40Var3;
                        stackConfig.databaseTemplate.analogs[arrayIndex].index = pointIndex;

                        arrayIndex += 1;
                    }
                }

                arrayIndex = 0;

                foreach (AnalogOutput analogOutput in configuration.analogOutputs)
                {
                    if (analogOutput.enable)
                    {
                        ushort pointIndex = Configuration.covertIndex(analogOutput.pointIndex);

                        stackConfig.databaseTemplate.analogOutputStatii[arrayIndex].clazz = PointClass.Class0;
                        stackConfig.databaseTemplate.analogOutputStatii[arrayIndex].staticVariation = StaticAnalogOutputStatusVariation.Group40Var3;
                        stackConfig.databaseTemplate.analogOutputStatii[arrayIndex].index = pointIndex;

                        arrayIndex += 1;
                    }
                }

                arrayIndex = 0;

                foreach (BinaryInput binaryInput in configuration.binaryInputs)
                {
                    if (binaryInput.enable)
                    {
                        ushort pointIndex = Configuration.covertIndex(binaryInput.pointIndex);

                        stackConfig.databaseTemplate.binaries[arrayIndex].clazz = PointClass.Class1;
                        stackConfig.databaseTemplate.binaries[arrayIndex].staticVariation = StaticBinaryVariation.Group1Var2;
                        stackConfig.databaseTemplate.binaries[arrayIndex].eventVariation = EventBinaryVariation.Group2Var2;
                        stackConfig.databaseTemplate.binaries[arrayIndex].index = pointIndex;

                        arrayIndex += 1;
                    }
                }

                arrayIndex = 0;

                foreach (BinaryOutput binaryOutput in configuration.binaryOutputs)
                {
                    if (binaryOutput.enable)
                    {
                        ushort pointIndex = Configuration.covertIndex(binaryOutput.pointIndex);

                        // stackConfig.databaseTemplate.binaryOutputStatii[arrayIndex].clazz = PointClass.Class1;
                        // stackConfig.databaseTemplate.binaryOutputStatii[arrayIndex].staticVariation = StaticBinaryVariation.Group1Var2;
                        // stackConfig.databaseTemplate.binaryOutputStatii[arrayIndex].eventVariation = EventBinaryVariation.Group2Var2;
                        stackConfig.databaseTemplate.binaryOutputStatii[arrayIndex].index = pointIndex;

                        arrayIndex += 1;
                    }
                }

                arrayIndex = 0;

                foreach (Commons.Configuration.Counter counter in configuration.counters)
                {
                    if (counter.enable)
                    {
                        ushort pointIndex = Configuration.covertIndex(counter.pointIndex);

                        // stackConfig.databaseTemplate.binaryOutputStatii[arrayIndex].clazz = PointClass.Class1;
                        // stackConfig.databaseTemplate.binaryOutputStatii[arrayIndex].staticVariation = StaticBinaryVariation.Group1Var2;
                        // stackConfig.databaseTemplate.binaryOutputStatii[arrayIndex].eventVariation = EventBinaryVariation.Group2Var2;
                        stackConfig.databaseTemplate.counters[arrayIndex].index = pointIndex;

                        arrayIndex += 1;
                    }
                }


                return stackConfig;
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
            return new OutstationFactory();
        }
    }

    class OutstationFactory : IOutstationFactory
    {
        private readonly ProxyCommandHandler m_commandHandler = new ProxyCommandHandler();
        private readonly EventedOutstationApplication m_application = new EventedOutstationApplication();

        public IOutstationApplication Application
        {
            get { return m_application; }
        }

        public ICommandHandler CommandHandler
        {
            get { return m_commandHandler; }
        }

        public IOutstationInstance CreateInstance(IOutstation outstation, string name, OutstationStackConfig config)
        {
            return new OutstationInstance(m_commandHandler, m_application, outstation, config, name);
        }
    }


    class OutstationInstance : IOutstationInstance
    {
        readonly ProxyCommandHandler handler;
        readonly EventedOutstationApplication application;
        readonly IOutstation outstation;
        readonly string alias;
        readonly MeasurementCache cache;

        OutstationForm form = null;

        public OutstationInstance(ProxyCommandHandler handler, EventedOutstationApplication application, IOutstation outstation, OutstationStackConfig config, string alias)
        {
            this.handler = handler;
            this.application = application;
            this.outstation = outstation;
            this.alias = alias;

            this.cache = new MeasurementCache(config.databaseTemplate);
        }

        string IOutstationInstance.DisplayName
        {
            get { return alias; }
        }

        bool IOutstationInstance.HasForm
        {
            get { return true; }
        }

        bool IOutstationInstance.ShowFormOnCreation
        {
            get { return true; }
        }

        void IOutstationInstance.ShowForm()
        {
            if (this.form == null)
            {
                this.form = new OutstationForm(outstation, application, cache, handler, alias);
            }

            form.Show();
        }

        void IOutstationInstance.Shutdown()
        {
            if (form != null)
            {
                form.Close();
                form.Dispose();
                form = null;
            }
        }
    }
}
