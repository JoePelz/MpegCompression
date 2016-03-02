using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.NodeProperties {
    public class PropertyInt : Property {
        private int nMin;
        private int nMax;
        private NumericUpDown nUpDown;

        public override int nValue
        {
            get { return (int)nUpDown.Value; }
            set
            {
                int newVal = value;
                if (newVal < nMin) newVal = nMin;
                else if (newVal > nMax) newVal = nMax;
                nUpDown.Value = newVal;
            }
        }

        public PropertyInt(int val, int min, int max, string label) : base(false, false, Type.INTEGER) {
            sLabel = label;
            nMin = min;
            nMax = max;
            updateLayout(min, max);
            nValue = val;
        }

        private void updateLayout(int min, int max) {
            SuspendLayout();

            resetLayout();

            nUpDown = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDown)).BeginInit();
            nUpDown.Size = new System.Drawing.Size(120, 20);
            nUpDown.Name = "nUpDown";
            nUpDown.Value = min;
            nUpDown.ValueChanged += fireEvent;
            nUpDown.Maximum = max;
            nUpDown.Minimum = min;

            Controls.Add(nUpDown, 0, 0);

            ResumeLayout();

            if (nUpDown != null)
                ((System.ComponentModel.ISupportInitialize)(nUpDown)).EndInit();
        }
    }
}
