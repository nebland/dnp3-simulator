using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;
using System.Windows.Forms;

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

            m_selectedCurve = m_curves[1];
        }

        public bool IsSelectedCurveIndexValid(int selectedCurveIndex)
        {
            return m_curves.ContainsKey(selectedCurveIndex);
        }

        public CommandStatus SelectCurve(int selectedCurveIndex)
        {
            if (!IsSelectedCurveIndexValid(selectedCurveIndex))
            {
                return CommandStatus.OUT_OF_RANGE;
            }

            // remove previous curve from ProxyLoader so it won't be written to
            m_proxyLoader.RemoveLoader(m_selectedCurve);

            m_selectedCurve = m_curves[selectedCurveIndex];

            // load the values of the newly selected curve into the registers
            ChangeSet selectedCurveChanges = m_selectedCurve.CreateChangeSet();

            ((IMeasurementLoader) m_proxyLoader).Load(selectedCurveChanges);

            // add the newly selected curve to ProxyLoader so it can be written to
            m_proxyLoader.AddLoader(m_selectedCurve);

            return CommandStatus.SUCCESS;
        }
    }
}
