using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MpegCompressor {
    public partial class NodeView : TransformPanel {
        public event EventHandler eSelectionChanged;

        private Node selectedNode;
        private Pen linePen;
        private Font nodeFont;
        private LinkedList<Node> nodes;

        public NodeView() {
            InitializeComponent();
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            nodeFont = new Font("Tahoma", 11.0f);
            linePen = new Pen(Color.Black, 3);
            nodes = new LinkedList<Node>();
        }

        public NodeView(IContainer container) {
            container.Add(this);

            InitializeComponent();
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            nodeFont = new Font("Tahoma", 11.0f);
            linePen = new Pen(Color.Black, 3);
            nodes = new LinkedList<Node>();
        }

        public void clearNodes() {
            SuspendLayout();
            Controls.Clear();
            nodes.Clear();
            ResumeLayout();
        }

        public void addNode(Node n) {
            nodes.AddLast(n);
        }

        private void select(Node sel) {
            selectedNode = sel;
            
            EventHandler handler = eSelectionChanged;
            if (handler != null) {
                handler(this, new EventArgs());
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Rectangle r = new Rectangle();
            r.Width = 100;
            r.Height = 50;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (Node n in nodes) {
                foreach (Node.Address a in n.getInputs().Values) {
                    drawLink(g, a.node, n);
                }
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            foreach (Node n in nodes) {
                r.Location = n.pos;
                if (n == selectedNode) {
                    g.FillRectangle(Brushes.Wheat, r);
                } else {
                    g.FillRectangle(Brushes.CadetBlue, r);
                }
                g.DrawRectangle(Pens.Black, r);

                g.DrawString(n.getName(), nodeFont, Brushes.Black, r.Location);
                r.Offset(0, nodeFont.Height);
                g.DrawString(n.getExtra(), nodeFont, Brushes.Black, r.Location);
            }
        }

        private void drawLink(Graphics g, Node a, Node b) {
            Point p1 = a.pos;
            Point p2 = b.pos;
            p1.Offset(100, 25);
            p2.Offset(0, 25);
            g.DrawLine(linePen, p1, p2);
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
            if ((Control.ModifierKeys & Keys.Alt) != 0) {
                base.OnMouseDown(e);
            } else {
                select(hitTest(e.X, e.Y));
            }
        }

        private Node hitTest(int x, int y) {
            //x and y are in screen coordinates where 
            //  (0, 0) is the top left of the panel
            ScreenToCanvas(ref x, ref y);
            Point pos;

            foreach (Node n in nodes) {
                pos = n.pos;
                if (x > pos.X && y > pos.Y && x < (pos.X + 100) && y < (pos.Y + 50)) {
                    return n;
                }
            }
            return null;
        }
    }
}
