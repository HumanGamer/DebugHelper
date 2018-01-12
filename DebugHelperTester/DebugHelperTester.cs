using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DebugHelperTester
{
    public partial class Form1 : Form
    {
        private readonly TestObject _testObject;

        public Form1()
        {
            InitializeComponent();

            _testObject = new TestObject();

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            dbgMain.SelectedObject = _testObject;
        }
    }
}
