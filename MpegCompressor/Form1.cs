using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MpegCompressor {
    public partial class Form1 : Form {
        //public const int WM_KEYDOWN = 0x0100;
        //public const int WM_KEYUP = 0x0101;
        //public const int WM_CHAR = 0x0102;
        private Controller controller;

        public Form1() {
            InitializeComponent();
            controller = new Controller(viewProperties, viewNodes, viewLeft, viewRight);
        }

        protected override bool ProcessCmdKey(ref Message message, Keys keys) {
            if (controller.HotKeys(keys)) {
                return true;
            }

            // run base implementation
            return base.ProcessCmdKey(ref message, keys);
        }

        /*
        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_KEYDOWN) {
                return;
            }
            base.WndProc(ref m);
        }
        */
    }
}
