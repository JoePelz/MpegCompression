using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
    public class NoOp : Node {

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
        
        public override DataBlob getData(string port) {
            if (inputs["inColor"] != null) {
                Node inputNode = inputs["inColor"].node;
                string inputPort = inputs["inColor"].port;
                return inputNode.getData(inputPort);
            }
            return null;
        }

        public override Bitmap view() {
            base.view();
            if (inputs.ContainsKey("inColor") && inputs["inColor"] != null) {
                return inputs["inColor"].node.view();
            } else {
                return null;
            }
        }
    }
}
