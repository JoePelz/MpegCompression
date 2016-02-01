using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MpegCompressor {
    public partial class Viewport : Panel {
        public Viewport() {
            InitializeComponent();
        }

        public Viewport(IContainer container) {
            container.Add(this);

            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe) {

        }
    }
}
