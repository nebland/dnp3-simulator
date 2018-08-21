using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Commons.Curve
{
    public class CurveLoader : IMeasurementLoader
    {
        CurveCollection m_curveCollectionLoader;

        ProxyLoader m_proxyLoader;

        public CurveLoader(CurveCollection curveCollection, ProxyLoader proxyLoader)
        {
            m_curveCollectionLoader = curveCollection;

            m_proxyLoader = proxyLoader;
        }

        public void AddCurveLoader(Curve loader)
        {
            m_proxyLoader.AddLoader(loader);
        }

        public void RemoveCurveLoader(Curve loader)
        {
            m_proxyLoader.RemoveLoader(loader);
        }

        void LoadProxy(IChangeSet updates)
        {
            ((IMeasurementLoader)m_proxyLoader).Load(updates);
        }

        void IMeasurementLoader.Load(IChangeSet updates)
        {
            ((IMeasurementLoader)m_curveCollectionLoader).Load(updates);

            ((IMeasurementLoader)m_proxyLoader).Load(updates);
        }
    }
}
