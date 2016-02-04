using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
    public enum PROP_TYPE{
        INTEGER,
        FLOAT,
        STRING,
        CHECKBOX,
        SELECTION
    };
    public class Property : TableLayoutPanel {
        private Label lblName;
        private NumericUpDown nUpDown;
        private TextBox sTextBox;
        private ComboBox comboBox;
        private int nVal;
        private int nMin;
        private int nMax;
        private float fVal;
        private float fMin;
        private float fMax;
        private string sVal;
        private PROP_TYPE type;
        private string sLabel;
        private string[] choices;

        public event EventHandler eValueChanged;

        public Property() {
            ColumnCount = 2;
            RowCount = 1;
            Dock = DockStyle.Fill;
        }

        public void createInt(int val, int min, int max, string label) {
            sLabel = label;
            nVal = val;
            nMin = min;
            nMax = max;
            type = PROP_TYPE.INTEGER;
            updateLayout();
        }

        public void createFloat(float val, float min, float max, string label) {
            sLabel = label;
            fVal = val;
            fMin = min;
            fMax = max;
            type = PROP_TYPE.FLOAT;
            updateLayout();
        }

        public void createString(string value, string label) {
            sVal = value;
            sLabel = label;
            type = PROP_TYPE.STRING;
            updateLayout();
        }

        public void createChoices(string[] choices, int defChoice, string label) {
            nVal = defChoice;
            this.choices = choices;
            sLabel = label;
            type = PROP_TYPE.SELECTION;
            updateLayout();
        }

        public void setFloat(float f) {
            nUpDown.Value = (decimal)f;
        }
        public void setInt(int n) {
            nUpDown.Value = n;
        }
        public void setString(string s) {
            sTextBox.Text = s;
        }
        public void setSelection(int sel) {
            nVal = sel;
            comboBox.SelectedIndex = sel;
        }

        public float getFloat() {
            return fVal;
        }
        public int getInt() {
            return nVal;
        }
        public string getString() {
            return sVal;
        }
        public int getSelection() {
            return nVal;
        }

        void resetLayout() {
            Controls.Clear();
            lblName = null;
            nUpDown = null;
        }

        private void updateLayout() {
            SuspendLayout();

            resetLayout();

            switch(type) {
                case PROP_TYPE.INTEGER:
                    layoutInt();
                    break;
                case PROP_TYPE.FLOAT:
                    layoutFloat();
                    break;
                case PROP_TYPE.STRING:
                    layoutString();
                    break;
                case PROP_TYPE.SELECTION:
                    layoutSelection();
                    break;
                default:
                    break;
            }

            ResumeLayout(false);
            PerformLayout();
            if (nUpDown != null)
                ((System.ComponentModel.ISupportInitialize)(nUpDown)).EndInit();
        }

        private void layoutInt() {
            lblName = new Label();
            lblName.Text = sLabel;
            lblName.Name = "lblName";
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

            nUpDown = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDown)).BeginInit();
            nUpDown.Size = new System.Drawing.Size(120, 20);
            nUpDown.Name = "nUpDown";
            nUpDown.Value = nVal;
            nUpDown.ValueChanged += NUpDown_ValueChanged;
            nUpDown.Maximum = nMax;
            nUpDown.Minimum = nMin;

            Controls.Add(nUpDown, 0, 0);
            Controls.Add(lblName, 1, 0);
        }

        private void layoutFloat() {
            lblName = new Label();
            lblName.Text = sLabel;
            lblName.Name = "lblName";
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

            nUpDown = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDown)).BeginInit();
            nUpDown.Size = new System.Drawing.Size(120, 20);
            nUpDown.Name = "fUpDown";
            nUpDown.Value = (decimal)fVal;
            nUpDown.ValueChanged += FUpDown_ValueChanged;
            nUpDown.Maximum = (decimal)fMax;
            nUpDown.Minimum = (decimal)fMin;

            Controls.Add(nUpDown, 0, 0);
            Controls.Add(lblName, 1, 0);
        }

        private void layoutString() {
            lblName = new Label();
            lblName.Text = sLabel;
            lblName.Name = "lblName";
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

            sTextBox = new TextBox();
            sTextBox.Size = new System.Drawing.Size(120, 20);
            sTextBox.Name = "sTextBox";
            sTextBox.Text = sVal;
            sTextBox.TextChanged += STextBox_TextChanged;

            Controls.Add(sTextBox, 0, 0);
            Controls.Add(lblName, 1, 0);
        }

        private void layoutSelection() {
            lblName = new Label();
            lblName.Text = sLabel;
            lblName.Name = "lblName";
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

            comboBox = new ComboBox();
            foreach (string option in choices) {
                comboBox.Items.Add(option);
            }
            comboBox.SelectedIndex = nVal;
            comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;

            Controls.Add(comboBox, 0, 0);
            Controls.Add(lblName, 1, 0);
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            nVal = comboBox.SelectedIndex;
            fireEvent(e);
        }

        private void STextBox_TextChanged(object sender, EventArgs e) {
            sVal = sTextBox.Text;
            fireEvent(e);
        }

        private void NUpDown_ValueChanged(object sender, EventArgs e) {
            nVal = (int)nUpDown.Value;
            nUpDown.Value = nVal;
            fireEvent(e);
        }

        private void FUpDown_ValueChanged(object sender, EventArgs e) {
            fVal = (float)nUpDown.Value;
            nUpDown.Value = (decimal)fVal;
            fireEvent(e);
        }

        private void fireEvent(EventArgs e) {
            EventHandler handler = eValueChanged;
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
