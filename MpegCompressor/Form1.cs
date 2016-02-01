using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
    public partial class Form1 : Form {
        Controller controller;
        public Form1() {
            InitializeComponent();
            controller = new Controller(viewProperties, viewNodes, viewLeft, viewRight);
            
        }
    }
}
