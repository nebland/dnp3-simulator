using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;
using Automatak.Simulator.DNP3.API;

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
			get { return new OutstationStackConfig(); }
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
