using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
    public class NoOp : Node {

        public NoOp() {
	    }

	    protected override void createProperties() {
            Property p = properties["name"];
            p.setString("NoOp");
        }

        protected override void createInputs() {
            inputs.Add("inColor", null);
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
        }

        /* check l8r */
        public override DataBlob getData(string port) {
            if (inputs.ContainsKey("inColor") && inputs["inColor"] != null) {
                Node inputNode = inputs["inColor"].node;
                string inputPort = inputs["inColor"].port;
                return inputNode.getData(inputPort);
            }
            return null;
        }

        protected override void clean() {
            //read inputs and update output...
        }

        public override void view(PaintEventArgs pe) {
            base.view(pe);
            if (inputs.ContainsKey("inColor") && inputs["inColor"] != null) {
                inputs["inColor"].node.view(pe);
            } else {
                Graphics g = pe.Graphics;
                g.DrawLine(SystemPens.ControlDarkDark, -15, -15, 15, 15);
                g.DrawLine(SystemPens.ControlDarkDark, 15, -15, -15, 15);
            }
        }

        public override Rectangle getExtents() {
            if (inputs.ContainsKey("inColor") && inputs["inColor"] != null) {
                return inputs["inColor"].node.getExtents();
            }
            return new Rectangle(-15, -15, 31, 31);
        }
    }
}
