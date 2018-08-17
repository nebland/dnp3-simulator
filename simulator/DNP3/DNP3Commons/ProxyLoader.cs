using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Commons
{
    public class ProxyLoader : IMeasurementLoader
    {
        IList<IMeasurementLoader> loaders;

        public void AddLoader(IMeasurementLoader loader)
        {
            this.loaders.Add(loader);
        }

        public void RemoveLoader(IMeasurementLoader loader)
        {
            this.loaders.Remove(loader);
        }

        public ProxyLoader(params IMeasurementLoader[] loaders)
        {
            this.loaders = new List<IMeasurementLoader>(loaders);
        }

        void IMeasurementLoader.Load(IChangeSet updates)
        {
            foreach (var loader in loaders)
            {
                loader.Load(updates);
            }
        }
    }
}
