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
        }

        public PropertyPage(IContainer container) {
            container.Add(this);

            InitializeComponent();
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

        public void addProperty(Property p) {
            SuspendLayout();
            RowCount++;
            RowStyles.Clear();
            for (int i=0; i < RowCount; i++) {
                RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.0f / RowCount));
            }
            Controls.Add(p, 0, RowCount - 1);
            ResumeLayout(false);
            PerformLayout();
        }

        public void addProperties(Dictionary<string, Property> props) {
            foreach (Property p in props.Values) {
                addProperty(p);
            }
        }
    }
}
