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
        private LinkedList<Node> nodes;
        private Point mdown;
        private Point mdrag;
        private bool bDragging;
        private bool bLinking;
        private Node.Address linkTo;
        private ContextMenu nodeMenu;

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
            initNodeMenu();
        }
        
        private void initNodeMenu() {
            MenuItem[] m = new MenuItem[4];
            m[0] = new MenuItem("IO");
            m[0].MenuItems.Add(new MenuItem("ReadImage", createNode));
            m[0].MenuItems.Add(new MenuItem("ReadChannels", createNode));
            m[0].MenuItems.Add(new MenuItem("ReadMulti2Channel", createNode));
            m[0].MenuItems.Add(new MenuItem("ReadMulti3Channel", createNode));
            m[0].MenuItems.Add(new MenuItem("WriteChannels", createNode));
            m[0].MenuItems.Add(new MenuItem("WriteMulti2Channel", createNode));
            m[0].MenuItems.Add(new MenuItem("WriteMulti3Channel", createNode));
            m[1] = new MenuItem("Color");
            m[1].MenuItems.Add(new MenuItem("ColorSpace", createNode));
            m[1].MenuItems.Add(new MenuItem("ColorToChannels", createNode));
            m[1].MenuItems.Add(new MenuItem("ChannelsToColor", createNode));
            m[1].MenuItems.Add(new MenuItem("Merge", createNode));
            m[1].MenuItems.Add(new MenuItem("Mix", createNode));
            m[2] = new MenuItem("Channels");
            m[2].MenuItems.Add(new MenuItem("ColorToChannels", createNode));
            m[2].MenuItems.Add(new MenuItem("ChannelsToColor", createNode));
            m[2].MenuItems.Add(new MenuItem("DCT", createNode));
            m[2].MenuItems.Add(new MenuItem("MoVecCompose", createNode));
            m[2].MenuItems.Add(new MenuItem("MoVecDecompose", createNode));
            m[2].MenuItems.Add(new MenuItem("Subsample", createNode));
            m[3] = new MenuItem("Other");
            m[3].MenuItems.Add(new MenuItem("NoOp", createNode));
            m[3].MenuItems.Add(new MenuItem("TestChunker", createNode));
            nodeMenu = new ContextMenu(m);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.Tab) {
                mdown = Cursor.Position;
                mdown = PointToClient(mdown);
                nodeMenu.Show(this, mdown);
                ScreenToCanvas(ref mdown);
            } else if (keyData == Keys.Delete) {
                deleteSelection();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void clearNodes() {
            SuspendLayout();
            Controls.Clear();
            nodes.Clear();
            ResumeLayout();
        }

        public void deleteSelection() {
            Node.Address other;
            foreach (Node n in selectedNodes) {
                foreach(var kvp in n.getProperties()) {
                    if (kvp.Value.isInput) {
                        other = kvp.Value.input;
                        if (other != null) {
                            Node.disconnect(other.node, other.port, n, kvp.Key);
                        }
                    } else if (kvp.Value.isOutput) {
                        var outputs = kvp.Value.output.ToArray();
                        foreach (var oc in outputs) {
                            Node.disconnect(n, kvp.Key, oc.node, oc.port);
                        }
                    }
                }
                nodes.Remove(n);
            }
            Invalidate();
        }

        public void addNode(Node n) {
            nodes.AddLast(n);
            recalcFocus();
        }

        public void createNode(object Sender, EventArgs e) {
            MenuItem m = (Sender as MenuItem);
            string name = "MpegCompressor.Nodes." + m.Text;
            Type type = Type.GetType(name);
            object instance = Activator.CreateInstance(type);
            if (instance == null) {
                return;
            }
            Node newNode = (Node)instance;
            newNode.setPos(mdown.X, mdown.Y);
            addNode(newNode);
            Invalidate();
        }

        private void recalcFocus() {
            int left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;
            foreach (Node n in nodes) {
                Rectangle r = NodeArtist.getRect(n);
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
                NodeArtist.drawLinks(g, n);
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            foreach (Node n in nodes) {
                NodeArtist.drawGraphNode(g, n, selectedNodes.Contains(n));
            }
            if (bLinking) {
                g.DrawLine(NodeArtist.linePen, mdown, mdrag);
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
            Node.Address a;

            mdown = e.Location;

            //if the mouse is over a node, selected it and begin dragging. otherwise do base.
            //  if shift is selected, toggle selection instead of replacing
            if ((a = hitTest(e.X, e.Y, true)).node != null) {
                if (a.port != null) {
                    bLinking = true;
                    ScreenToCanvas(ref mdown);
                    mdrag = mdown;
                    linkTo = a;
                } else {
                    if (!selectedNodes.Contains(a.node)) {
                        select(a.node, Control.ModifierKeys == Keys.Shift);
                    }
                    bDragging = true;
                    ScreenToCanvas(ref mdown);
                }
            } else {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (bDragging) {
                //mdown is in canvas coordinates
                mdrag = e.Location;
                ScreenToCanvas(ref mdrag);
                foreach (Node n in selectedNodes) {
                    n.offsetPos(mdrag.X - mdown.X, mdrag.Y - mdown.Y);
                }
                mdown = mdrag;
                Invalidate();
            } else if (bLinking) {
                mdrag = e.Location;
                ScreenToCanvas(ref mdrag);
                Invalidate();
            } else {
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            if (mdown.X == e.X && mdown.Y == e.Y) {
                select(hitTest(e.X, e.Y).node, Control.ModifierKeys == Keys.Shift);
            }

            if (bDragging) {
                bDragging = false;
                recalcFocus();
                return;
            }

            if (bLinking) {
                bLinking = false;
                Node.Address a = hitTest(e.X, e.Y, false);
                Node.Address old = linkTo.node.getProperties()[linkTo.port].input;
                if (old != null) {
                    Node.disconnect(old.node, old.port, linkTo.node, linkTo.port);
                }
                if (a.node != null && a.port != null) {
                    Node.connect(a.node, a.port, linkTo.node, linkTo.port);
                }

                Invalidate();
                return;
            }
            base.OnMouseUp(e);
        }

        private Node.Address hitTest(int x, int y) {
            return hitTest(x, y, true);
        }

        private Node.Address hitTest(int x, int y, bool input) {
            //x and y are in screen coordinates where 
            //  (0, 0) is the top left of the panel
            ScreenToCanvas(ref x, ref y);
            string s;
            Rectangle rect;
            foreach (Node n in nodes) {
                rect = NodeArtist.getPaddedRect(n);
                if (rect.Contains(x, y)) {
                    s = NodeArtist.hitJoint(n, rect, x, y, input);
                    return new Node.Address(n, s);
                }
            }
            return new Node.Address(null, null);
        }
    }
}
