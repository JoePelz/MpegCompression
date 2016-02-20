using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.Nodes {
    //TODO: have node not extend panel, and be drawn purely by NodeView.
    public abstract class Node : IViewable, IProperties {
        public class Address {
            public Node node;
            public string port;
            public Address(Node n, string s) {
                node = n;
                port = s;
            }
            public bool Equals(Address other) {
                return node == other.node && port.Equals(other.port);
            }
        }

        private static Font nodeFont = new Font("Tahoma", 11.0f);
        private static Font nodeExtraFont = new Font("Tahoma", 11.0f, FontStyle.Italic);
        private static Font nodeTitleFont = new Font("Tahoma", 13.0f, FontStyle.Bold);
        private static int ballSize = nodeFont.Height / 2;
        private static int ballOffset = (nodeFont.Height - ballSize) / 2;
        private int titleWidth;
        private Rectangle nodeRect;
        private bool isDirty;
        private string extra;
        protected Dictionary<string, Property> properties;
        protected DataBlob state;

        public event EventHandler eViewChanged;

        public Node() {
            properties = new Dictionary<string, Property>();
            state = new DataBlob();
            titleWidth = 100;
            Property p = new Property(false, false);
            p.createString("default", "Name of the control");
            p.eValueChanged += (s, e) => fireOutputChanged(e);
            properties.Add("name", p);
            createProperties();

            nodeRect = new Rectangle(0, 0, 100, 100);
        }

        internal Rectangle getNodeRect() {
            return nodeRect;
        }

        public void setPos(int x, int y) {
            nodeRect.X = x;
            nodeRect.Y = y;
        }

        public void offsetPos(int x, int y) {
            nodeRect.X += x;
            nodeRect.Y += y;
        }

        private void updateGraphRect() {
            nodeRect.Width = Math.Max(100, titleWidth);
            nodeRect.Height = nodeTitleFont.Height;

            //count the lines to cover with text
            if (getExtra() != null) {
                nodeRect.Height += nodeExtraFont.Height;
            }
            foreach (var kvp in getProperties()) {
                if (kvp.Value.getType() == Property.Type.NONE) {
                    nodeRect.Height += nodeFont.Height;
                }
            }
        }

        public void rename(string newName) {
            properties["name"].setString(newName);
            titleWidth = System.Windows.Forms.TextRenderer.MeasureText(newName, nodeTitleFont).Width;
            updateGraphRect();
        }

        public void setExtra(string sExtra) {
            extra = sExtra;
            updateGraphRect();
        }

        public string getName() {
            return properties["name"].getString();
        }

        public string getExtra() {
            return extra;
        }

        protected virtual void createProperties() { }

        public static bool connect(Node from, string fromPort, Node to, string toPort) {
            if (!from.addOutput(fromPort, to, toPort))
                return false;
            if (!to.addInput(toPort, from, fromPort)) {
                from.removeOutput(fromPort, to, toPort);
                return false;
            }
            return true;
        }

        public static void disconnect(Node from, string fromPort, Node to, string toPort) {
            from.removeOutput(fromPort, to, toPort);
            to.removeInput(toPort);
        }

        protected virtual bool addInput(string port, Node from, string fromPort) {
            //if the port is valid
            if (properties.ContainsKey(port)) {
                //if there's an old connection, disconnect both ends
                if (properties[port].input != null) {
                    properties[port].input.node.removeOutput(properties[port].input.port, this, port);
                    properties[port].input = null;
                }
                //place the new connection
                properties[port].input = new Address(from, fromPort);
                soil();
                return true;
            }
            //else fail
            return false;
        }

        protected virtual bool addOutput(string port, Node to, string toPort) {
            //if there's an old connection, doesn't matter. Output can be 1..*
            HashSet<Address> cnx;
            if (properties.ContainsKey(port)) {
                cnx = properties[port].output;
                cnx.Add(new Address(to, toPort));
                return true;
            }
            return false;
        }

        protected virtual void removeInput(string port) {
            //Note: only breaks this end of the connection.
            if (properties.ContainsKey(port)) {
                properties[port].input = null;
            }
            soil();
        }

        protected virtual void removeOutput(string port, Node to, string toPort) {
            //Note: only breaks this end of the connection.
            Address match = new Address(to, toPort);
            if (properties.ContainsKey(port)) {
                //TODO: test this. It uses .Equals() to find the match right?
                properties[port].output.Remove(match);  //returns false if item not found.
            }
        }

        public virtual DataBlob getData(string port) {
            if (isDirty) {
                clean();
            }
            return state;
        }

        public Dictionary<string, Property> getProperties() {
            return properties;
        }

        protected void soil() {
            isDirty = true;
            foreach (KeyValuePair<string, Property> kvp in properties) {
                if (!kvp.Value.isOutput) {
                    continue;
                }
                foreach (Address a in kvp.Value.output) {
                    a.node.soil();
                }
            }
            fireOutputChanged(new EventArgs());
        }

        protected virtual void clean() {
            isDirty = false;
        }
        
        private void fireOutputChanged(EventArgs e) {
            EventHandler handler = eViewChanged;
            if (handler != null) {
                handler(this, e);
            }
        }
        
        public virtual Bitmap view() {
            if (isDirty) {
                clean();
            }
            if (state != null) {
                return state.bmp;
            }
            //Debug.Write("View missing in " + properties["name"].getString() + "\n");
            return null;
        }
        
        public void viewExtra(Graphics g) {
            if (state == null || state.bmp == null) {
                return;
            }
            //draw corner crosses.
            //bottom left
            g.DrawLine(Pens.BlanchedAlmond, -0.5f, -10, -0.5f, 10);
            g.DrawLine(Pens.BlanchedAlmond, -10, -0.5f, 10, -0.5f);

            //bottom right
            g.DrawLine(Pens.BlanchedAlmond, state.bmp.Width + 0.5f, -10, state.bmp.Width + 0.5f, 10);
            g.DrawLine(Pens.BlanchedAlmond, state.bmp.Width - 10, -0.5f, state.bmp.Width + 10, -0.5f);

            //top right
            g.DrawLine(Pens.BlanchedAlmond, state.bmp.Width + 0.5f, state.bmp.Height - 10, state.bmp.Width + 0.5f, state.bmp.Height + 10);
            g.DrawLine(Pens.BlanchedAlmond, state.bmp.Width - 10, state.bmp.Height + 0.5f, state.bmp.Width + 10, state.bmp.Height + 0.5f);

            //top left
            g.DrawLine(Pens.BlanchedAlmond, -0.5f, state.bmp.Height - 10, -0.5f, state.bmp.Height + 10);
            g.DrawLine(Pens.BlanchedAlmond, -10, state.bmp.Height + 0.5f, +10, state.bmp.Height + 0.5f);
        }

        public Point getJointPos(string port, bool input) {
            Point result = new Point(nodeRect.X, nodeRect.Y);
            result.Y += nodeTitleFont.Height;
            if (getExtra() != null) {
                result.Y += nodeExtraFont.Height;
            }
            
            foreach (var kvp in getProperties()) {
                if (kvp.Key == port) {
                    break;
                }
                if (kvp.Value.getType() == Property.Type.NONE) {
                    result.Y += nodeFont.Height;
                }
            }
            if (!input) {
                result.X += Math.Max(100, titleWidth);
            }
            result.Y += nodeFont.Height / 2 - ballSize / 2;
            return result;
        }

        public void drawGraphNode(Graphics g, bool isSelected) {
            Point drawPos = nodeRect.Location;
            //draw background
            if (isSelected) {
                g.FillRectangle(Brushes.Wheat, nodeRect);
            } else {
                g.FillRectangle(Brushes.CadetBlue, nodeRect);
            }
            g.DrawRectangle(Pens.Black, nodeRect);

            //draw title
            g.DrawString(getName(), nodeTitleFont, Brushes.Black, drawPos.X + (nodeRect.Width - titleWidth) / 2, drawPos.Y);
            drawPos.Y += nodeFont.Height;
            g.DrawLine(Pens.Black, nodeRect.Left, drawPos.Y, nodeRect.Right, drawPos.Y);


            //draw extra
            if (getExtra() != null) {
                g.DrawString(getExtra(), nodeExtraFont, Brushes.Black, drawPos);
                drawPos.Y += nodeFont.Height;
                g.DrawLine(Pens.Black, nodeRect.Left, drawPos.Y, nodeRect.Right, drawPos.Y);
            }

            //draw properties
            foreach (var kvp in getProperties()) {
                if (kvp.Value.getType() == Property.Type.NONE) {
                    g.DrawString(kvp.Key, nodeFont, Brushes.Black, nodeRect.Left + (nodeRect.Width - g.MeasureString(kvp.Key, nodeFont).Width) / 2, drawPos.Y);

                    //draw bubbles
                    if (kvp.Value.isInput) {
                        if (kvp.Value.input != null) {
                            g.FillEllipse(Brushes.Black, nodeRect.Left - ballSize / 2, drawPos.Y + ballOffset, ballSize, ballSize);
                        } else {
                            g.DrawEllipse(Pens.Black, nodeRect.Left - ballSize / 2, drawPos.Y + ballOffset, ballSize, ballSize);
                        }
                    } else if (kvp.Value.isOutput) {
                        if (kvp.Value.output.Any()) {
                            g.FillEllipse(Brushes.Black, nodeRect.Right - ballSize / 2, drawPos.Y + ballOffset, ballSize, ballSize);
                        } else {
                            g.DrawEllipse(Pens.Black, nodeRect.Right - ballSize / 2, drawPos.Y + ballOffset, ballSize, ballSize);
                        }
                    }
                    drawPos.Y += nodeFont.Height;
                }
            }
        }

        public bool hitTest(int x, int y) {
            return x >= nodeRect.Left && x < nodeRect.Right && y >= nodeRect.Top && y < nodeRect.Bottom;
        }
    }
}
