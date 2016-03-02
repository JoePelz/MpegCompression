using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.NodeProperties {
    public class PropertyButton : Property {
        private Button btn;

        public PropertyButton(string text, string label) : base(false, false, Type.BUTTON) {
            sLabel = label;
            updateLayout();
            sValue = text;
        }

        public override string sValue
        {
            get { return btn.Text; }
            set { btn.Text = value; }
        }

        private void updateLayout() {
            SuspendLayout();

            resetLayout();
            
            btn = new Button();
            btn.Click += fireEvent;
            btn.Text = "Do the thing";

            Controls.Add(btn, 0, 0);

            ResumeLayout();
        }
    }
}
