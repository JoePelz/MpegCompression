using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
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

        //private bool isSelected;
        private bool isDirty;
        private string extra;
        protected Dictionary<string, Property> properties;
        protected Dictionary<string, Address> inputs;
        protected Dictionary<string, HashSet<Address>> outputs;
        protected DataBlob state;
        public Point pos;

        public event EventHandler eViewChanged;

        public Node() {
            properties = new Dictionary<string, Property>();
            inputs = new Dictionary<string, Address>();
            outputs = new Dictionary<string, HashSet<Address>>();
            state = new DataBlob();
            
            Property p = new Property();
            p.createString("default", "Name of the control");
            p.eValueChanged += (s, e) => fireOutputChanged(e);
            properties.Add("name", p);
            createProperties();
            createInputs();
            createOutputs();
        }

        public void rename(string newName) {
            properties["name"].setString(newName);
        }

        public void setExtra(string sExtra) {
            extra = sExtra;
        }

        public string getName() {
            return properties["name"].getString();
        }

        public string getExtra() {
            return extra;
        }

        protected virtual void createProperties() { }

        protected virtual void createInputs() { }

        protected virtual void createOutputs() { }

        public Dictionary<string, Address> getInputs() {
            return inputs;
        }

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
            if (inputs.ContainsKey(port)) {
                //if there's an old connection, disconnect both ends
                if (inputs[port] != null) {
                    inputs[port].node.removeOutput(inputs[port].port, this, port);
                    inputs[port] = null;
                }
                //place the new connection
                inputs[port] = new Address(from, fromPort);
                soil();
                return true;
            }
            //else fail
            return false;
        }

        protected virtual bool addOutput(string port, Node to, string toPort) {
            //if there's an old connection, doesn't matter. Output can be 1..*
            HashSet<Address> cnx;
            if (outputs.ContainsKey(port)) {
                cnx = outputs[port];
                cnx.Add(new Address(to, toPort));
                return true;
            }
            return false;
        }

        protected virtual void removeInput(string port) {
            //Note: only breaks this end of the connection.
            if (inputs.ContainsKey(port)) {
                inputs[port] = null;
            }
            soil();
        }

        protected virtual void removeOutput(string port, Node to, string toPort) {
            //Note: only breaks this end of the connection.
            Address match = new Address(to, toPort);
            if (inputs.ContainsKey(port)) {
                //TODO: test this. It uses .Equals() to find the match right?
                outputs[port].Remove(match);  //returns false if item not found.
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
            foreach (KeyValuePair<string, HashSet<Address>> kvp in outputs) {
                foreach (Address a in kvp.Value) {
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
            return state.bmp;
        }
    }
}
