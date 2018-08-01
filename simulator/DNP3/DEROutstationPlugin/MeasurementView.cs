﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using Automatak.Simulator.DNP3.Commons;
using Automatak.Simulator.DNP3.Commons.Configuration;

namespace Automatak.Simulator.DNP3.DEROutstationPlugin
{
    public partial class MeasurementView : UserControl, IMeasurementObserver
    {
        MeasurementCollection collection = new MeasurementCollection();
        SortedDictionary<ushort, int> indexToRow = new SortedDictionary<ushort, int>();

        public delegate void RowSelectionEvent(IEnumerable<UInt16> rows);

        public event RowSelectionEvent OnRowSelectionChanged;

        bool allowSelection = false;

        readonly Configuration m_configuration;

        public MeasurementView()
        {
            m_configuration = Configuration.LoadConfiguration();

            InitializeComponent();
        }

        public bool AllowSelection
        {
            set
            {
                allowSelection = value;
            }
            get
            {
                return allowSelection;
            }
        }

        ListViewItem CreateItem(Measurement m)
        {
            string name = "---";
            string mappedIndex = "---";

            if (m.Type == MeasType.Analog)
            {
                name = m_configuration.analogInputsMap[m.Index].name;

                if (m_configuration.analogIndexInputToOutput.ContainsKey(m.Index))
                {
                    mappedIndex = m_configuration.analogIndexInputToOutput[m.Index].ToString();
                }
            }
            else if (m.Type == MeasType.AnalogOutputStatus)
            {
                name = m_configuration.analogOutputsMap[m.Index].name;

                if (m_configuration.analogIndexOutputToInput.ContainsKey(m.Index))
                {
                    mappedIndex = m_configuration.analogIndexOutputToInput[m.Index].ToString();
                }
            }
            else if (m.Type == MeasType.Binary)
            {
                name = m_configuration.binaryInputsMap[m.Index].name;

                if (m_configuration.binaryIndexInputToOutput.ContainsKey(m.Index))
                {
                    mappedIndex = m_configuration.binaryIndexInputToOutput[m.Index].ToString();
                }
            }
            else if (m.Type == MeasType.BinaryOutputStatus)
            {
                name = m_configuration.binaryOutputsMap[m.Index].name;

                if (m_configuration.binaryIndexOutputToInput.ContainsKey(m.Index))
                {
                    mappedIndex = m_configuration.binaryIndexOutputToInput[m.Index].ToString();
                }
            }

            string[] text = { m.Index.ToString(), name, m.Value, mappedIndex, m.Flags, m.Timestamp };

            var item = new ListViewItem(text);
            return item;
        }

        void RefreshAllRows(IEnumerable<Measurement> rows)
        {
            try
            {
                this.listView.BeginUpdate();
                this.listView.Items.Clear();
                this.indexToRow.Clear();
                int ri = 0;
                foreach (var m in rows)
                {
                    this.listView.Items.Add(CreateItem(m));
                    indexToRow[m.Index] = ri;
                    ++ri;
                }
            }
            finally
            {
                this.listView.EndUpdate();
            }
        }

        void InsertOrUpdate(Measurement meas)
        {
            if (indexToRow.ContainsKey(meas.Index))
            {
                var row = indexToRow[meas.Index];
                this.listView.Items[row] = CreateItem(meas);
            }
            else
            { 
                // figure out where to insert
                var entry = indexToRow.FirstOrDefault(kvp => kvp.Key > meas.Index);
                if (entry.Equals(default(KeyValuePair<ushort, int>)))
                {
                    var row = listView.Items.Count;
                    listView.Items.Add(CreateItem(meas));
                    indexToRow[meas.Index] = row;
                }
                else
                {
                    listView.Items.Insert(entry.Value, CreateItem(meas));
                    indexToRow[meas.Index] = entry.Value;
                    var rows = indexToRow.Select(kvp => kvp.Key > meas.Index);
                    foreach (var kvp in indexToRow)
                    {
                        if (kvp.Key > meas.Index)
                        {
                            indexToRow[kvp.Key] = kvp.Value + 1;
                        }
                    }
                }
            }
        }

        void IMeasurementObserver.Refresh(IEnumerable<Measurement> rows)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => RefreshAllRows(rows)));
            }
            else
            {
                RefreshAllRows(rows);
            }
        }

        void IMeasurementObserver.Update(Measurement meas)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => InsertOrUpdate(meas)));
            }
            else
            {
                this.InsertOrUpdate(meas);
            }
        }

        private void listView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!allowSelection && e.IsSelected)
            {
                e.Item.Selected = false;
            }
        }

        public IEnumerable<UInt16> SelectedIndices
        {
            get
            {
                foreach (int i in listView.SelectedIndices)
                {
                    yield return (ushort) i;
                }
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.OnRowSelectionChanged != null)
            {
                OnRowSelectionChanged(SelectedIndices.ToArray());
            }
        }
    }
}
