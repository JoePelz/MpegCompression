using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.NodeProperties {
    public class PropertyFloat : Property {
        private float fMin;
        private float fMax;
        private NumericUpDown nUpDown;

        public override float fValue
        {
            get { return (float)nUpDown.Value; }
            set
            {
                float newVal = value;
                if (newVal < fMin) newVal = fMin;
                else if (newVal > fMax) newVal = fMax;
                nUpDown.Value = (decimal)newVal;
            }
        }
        
        public override string ToString() {
            return ((float)nUpDown.Value).ToString();
        }

        public override void FromString(string data) {
            fValue = float.Parse(data);
        }

        public PropertyFloat(float val, float min, float max, string label) : base(false, false, Type.FLOAT) {
            sLabel = label;
            updateLayout(min, max);
            fMin = min;
            fMax = max;
            fValue = val;
        }

        private void updateLayout(float min, float max) {
            SuspendLayout();

            resetLayout();

            nUpDown = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDown)).BeginInit();
            nUpDown.Size = new System.Drawing.Size(120, 20);
            nUpDown.Increment = (decimal)0.001;
            nUpDown.DecimalPlaces = 3;
            nUpDown.Name = "fUpDown";
            nUpDown.Value = (decimal)min;
            nUpDown.ValueChanged += fireEvent;
            nUpDown.Maximum = (decimal)max;
            nUpDown.Minimum = (decimal)min;

            Controls.Add(nUpDown, 0, 0);

            ResumeLayout();

            if (nUpDown != null)
                ((System.ComponentModel.ISupportInitialize)(nUpDown)).EndInit();
        }
    }
}
