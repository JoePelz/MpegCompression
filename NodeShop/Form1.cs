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

namespace NodeShop {
    public partial class Form1 : Form {
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
    }
}
