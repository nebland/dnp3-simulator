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

                OutstationStackConfig config = new OutstationStackConfig();

                config.databaseTemplate = new DatabaseTemplate(
                    (ushort) configuration.binaryInputs.Count, 
                    1, 
                    (ushort) configuration.analogInputs.Count, 
                    (ushort) configuration.counters.Count, 
                    0, 
                    (ushort) configuration.binaryOutputs.Count, 
                    (ushort) configuration.analogOutputs.Count, 
                    11);

                foreach (AnalogOutput analogOutput in configuration.analogOutputs)
                {
                    ushort index = Configuration.covertIndex(analogOutput.pointIndex);

                    config.databaseTemplate.analogOutputStatii[index].clazz = PointClass.Class0;

                    config.databaseTemplate.analogOutputStatii[index].staticVariation = StaticAnalogOutputStatusVariation.Group40Var3;
                }

                foreach (BinaryInput binaryInput in configuration.binaryInputs)
                {
                    ushort index = Configuration.covertIndex(binaryInput.pointIndex);

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
