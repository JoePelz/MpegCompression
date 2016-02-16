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

        protected override void clean() {
            base.clean();

            Address upstream = inputs["inColor"];
            if (upstream == null) {
                return;
            }

            state = upstream.node.getData(upstream.port);
        }
    }
}
