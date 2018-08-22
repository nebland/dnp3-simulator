using System;
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

        public MeasurementView()
        {
            InitializeComponent();
        }

        /*
         * Could not load Configuration in constructor because relative path to
         * file would be incorrect in VS Designer, so would break loading of Forms
         * that contain this class. Over loading OnLoad and implementing Load event
         * did not work, neither functions were called for some reason.
         * 
         * Forms using this class need to call this function, this class assumes this
         * object exists.
         */
        public Configuration Configuration { get; set; }

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
                name = this.Configuration.analogInputsMap[m.Index].name;

                if (this.Configuration.analogIndexInputToOutput.ContainsKey(m.Index))
                {
                    mappedIndex = this.Configuration.analogIndexInputToOutput[m.Index].ToString();
                }
            }
            else if (m.Type == MeasType.AnalogOutputStatus)
            {
                name = this.Configuration.analogOutputsMap[m.Index].name;

                if (this.Configuration.analogIndexOutputToInput.ContainsKey(m.Index))
                {
                    mappedIndex = this.Configuration.analogIndexOutputToInput[m.Index].ToString();
                }
            }
            else if (m.Type == MeasType.Binary)
            {
                name = this.Configuration.binaryInputsMap[m.Index].name;

                if (this.Configuration.binaryIndexInputToOutput.ContainsKey(m.Index))
                {
                    mappedIndex = this.Configuration.binaryIndexInputToOutput[m.Index].ToString();
                }
            }
            else if (m.Type == MeasType.BinaryOutputStatus)
            {
                name = this.Configuration.binaryOutputsMap[m.Index].name;

                if (this.Configuration.binaryIndexOutputToInput.ContainsKey(m.Index))
                {
                    mappedIndex = this.Configuration.binaryIndexOutputToInput[m.Index].ToString();
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
