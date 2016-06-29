using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NodeShop.NodeProperties {
    public enum Type {
        NONE,
        INTEGER,
        FLOAT,
        STRING,
        CHECKBOX,
        SELECTION,
        BUTTON,
        CUSTOM,
        //Place linking types below channels, display types above.
        CHANNELS,
        COLOR,
        VECTORS
    };

    public abstract class Property : TableLayoutPanel {
        protected Type type;
        protected string sLabel;
        protected Label lblName;
        public bool isWriteable = true;
        public bool isInput { get; private set; }
        public bool isOutput { get; private set; }

        public event EventHandler eValueChanged;
        
        private string[] choices;
        public Nodes.Node.Address input;
        public HashSet<Nodes.Node.Address> output;


        public Property(bool input, bool output, Type type) {
            ColumnCount = 2;
            RowCount = 1;
            Dock = DockStyle.Fill;
            isInput = input;
            isOutput = output;
            this.type = type;
            if (output) {
                this.output = new HashSet<Nodes.Node.Address>();
            }
        }

        public Type getType() {
            return type;
        }

        protected void resetLayout() {
            Controls.Clear();
            lblName = new Label();
            lblName.Text = sLabel;
            lblName.Name = "lblName";
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            Controls.Add(lblName, 1, 0);
        }
        
        protected void fireEvent(object sender, EventArgs e) {
            EventHandler handler = eValueChanged;
            if (handler != null) {
                handler(this, e);
            }
        }
        
        public virtual string sValue
        {
            get { return ""; }
            set { }
        }
        public virtual float fValue
        {
            get { return 0.0f; }
            set { }
        }
        public virtual int nValue
        {
            get { return 0; }
            set { }
        }
        public virtual bool bValue
        {
            get { return false; }
            set { }
        }

        public abstract void FromString(string data);
    }
}
