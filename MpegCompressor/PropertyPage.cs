using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public partial class PropertyPage : TableLayoutPanel {
        public PropertyPage() {
            InitializeComponent();
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        public PropertyPage(IContainer container) {
            container.Add(this);

            InitializeComponent();
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;

        }

        protected override void OnMouseEnter(EventArgs e) {
            this.Focus();
            base.OnMouseEnter(e);
        }

        public void clearProperties() {
            SuspendLayout();
            Controls.Clear();
            ColumnCount = 1;
            RowCount = 0;
            ColumnStyles.Clear();
            ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            RowStyles.Clear();
            RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            ResumeLayout();
        }

        private void addProperty(Property p) {
            RowCount++;
            RowStyles.Clear();
            for (int i=0; i < RowCount; i++) {
                RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0f / RowCount));
            }
            Controls.Add(p, 0, RowCount - 1);
        }

        public void addProperties(Dictionary<string, Property> props) {
            SuspendLayout();
            foreach (Property p in props.Values) {
                addProperty(p);
            }
            ResumeLayout();
        }

        public void showProperties(IProperties hasProperties) {
            clearProperties();
            addProperties(hasProperties.getProperties());
        }
    }
}
