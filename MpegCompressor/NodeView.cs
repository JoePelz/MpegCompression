using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MpegCompressor.Nodes;

namespace MpegCompressor {
    public partial class NodeView : TransformPanel {
        public event EventHandler eSelectionChanged;

        private Node selectedNode;
        private LinkedList<Node> selectedNodes;
        private static Pen linePen = new Pen(Color.Black, 3);
        private static Pen linkChannels = new Pen(Color.Navy, 3);
        private static Pen linkColors = new Pen(Color.Orange, 3);
        private static Pen linkVectors = new Pen(Color.Violet, 3);
        private LinkedList<Node> nodes;
        private Point mdown;
        private bool bDragging;

        public NodeView() {
            InitializeComponent();
            init();
        }

        public NodeView(IContainer container) {
            container.Add(this);

            InitializeComponent();
            init();
        }

        private void init() {
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            nodes = new LinkedList<Node>();
            mdown = new Point();
            selectedNodes = new LinkedList<Node>();
        }

        public void clearNodes() {
            SuspendLayout();
            Controls.Clear();
            nodes.Clear();
            ResumeLayout();
        }

        public void addNode(Node n) {
            nodes.AddLast(n);
            recalcFocus();
        }

        private void recalcFocus() {
            int left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;
            foreach (Node d in nodes) {
                Rectangle r = d.getNodeRect();
                if (r.Left < left) left = r.Left;
                if (r.Right > right) right = r.Right;
                if (r.Top < top) top = r.Top;
                if (r.Bottom > bottom) bottom = r.Bottom;
            }
            setFocusRect(left, top, right - left + 100, bottom - top + 50);
        }

        private void select(Node sel, bool toggle) {
            if (toggle) {
                if (sel != null)
                    selectedNodes.AddLast(sel);
            } else {
                selectedNodes.Clear();
                if (sel != null)
                    selectedNodes.AddLast(sel);
            }
            if (selectedNodes.Count() == 1) {
                selectedNode = sel;
                EventHandler handler = eSelectionChanged;
                if (handler != null) {
                    handler(this, new EventArgs());
                }
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (Node n in nodes) {
                foreach (var kvp in n.getProperties()) {
                    //This may be redundant.
                    if (kvp.Value.isInput && kvp.Value.input != null) {
                        g.DrawLine(linePen, kvp.Value.input.node.getJointPos(kvp.Value.input.port, false), n.getJointPos(kvp.Key, true));
                    }
                }
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            foreach (Node n in nodes) {
                n.drawGraphNode(g, selectedNodes.Contains(n));
            }
        }

        public Node getSelection() {
            return selectedNode;
        }

        protected override void OnMouseEnter(EventArgs e) {
            this.Focus();
            base.OnMouseEnter(e);
        }
        
        protected override void OnMouseDown(MouseEventArgs e) {
            Node n;

            mdown = e.Location;

            //if the mouse is over a node, selected it and begin dragging. otherwise do base.
            //  if shift is selected, toggle selection instead of replacing
            if ((n = hitTest(e.X, e.Y)) != null) {
                if (!selectedNodes.Contains(n)) {
                    select(n, Control.ModifierKeys == Keys.Shift);
                }
                bDragging = true;
                ScreenToCanvas(ref mdown);
            } else {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (bDragging) {
                //mdown is in canvas coordinates
                Point newPos = e.Location;
                ScreenToCanvas(ref newPos);
                foreach (Node n in selectedNodes) {
                    n.offsetPos(newPos.X - mdown.X, newPos.Y - mdown.Y);
                }
                mdown = newPos;
                Invalidate();
            } else {
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (mdown.X == e.X && mdown.Y == e.Y) {
                select(hitTest(e.X, e.Y), Control.ModifierKeys == Keys.Shift);
            }

            if (bDragging) {
                bDragging = false;
                recalcFocus();
                return;
            }
            base.OnMouseUp(e);
        }

        private Node hitTest(int x, int y) {
            //x and y are in screen coordinates where 
            //  (0, 0) is the top left of the panel
            ScreenToCanvas(ref x, ref y);

            foreach (Node n in nodes) {
                if (n.hitTest(x, y)) {
                    return n;
                }
            }
            return null;
        }
    }
}
