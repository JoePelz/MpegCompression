using MpegCompressor.NodeProperties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

        Point pos;

        private bool isDirty;
        private string extra;
        protected Dictionary<string, Property> properties;
        protected DataBlob state;

        public event EventHandler eViewChanged;

        public Node() {
            init();
        }

        public Node(NodeView graph) {
            init();
            graph.addNode(this);
        }

        public Node(NodeView graph, int posX, int posY) {
            init();
            graph.addNode(this);
            setPos(posX, posY);
        }

        protected virtual void init() {
            properties = new Dictionary<string, Property>();
            state = new DataBlob();
            PropertyString p = new PropertyString("default", "Name of the control");
            p.eValueChanged += (s, e) => fireOutputChanged(e);
            properties.Add("name", p);
            createProperties();
        }

        public void setPos(int x, int y) {
            pos.X = x;
            pos.Y = y;
        }

        public Point getPos() {
            return pos;
        }

        public void offsetPos(int x, int y) {
            pos.X += x;
            pos.Y += y;
        }

        public void rename(string newName) {
            properties["name"].sValue = newName;
        }

        public void setExtra(string sExtra) {
            extra = sExtra;
        }

        public string getName() {
            return properties["name"].sValue;
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

        protected bool addInput(string port, Node from, string fromPort) {
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

        protected bool addOutput(string port, Node to, string toPort) {
            //if there's an old connection, doesn't matter. Output can be 1..*
            HashSet<Address> cnx;
            if (properties.ContainsKey(port)) {
                cnx = properties[port].output;
                cnx.Add(new Address(to, toPort));
                return true;
            }
            return false;
        }

        protected void removeInput(string port) {
            //Note: only breaks this end of the connection.
            if (properties.ContainsKey(port)) {
                properties[port].input = null;
            }
            soil();
        }

        protected void removeOutput(string port, Node to, string toPort) {
            //Note: only breaks this end of the connection.
            if (properties.ContainsKey(port)) {
                properties[port].output.RemoveWhere(x => x.node == to && x.port.Equals(toPort));

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
            if (state != null && (state.type == DataBlob.Type.Image || state.bmp != null)) {
                return state.bmp;
            }
            if (state != null && state.type == DataBlob.Type.Channels && state.channels != null) {
                Size s = Subsample.deduceCbCrSize(state);

                Bitmap bmp = new Bitmap(state.channelWidth, state.channelHeight, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                //copy bytes
                int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
                byte[] rgbValues = new byte[nBytes];
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);


                //order: B,G,R,  B,G,R,  ...
                int channelIndex = 0;
                int counter = 0;
                int channelIndexRB = 0;
                for (int y = 0; y < state.imageHeight; y++) {
                    channelIndex = y * state.channelWidth;
                    channelIndexRB = y * s.Width;
                    counter = y * bmpData.Stride;

                    for (int x = 0; x < state.imageWidth; x++) {
                        rgbValues[counter + 2] = state.channels[0][channelIndex];
                        if (y < s.Height && x < s.Width) {
                            rgbValues[counter + 1] = state.channels[1][channelIndexRB];
                            rgbValues[counter + 0] = state.channels[2][channelIndexRB];
                            channelIndexRB++;
                        }
                        counter += 3;
                        channelIndex++;
                    }
                }
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

                bmp.UnlockBits(bmpData);
                state.bmp = bmp;
                return bmp;
            }
            
            //Debug.Write("View missing in " + properties["name"].getString() + "\n");
            return null;
        }

        public override string ToString() {
            return properties["name"].sValue;
        }
        
        public virtual void viewExtra(Graphics g) {
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
    }
}
