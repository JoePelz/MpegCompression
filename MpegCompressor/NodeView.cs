using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public partial class NodeView : FlowLayoutPanel {
        private Node selectedNode;
        public event EventHandler eSelectionChanged;


        public NodeView() {
            InitializeComponent();
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        public NodeView(IContainer container) {
            container.Add(this);

            InitializeComponent();
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        public void clearNodes() {
            SuspendLayout();
            Controls.Clear();
            ResumeLayout();
        }

        public void addNode(Node n) {
            SuspendLayout();
            Controls.Add(n);
            n.MouseDown += onNodeClicked;
            ResumeLayout();
        }

        private void select(Node sel) {
            selectedNode = sel;
            foreach (Node n in Controls) {
                if (n == sel) {
                    n.setSelected(true);
                } else {
                    n.setSelected(false);
                }
            }
            EventHandler handler = eSelectionChanged;
            if (handler != null) {
                handler(this, new EventArgs());
            }
        }

        public Node getSelection() {
            return selectedNode;
        }

        private void onNodeClicked(object sender, MouseEventArgs e) {
            select(sender as Node);
        }

        protected override void OnMouseEnter(EventArgs e) {
            this.Focus();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            select(null);
        }
    }
}
