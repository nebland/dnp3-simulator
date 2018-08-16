using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Commons
{
    public class CurveCollection
    {
        private IDictionary<int, Curve> m_curves = new Dictionary<int, Curve>();

        private ProxyLoader m_proxyLoader;

        private Curve m_selectedCurve = null;

        public CurveCollection(ProxyLoader proxyLoader)
        {
            m_proxyLoader = proxyLoader;

            // support storage for 10 curves
            for (int index = 1; index <= 10; index++)
            {
                m_curves[index] = new Curve();
            }
        }

        public void SelectCurve(int selectedCurveIndex)
        {
            if (m_curves.ContainsKey(selectedCurveIndex))
            {
                // remove previous curve from ProxyLoader
                if (m_selectedCurve != null)
                {
                    m_proxyLoader.RemoveLoader(m_selectedCurve);
                }

                m_selectedCurve = m_curves[selectedCurveIndex];

                // load the values of the newly selected curve into the registers
                ChangeSet selectedCurveChanges = m_selectedCurve.CreateChangeSet();

                ((IMeasurementLoader) m_proxyLoader).Load(selectedCurveChanges);

                // add new curve to ProxyLoader
                m_proxyLoader.AddLoader(m_selectedCurve);
            }
        }
    }
}
