using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Automatak.DNP3.Interface;
using Automatak.Simulator.DNP3.Commons;
using Automatak.Simulator.DNP3.Commons.Configuration;

namespace Automatak.Simulator.DNP3.DEROutstationPlugin
{
    partial class OutstationForm : Form, ICommandHandler
    {
        MeasurementCollection activeCollection = null;

        readonly IOutstation outstation;
        readonly EventedOutstationApplication application;
        readonly MeasurementCache cache;
        readonly ProxyCommandHandler proxy;
        readonly IMeasurementLoader loader;

        readonly ChangeSet events = new ChangeSet();

        readonly Configuration m_configuration;

        public OutstationForm(IOutstation outstation, EventedOutstationApplication application, MeasurementCache cache, ProxyCommandHandler proxy, String alias)
        {
            InitializeComponent();

            this.outstation = outstation;
            this.application = application;
            this.cache = cache;
            this.proxy = proxy;

            this.loader = new ProxyLoader(outstation, cache);

            this.Text = String.Format("DNP3 Outstation ({0})", alias);
            this.comboBoxTypes.DataSource = System.Enum.GetValues(typeof(MeasType));

            this.comboBoxColdRestartMode.DataSource = System.Enum.GetValues(typeof(RestartMode));

            this.application.ColdRestart += application_ColdRestart;
            this.application.TimeWrite += application_TimeWrite;

            this.CheckState();

            // tell ProxyCommandHandler to use the proxy
            this.proxy.Enabled = false;

            // and use this form as the proxy
            proxy.CommandProxy = this;

            m_configuration = Configuration.LoadConfiguration();
        }

        void application_TimeWrite(ulong millisecSinceEpoch)
        {
            this.application.NeedTime = false;
            this.Invoke(new Action(() => this.checkBoxNeedTime.Checked = false));
        }

        void application_ColdRestart()
        {
            // simulate a restart with the restart IIN bit
            this.outstation.SetRestartIIN();
        }
        
        void CheckState()
        {
            if (((MeasType)comboBoxTypes.SelectedValue) != MeasType.OctetString && this.measurementView.SelectedIndices.Any())
            {
                this.buttonEdit.Enabled = true;
            }
            else
            {
                this.buttonEdit.Enabled = false;
            }

            if (events.Count > 0)
            {
                this.buttonApply.Enabled = true;
                this.buttonClear.Enabled = true;
            }
            else
            {
                this.buttonApply.Enabled = false;
                this.buttonClear.Enabled = false;
            }
        }
     
        void GUIMasterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        void comboBoxTypes_SelectedIndexChanged(object sender, EventArgs e)
        {        
            var index = this.comboBoxTypes.SelectedIndex;
            if(Enum.IsDefined(typeof(MeasType), index))
            {
                MeasType type = (MeasType) Enum.ToObject(typeof(MeasType), index);             
                var collection = cache.GetCollection(type);
                if (collection != null)
                {
                    if (activeCollection != null)
                    {
                        activeCollection.RemoveObserver(this.measurementView);
                    }

                    activeCollection = collection;

                    collection.AddObserver(this.measurementView);
                } 
            }
            this.CheckState(); 
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            var indices = this.measurementView.SelectedIndices;

            switch ((MeasType) comboBoxTypes.SelectedValue)
            { 
                case(MeasType.Binary):
                    LoadBinaries(indices, true);
                    break;
                case (MeasType.BinaryOutputStatus):
                    LoadBinaries(indices, false);
                    break;
                case (MeasType.Counter):
                    LoadCounters(indices, true);
                    break;
                case (MeasType.FrozenCounter):
                    LoadCounters(indices, false);
                    break;
                case (MeasType.Analog):
                    LoadAnalogs(indices, true);
                    break;
                case (MeasType.AnalogOutputStatus):
                    LoadAnalogs(indices, false);
                    break;
                case(MeasType.DoubleBitBinary):
                    LoadDoubleBinaries(indices);
                    break;
                
            }
        }

        void LoadBinaries(IEnumerable<ushort> indices, bool isBinary)
        {
            using (var dialog = new BinaryValueDialog(isBinary, indices))
            {                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.AddChanges(dialog.SelectedChanges);
                }
            }
        }

