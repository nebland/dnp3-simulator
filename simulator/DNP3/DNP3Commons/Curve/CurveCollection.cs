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
     * This class is not a IMeausermentLoader, and thus cannot be added to the ProxyLoader, 
     * because it must be able to manipulate the objects being loaded (the curves) 
     * when a ChangeSet is applied. The Load function applies changes to this class
     * first, then updates the ChangeSet with additional changes affected by associated 
     * curve data, then applies these final changes to the ProxyLoader.
     * 
     * AO173 - Volt-Watt Curve Index
     * AO186 - Frequency-Watt Curve Index
     * AO217 - Volt-VAR Curve Index
     * AO226 - Watt-VAr Curve Index
     * 
     **/
    public class CurveCollection : IDatabase
    {
        // storage for registers associated with curves
        private IDictionary<ushort, AnalogOutputStatus> m_analogOutputMeasurements = new SortedDictionary<ushort, AnalogOutputStatus>();
        private IDictionary<ushort, Binary> m_binaryInputMeasurements = new SortedDictionary<ushort, Binary>();
        private IDictionary<ushort, BinaryOutputStatus> m_binaryOutputMeasurements = new SortedDictionary<ushort, BinaryOutputStatus>();

        private IList<Curve> m_curves = new List<Curve>();

        private ProxyLoader m_proxyLoader;

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

        public enum BinaryOutputPoint : ushort
        {
            ENABLE_VOLT_WATT_MODE = 25,
            ENABLE_FREQUENCY_WATT_MODE = 26,
            ENABLE_VOLT_VAR_MODE = 29,
            ENABLE_WATT_VAR_MODE = 30
        };

        public CurveCollection(Configuration.Configuration configuration, ProxyLoader proxyLoader)
        {
            m_proxyLoader = proxyLoader;

            // support storage for 20 curves
            for (int index = 0; index < 20; index++)
            {
                m_curves.Add(new Curve());
            }

            // select first curve to prevent needing to check for null in SelectCurve
            // (the default selected curve will be set from configuration file at 
            // end of this constructor)
            m_selectedCurve = m_curves[0];
            m_proxyLoader.AddLoader(m_selectedCurve);

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

            m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_VOLT_WATT_MODE] = new BinaryOutputStatus(configuration.binaryOutputsMap[(ushort)BinaryOutputPoint.ENABLE_VOLT_WATT_MODE].value, quality, dateTime);
            m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_FREQUENCY_WATT_MODE] = new BinaryOutputStatus(configuration.binaryOutputsMap[(ushort)BinaryOutputPoint.ENABLE_FREQUENCY_WATT_MODE].value, quality, dateTime);
            m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_VOLT_VAR_MODE] = new BinaryOutputStatus(configuration.binaryOutputsMap[(ushort)BinaryOutputPoint.ENABLE_VOLT_VAR_MODE].value, quality, dateTime);
            m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_WATT_VAR_MODE] = new BinaryOutputStatus(configuration.binaryOutputsMap[(ushort)BinaryOutputPoint.ENABLE_WATT_VAR_MODE].value, quality, dateTime);

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
            m_proxyLoader.RemoveLoader(m_selectedCurve);

            m_selectedCurve = m_curves[selectedCurveIndex-1];

            // load the values of the newly selected curve into the registers
            ChangeSet selectedCurveChanges = m_selectedCurve.CreateChangeSet();

            ((IMeasurementLoader)m_proxyLoader).Load(selectedCurveChanges);

            // add the selected curve to the ProxyLoader
            m_proxyLoader.AddLoader(m_selectedCurve);
        }

        void UpdateReferencedCurveRegister(ChangeSet updates)
        {
            ushort curveEditSelector = (ushort)(m_analogOutputMeasurements[(ushort)AnalogOutputPoint.CURVE_EDIT_SELECTOR].Value);

            bool result =
               (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_WATT_CURVE_INDEX].Value))
            || (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.FREQUENCY_WATT_CURVE_INDEX].Value))
            || (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_VAR_CURVE_INDEX].Value))
            || (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.WATT_VAR_CURVE_INDEX].Value));

            byte quality = 0x01;
            DateTime dateTime = DateTime.Now;

            m_binaryInputMeasurements[(ushort)BinaryInputPoint.SELECTED_CURVE_IS_REFERENCED_BY_A_MODE] = new Binary(result, quality, dateTime);
            updates.Update(new Binary(result, 0, DateTime.Now), (ushort)BinaryInputPoint.SELECTED_CURVE_IS_REFERENCED_BY_A_MODE);

            UpdateEnabledCurve();
        }

        void UpdateEnabledCurve()
        {
            // if the curve is referenced and enabled, then it cannot be written to

            bool selectedCurveReferencedByMode = m_binaryInputMeasurements[(ushort)BinaryInputPoint.SELECTED_CURVE_IS_REFERENCED_BY_A_MODE].Value;

            bool enable = true;

            if (selectedCurveReferencedByMode)
            {
                // find the mode referencing the curve so we can check if it is enabled
                ushort curveEditSelector = (ushort)(m_analogOutputMeasurements[(ushort)AnalogOutputPoint.CURVE_EDIT_SELECTOR].Value);

                if (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_WATT_CURVE_INDEX].Value))
                {
                    enable = !m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_VOLT_WATT_MODE].Value;
                }
                else if (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.FREQUENCY_WATT_CURVE_INDEX].Value))
                {
                    enable = !m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_FREQUENCY_WATT_MODE].Value;
                }
                else if (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_VAR_CURVE_INDEX].Value))
                {
                    enable = !m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_VOLT_VAR_MODE].Value;
                }
                else if (curveEditSelector == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.WATT_VAR_CURVE_INDEX].Value))
                {
                    enable = !m_binaryOutputMeasurements[(ushort)BinaryOutputPoint.ENABLE_WATT_VAR_MODE].Value;
                }
            }

            m_selectedCurve.Enable = enable;
        }

        public void Load(ChangeSet updates)
        {
            // first apply ChangeSet to this class
            ((IChangeSet)updates).Apply(this);

            //
            // now update associated registers that may change based on the ChangeSet,
            // and update the ChangeSet with the changes
            //
            UpdateReferencedCurveRegister(updates);

            // lastly apply all changes to everything else
            ((IMeasurementLoader)m_proxyLoader).Load(updates);
        }

        void IDatabase.Update(Binary update, ushort index, EventMode mode)
        {
            // the binary input tracked by this class is not set externally
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
            if (m_binaryOutputMeasurements.ContainsKey(index))
            {
                m_binaryOutputMeasurements[index] = update;
            }
        }

        void IDatabase.Update(AnalogOutputStatus update, ushort index, EventMode mode)
        {
            if (m_analogOutputMeasurements.ContainsKey(index))
            {
                if (index == (ushort)AnalogOutputPoint.CURVE_EDIT_SELECTOR)
                {
                    SelectCurve((int)update.Value);
                }
                else if (index == (ushort)AnalogOutputPoint.FREQUENCY_WATT_CURVE_INDEX
                    || index == (ushort)AnalogOutputPoint.VOLT_VAR_CURVE_INDEX
                    || index == (ushort)AnalogOutputPoint.VOLT_WATT_CURVE_INDEX
                    || index == (ushort)AnalogOutputPoint.WATT_VAR_CURVE_INDEX)
                {
                    // make sure none of the curve indexes have the requested value

                    int value = (int)update.Value;

                    if (value != 0)
                    {
                        if ((value == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_WATT_CURVE_INDEX].Value))
                            || (value == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.FREQUENCY_WATT_CURVE_INDEX].Value))
                            || (value == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.VOLT_VAR_CURVE_INDEX].Value))
                            || (value == ((ushort)m_analogOutputMeasurements[(ushort)AnalogOutputPoint.WATT_VAR_CURVE_INDEX].Value)))
                        {
                            throw new CurveException(CommandStatus.BLOCKED, "Cannot set AO" + index.ToString() + " to " + value.ToString() + " because it is assigned to another curve index");
                        }
                    }
                }

                m_analogOutputMeasurements[index] = update;
            }
        }

        void IDatabase.Update(TimeAndInterval update, ushort index)
        {
        }
    }
}
