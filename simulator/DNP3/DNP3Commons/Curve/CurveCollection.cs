using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;
using System.Windows.Forms;

namespace Automatak.Simulator.DNP3.Commons.Curve
{
    /*
     * Track data associated with a curve and manage multiple curves
     * 
     * AO173 - Volt-Watt Curve Index
     * AO186 - Frequency-Watt Curve Index
     * AO217 - Volt-VAR Curve Index
     * AO226 - Watt-VAr Curve Index
     * 

     **/
    public class CurveCollection : IMeasurementLoader, IDatabase
    {
        // storage for registers associated with curves
        private IDictionary<ushort, AnalogOutputStatus> m_analogOutputMeasurements = new SortedDictionary<ushort, AnalogOutputStatus>();
        private IDictionary<ushort, Binary> m_binaryInputMeasurements = new SortedDictionary<ushort, Binary>();

        private IList<Curve> m_curves = new List<Curve>();

        public CurveLoader Loader { get; private set; }

        private Curve m_selectedCurve = null;

        public enum AnalogOutputPoint : ushort
        {
            VOLT_WATT_CURVE_INDEX = 173,
            FREQUENCY_WATT_CURVE_INDEX = 186,
            VOLT_VAR_CURVE_INDEX = 217,
            WATT_VAR_CURVE_INDEX = 226,
            CURVE_EDIT_SELECTOR = 244
        };

        public enum BinaryInputPoint : ushort
        {
            SELECTED_CURVE_IS_REFERENCED_BY_A_MODE = 107
        };

        public CurveCollection(Configuration.Configuration configuration, ProxyLoader proxyLoader)
        {
            Loader = new CurveLoader(this, proxyLoader);

            // support storage for 10 curves
            for (int index = 0; index < 10; index++)
            {
                m_curves.Add(new Curve());
            }

            // select first curve to prevent needing to check for null in SelectCurve
            // (the default selected curve will be set from configuration file at 
            // end of this constructor)
            m_selectedCurve = m_curves[0];
            Loader.AddCurveLoader(m_selectedCurve);

            //
            // initialize tracked data from configuration file
            //
            ushort defaultSelectedCurveIndex = (ushort)configuration.analogOutputsMap[(ushort)AnalogOutputPoint.CURVE_EDIT_SELECTOR].value;

            //
            // create storage for data points associated with curves
            //
            byte quality = 0x01;
            DateTime dateTime = DateTime.Now;

            //
            // curve index registers
            //
            m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_WATT_CURVE_INDEX] = new AnalogOutputStatus(configuration.analogOutputsMap[(ushort)AnalogOutputPoint.VOLT_WATT_CURVE_INDEX].value, quality, dateTime);
            m_analogOutputMeasurements[(ushort)AnalogOutputPoint.FREQUENCY_WATT_CURVE_INDEX] = new AnalogOutputStatus(configuration.analogOutputsMap[(ushort)AnalogOutputPoint.FREQUENCY_WATT_CURVE_INDEX].value, quality, dateTime);
            m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_VAR_CURVE_INDEX] = new AnalogOutputStatus(configuration.analogOutputsMap[(ushort)AnalogOutputPoint.VOLT_VAR_CURVE_INDEX].value, quality, dateTime);
            m_analogOutputMeasurements[(ushort)AnalogOutputPoint.WATT_VAR_CURVE_INDEX] = new AnalogOutputStatus(configuration.analogOutputsMap[(ushort)AnalogOutputPoint.WATT_VAR_CURVE_INDEX].value, quality, dateTime);

            m_analogOutputMeasurements[(ushort)AnalogOutputPoint.CURVE_EDIT_SELECTOR] = new AnalogOutputStatus(defaultSelectedCurveIndex, quality, dateTime);

            m_binaryInputMeasurements[(ushort)BinaryInputPoint.SELECTED_CURVE_IS_REFERENCED_BY_A_MODE] = new Binary(configuration.binaryInputsMap[(ushort)BinaryInputPoint.SELECTED_CURVE_IS_REFERENCED_BY_A_MODE].value, quality, dateTime);
            
            // now "select" the curve from the configuration file
            SelectCurve(defaultSelectedCurveIndex);
        }

        public bool IsSelectedCurveIndexValid(int selectedCurveIndex)
        {
            return (selectedCurveIndex >= 1) && (selectedCurveIndex <= m_curves.Count);
        }

        public void SelectCurve(int selectedCurveIndex)
        {
            if (!IsSelectedCurveIndexValid(selectedCurveIndex))
            {
                throw new CurveException(CommandStatus.OUT_OF_RANGE, "Given curve index '" + selectedCurveIndex.ToString() + "' is outside of range 1 to " + m_curves.Count);
            }

            // remove previous curve from ProxyLoader so it won't be written to
            Loader.RemoveCurveLoader(m_selectedCurve);

            m_selectedCurve = m_curves[selectedCurveIndex-1];

            // load the values of the newly selected curve into the registers
            ChangeSet selectedCurveChanges = m_selectedCurve.CreateChangeSet();

            ((IMeasurementLoader)Loader).Load(selectedCurveChanges);

            // add the selected curve to the ProxyLoader
            Loader.AddCurveLoader(m_selectedCurve);
        }

        void IMeasurementLoader.Load(IChangeSet updates)
        {
            updates.Apply(this);
        }

        void IDatabase.Update(Binary update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(DoubleBitBinary update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(Analog update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(Counter update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(FrozenCounter update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(BinaryOutputStatus update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(AnalogOutputStatus update, ushort index, EventMode mode)
        {
            if (m_analogOutputMeasurements.ContainsKey(index))
            {
                if (index == (ushort)AnalogOutputPoint.CURVE_EDIT_SELECTOR)
                {
                    SelectCurve((int)update.Value);
                }

                m_analogOutputMeasurements[index] = update;
            }
        }

        void IDatabase.Update(TimeAndInterval update, ushort index)
        {
        }
    }
}