        void LoadDoubleBinaries(IEnumerable<ushort> indices)
        {
            using (var dialog = new DoubleBinaryValueDialog(indices))
            {                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.AddChanges(dialog.SelectedChanges);
                }
            }
        }

        void LoadAnalogs(IEnumerable<ushort> indices, bool isAnalog)
        {
            using (var dialog = new AnalogValueDialog(isAnalog, indices))
            {                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.AddChanges(dialog.SelectedChanges);
                }
            }
        }

        void LoadCounters(IEnumerable<ushort> indices, bool isCounter)
        {
            using (var dialog = new CounterValueDialog(isCounter, indices))
            {                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.AddChanges(dialog.SelectedChanges);
                }
            }
        }

        void AddChanges(IChangeSet changes)
        {
            ListviewDatabaseAdapter.Process(changes, listBoxEvents);

            // merge these changes onto the main changeset
            changes.Apply(events);
            
            this.CheckState();
        }

        void LoadSingleBinaryOutputStatus(ControlRelayOutputBlock command, ushort index, bool value)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => LoadSingleBinaryOutputStatus(command, index, value)));
            }
            else
            {
                var changes = new ChangeSet();

                DateTime dateTime = DateTime.Now;
                byte quality = 0x01;

                string logText = String.Format("Accepted CROB: {0} - {1}", command.code, index);

                //
                // set the output point in the change set
                //
                changes.Update(new BinaryOutputStatus(value, quality, dateTime), index);

                //
                // set the mapped input point in the change set
                //
                if (m_configuration.binaryIndexOutputToInput.ContainsKey(index))
                {
                    ushort inputIndex = m_configuration.binaryIndexOutputToInput[index];

                    logText += String.Format(", writing CROB: {0}", inputIndex);
                    changes.Update(new Binary(value, quality, dateTime), inputIndex);
                }

                this.listBoxLog.Items.Add(logText);
                loader.Load(changes);
            }
        }

        void LoadSingleAnalogOutputStatus(ushort index, double value)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => LoadSingleAnalogOutputStatus(index, value)));
            }
            else
            {
                var changes = new ChangeSet();

                byte quality = 0x01;

                string logText = String.Format("Accepted AOB: {0} - {1}", value, index);

                //
                // set the output point in the change set
                //
                this.listBoxLog.Items.Add(logText);
                changes.Update(new AnalogOutputStatus(value, quality, DateTime.Now), index);

                //
                // set the mapped input point in the change set
                //
                if (m_configuration.analogIndexOutputToInput.ContainsKey(index))
                {
                    ushort inputIndex = m_configuration.analogIndexOutputToInput[index];

                    changes.Update(new Analog(value, quality), inputIndex);

                    logText += String.Format(", writing AI: {0}", inputIndex);
                }

                this.listBoxLog.Items.Add(logText);

                loader.Load(changes);
            }
        }

        /*
         * Handles logic to set CommandStatus for both Select and Operate
         */
        CommandStatus OnControl(ControlRelayOutputBlock command, ushort index, bool operate)
        {
            if (!(index < m_configuration.binaryOutputs.Count))
            {
                return CommandStatus.OUT_OF_RANGE;
            }

            switch (command.code)
            {
                case (ControlCode.LATCH_ON):
                    if (operate) this.LoadSingleBinaryOutputStatus(command, index, true);
                    return CommandStatus.SUCCESS;

                case (ControlCode.LATCH_OFF):
                    if (operate) this.LoadSingleBinaryOutputStatus(command, index, false);
                    return CommandStatus.SUCCESS;

                default:
                    return CommandStatus.NOT_SUPPORTED;
            }
        }

        CommandStatus OnAnalogControl(double value, ushort index, bool operate)
        {
            if (!(index < m_configuration.analogOutputs.Count))
            {
                return CommandStatus.OUT_OF_RANGE;
            }

            if (operate)
            {
                LoadSingleAnalogOutputStatus(index, value);
            }

            return CommandStatus.SUCCESS;
        }

        private void measurementView_OnRowSelectionChanged(IEnumerable<UInt16> selection)
        {
            this.CheckState();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {           
           loader.Load(events);
           events.Clear();           
           
           this.listBoxEvents.Items.Clear();           
           this.CheckState();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.listBoxEvents.Items.Clear();
            this.events.Clear();
            this.CheckState();
        }

        private void checkBoxNeedTime_CheckedChanged(object sender, EventArgs e)
        {
            this.application.SupportsWriteTime = checkBoxNeedTime.Checked;
            this.application.NeedTime = checkBoxNeedTime.Checked;                        
        }

        private void checkBoxLocalMode_CheckedChanged(object sender, EventArgs e)
        {
            this.application.LocalMode = checkBoxLocalMode.Checked;
        }

        private void comboBoxColdRestartMode_SelectedValueChanged(object sender, EventArgs e)
        {
            this.application.ColdRestartMode = (RestartMode) comboBoxColdRestartMode.SelectedValue;
        }

        private void numericUpDownColdRestartTime_ValueChanged(object sender, EventArgs e)
        {
            this.application.ColdRestartTime = Decimal.ToUInt16(numericUpDownColdRestartTime.Value);
        }

        void ICommandHandler.Start()
        {
            
        }

        void ICommandHandler.End()
        {
            
        }

        CommandStatus ICommandHandler.Select(ControlRelayOutputBlock command, ushort index)
        {
            return OnControl(command, index, false);
        }

        CommandStatus ICommandHandler.Select(AnalogOutputInt32 command, ushort index)
        {
            return OnAnalogControl(command.value, index, false);
        }

        CommandStatus ICommandHandler.Select(AnalogOutputInt16 command, ushort index)
        {
            return OnAnalogControl(command.value, index, false);
        }

        CommandStatus ICommandHandler.Select(AnalogOutputFloat32 command, ushort index)
        {
            return OnAnalogControl(command.value, index, false);
        }

        CommandStatus ICommandHandler.Select(AnalogOutputDouble64 command, ushort index)
        {
            return OnAnalogControl(command.value, index, false);
        }

        CommandStatus ICommandHandler.Operate(ControlRelayOutputBlock command, ushort index, OperateType opType)
        {
            return OnControl(command, index, true);
        }

        CommandStatus ICommandHandler.Operate(AnalogOutputInt32 command, ushort index, OperateType opType)
        {
            return OnAnalogControl(command.value, index, true);
        }

        CommandStatus ICommandHandler.Operate(AnalogOutputInt16 command, ushort index, OperateType opType)
        {
            return OnAnalogControl(command.value, index, true);
        }

        CommandStatus ICommandHandler.Operate(AnalogOutputFloat32 command, ushort index, OperateType opType)
        {
            return OnAnalogControl(command.value, index, true);
        }

        CommandStatus ICommandHandler.Operate(AnalogOutputDouble64 command, ushort index, OperateType opType)
        {
            return OnAnalogControl(command.value, index, true);
        }
    }
}
