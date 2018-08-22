using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Commons.Curve
{
    /*
     * Generic curve points
     * 
     * AO245 - Curve Mode Type
     * AO246 - Number Of Curve Points
     * AO247 - Independent (X-Value) Units for Curve
     * AO248 - Dependent (Y-Value) Units for Curve
     * AO249 through AO447 - X and Y-Values of each curve
     * 
     * AI330 - Curve Number of Points
     * 
     * Enumeration Values
     * AO245
     *   <0> Curve disabled
     *   <1> Not applicable / Unknown
     *   <2> Volt-Var modes
     *   <3> Frequency-Watt mode
     *   <4> Watt-VAr mode
     *   <5> Voltage-Watt modes
     *   <6> Remain Connected
     *   <7> Temperature mode
     *   <8> Pricing signal mode High Voltage ride-through curves
     *   <9> HVRT Must Trip
     *   <10> HVRT Momentary Cessation Low Voltage ride-through curves
     *   <11> LVRT Must Trip
     *   <12> LVRT Momentary Cessation High Frequency ride-through curves
     *   <13> HFRT Must Trip
     *   <14> HFRT Momentary Cessation Low Frequency ride-through curves
     *   <15> LFRT Must Trip
     *   <16> LFRT Mandatory Operation
     * 
     * AO247
     *   <0> Curve disabled
     *   <1> Not applicable / Unknown
     *   <4> Time
     *   <29> Voltage
     *   <33> Frequency
     *   <38> Watts
     *   <23> Celsius Temperature
     *   <100> Price in hundredths of
     *   local currency
     *   <129> Percent Voltage
     *   <133> Percent Frequency
     *   <138> Percent Watts
     *   <233> Frequency Deviation
     *   <234+> Other
     *   
     * AO248
     *   <0> Curve disabled
     *   <1> Not applicable / unknown
     *   <2> VArs as percent of max VArs (VARMax)
     *   <3> VArs as percent of max available VArs (VArAval)
     *   <4> Vars as percent of max Watts (Wmax) – not used
     *   <5> Watts as percent of max Watts (Wmax)
     *   <6> Watts as percent of frozen active power (DeptSnptRef)
     *   <7> Power Factor in EEI notation
     *   <8> Volts as a percent of the nominal voltage (VRef)
     *   <9> Frequency as a percent of the nominal grid frequency (ECPNomHz)
     **/
    public class Curve : IMeasurementLoader, IDatabase
    {
        private IDictionary<ushort, Analog> m_analogInputMeasurements = new SortedDictionary<ushort, Analog>();
        private IDictionary<ushort, AnalogOutputStatus> m_analogOutputMeasurements = new SortedDictionary<ushort, AnalogOutputStatus>();

        public Curve()
        {
            byte quality = 0x01;
            DateTime dateTime = DateTime.Now;

            //
            // set default values for analog inputs
            //
            m_analogInputMeasurements[330] = new Analog(0, quality, dateTime);

            //
            // set default values for analog outputs
            //
            m_analogOutputMeasurements[245] = new AnalogOutputStatus(0, quality, dateTime);
            m_analogOutputMeasurements[246] = new AnalogOutputStatus(0, quality, dateTime);
            m_analogOutputMeasurements[247] = new AnalogOutputStatus(0, quality, dateTime);
            m_analogOutputMeasurements[248] = new AnalogOutputStatus(0, quality, dateTime);

            for (ushort index = 249; index <= 447; index++)
            {
                m_analogOutputMeasurements[index] = new AnalogOutputStatus(0, quality, dateTime);
            }
        }

        public bool Enable { get; set; }

        public ChangeSet CreateChangeSet()
        {
            ChangeSet result = new ChangeSet();

            foreach (var data in m_analogInputMeasurements)
            {
                result.Update(data.Value, data.Key);
            }

            foreach (var data in m_analogOutputMeasurements)
            {
                result.Update(data.Value, data.Key);
            }

            return result;
        }

        void IMeasurementLoader.Load(IChangeSet updates)
        {
            updates.Apply(this);
        }

        void IDatabase.Update(TimeAndInterval update, ushort index)
        {
        }

        void IDatabase.Update(Counter update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(BinaryOutputStatus update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(AnalogOutputStatus update, ushort index, EventMode mode)
        {
            if (m_analogOutputMeasurements.ContainsKey(index))
            {
                if (!Enable)
                {
                    throw new CurveException(CommandStatus.BLOCKED, "Curve editing is disabled, failed to write AO" + index.ToString() + " with value " + update.Value.ToString());
                }

                m_analogOutputMeasurements[index] = update;
            }
        }

        void IDatabase.Update(FrozenCounter update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(Analog update, ushort index, EventMode mode)
        {
            if (m_analogInputMeasurements.ContainsKey(index))
            {
                if (!Enable)
                {
                    throw new CurveException(CommandStatus.BLOCKED, "Curve editing is disabled, failed to write AI" + index.ToString() + " with value " + update.Value.ToString());
                }

                m_analogInputMeasurements[index] = update;
            }
        }

        void IDatabase.Update(DoubleBitBinary update, ushort index, EventMode mode)
        {
        }

        void IDatabase.Update(Binary update, ushort index, EventMode mode)
        {
        }
    }
}
