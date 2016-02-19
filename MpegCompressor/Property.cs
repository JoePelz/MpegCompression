using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
    public class Property : TableLayoutPanel {
        public enum Type {
            NONE,
            INTEGER,
            FLOAT,
            STRING,
            CHECKBOX,
            SELECTION,
            BUTTON
        };

        private Label lblName;
        private NumericUpDown nUpDown;
        private TextBox sTextBox;
        private ComboBox comboBox;
        private CheckBox checkbox;
        private Button btn;
        private int nVal;
        private int nMin;
        private int nMax;
        private float fVal;
        private float fMin;
        private float fMax;
        private string sVal;
        private Type type;
        private string sLabel;
        private string[] choices;
        public bool isInput { get; private set; }
        public bool isOutput { get; private set; }
        public Node.Address input;
        public HashSet<Node.Address> output;

        public event EventHandler eValueChanged;

        public Property(bool input, bool output) {
            ColumnCount = 2;
            RowCount = 1;
            Dock = DockStyle.Fill;
            isInput = input;
            isOutput = output;
            type = Type.NONE;
            if (output) {
                this.output = new HashSet<Node.Address>();
            }

        }

        public Type getType() {
            return type;
        }


        public void createInt(int val, int min, int max, string label) {
            sLabel = label;
            nVal = val;
            nMin = min;
            nMax = max;
            type = Type.INTEGER;
            updateLayout();
        }

        public void createFloat(float val, float min, float max, string label) {
            sLabel = label;
            fVal = val;
            fMin = min;
            fMax = max;
            type = Type.FLOAT;
            updateLayout();
        }

        public void createString(string value, string label) {
            sVal = value;
            sLabel = label;
            type = Type.STRING;
            updateLayout();
        }

        public void createChoices(string[] choices, int defChoice, string label) {
            nVal = defChoice;
            this.choices = choices;
            sLabel = label;
            type = Type.SELECTION;
            updateLayout();
        }

        public void createCheckbox(string label) {
            sLabel = label;
            type = Type.CHECKBOX;
            updateLayout();
        }

        public void createButton(string text, string label) {
            sVal = text;
            sLabel = label;
            type = Type.BUTTON;
            updateLayout();
        }

        public void setFloat(float f) {
            nUpDown.Value = (decimal)f;
        }
        public void setInt(int n) {
            nUpDown.Value = n;
        }
        public void setString(string s) {
            if (sTextBox != null) {
                sTextBox.Text = s;
            }
            if (btn != null) {
                btn.Text = s;
            }
            sVal = s;
        }
        public void setSelection(int sel) {
            nVal = sel;
            comboBox.SelectedIndex = sel;
        }
        public void setChecked(bool b) {
            checkbox.Checked = b;
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
        public bool getChecked() {
            return checkbox.Checked;
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
                case Type.INTEGER:
                    layoutInt();
                    break;
                case Type.FLOAT:
                    layoutFloat();
                    break;
                case Type.STRING:
                    layoutString();
                    break;
                case Type.SELECTION:
                    layoutSelection();
                    break;
                case Type.CHECKBOX:
                    layoutCheckbox();
                    break;
                case Type.BUTTON:
                    layoutButton();
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

        private void layoutCheckbox() {
            lblName = new Label();
            lblName.Text = sLabel;
            lblName.Name = "lblName";
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

            checkbox = new CheckBox();
            checkbox.CheckedChanged += Checkbox_CheckedChanged;

            Controls.Add(checkbox, 0, 0);
            Controls.Add(lblName, 1, 0);
        }

        private void layoutButton() {
            lblName = new Label();
            lblName.Text = sLabel;
            lblName.Name = "lblName";
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;

            btn = new Button();
            btn.Click += Btn_Click;
            btn.Text = sVal;

            Controls.Add(btn, 0, 0);
            Controls.Add(lblName, 1, 0);
        }

        private void Btn_Click(object sender, EventArgs e) {
            fireEvent(e);
        }

        private void Checkbox_CheckedChanged(object sender, EventArgs e) {
            fireEvent(e);
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
