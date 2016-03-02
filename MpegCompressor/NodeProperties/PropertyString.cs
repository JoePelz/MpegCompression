using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.NodeProperties {
    public class PropertyString : Property {
        private TextBox sTextBox;
        public override string sValue
        {
            get { return sTextBox.Text; }
            set { sTextBox.Text = value; }
        }

        public PropertyString(string value, string label) : base(false, false, Type.STRING) {
            sLabel = label;
            updateLayout();
            sValue = value;
        }

        private void updateLayout() {
            SuspendLayout();

            resetLayout();

            sTextBox = new TextBox();
            sTextBox.Size = new System.Drawing.Size(120, 20);
            sTextBox.Name = "sTextBox";
            sTextBox.TextChanged += fireEvent;

            Controls.Add(sTextBox, 0, 0);

            ResumeLayout();
        }
    }
}
