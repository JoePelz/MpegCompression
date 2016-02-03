using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class ColorSpace : Node {

        protected override void createInputs() {
            inputs.Add("inColor", null);
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("ColorSpace");

            //Will need to choose input color space
            //and output color space.
        }

        public override DataBlob getData(string port) {
            throw new NotImplementedException();
        }

        public override Rectangle getExtents() {
            throw new NotImplementedException();
        }
    }
}
