using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.NodeProperties {
    public class PropertyCheckbox : Property {
        private CheckBox checkbox;

        public PropertyCheckbox(string label) : base(false, false, Type.CHECKBOX) {
            sLabel = label;
            updateLayout();
        }

        public override bool bValue
        {
            get { return checkbox.Checked; }
            set { checkbox.Checked = value; }
        }

        private void updateLayout() {
            SuspendLayout();

            resetLayout();

            checkbox = new CheckBox();
            checkbox.CheckedChanged += fireEvent;

            Controls.Add(checkbox, 0, 0);

            ResumeLayout();
        }
    }
}
