using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Automatak.Simulator.DNP3.DEROutstationPlugin
{
    public partial class OutstationForm : Form
    {
        public OutstationForm()
        {
            InitializeComponent();
        }

        private void OutstationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
