using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public abstract class PixelNode : Node {

        protected override void createInputs() {
            inputs.Add("inColor", null);
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
        }

        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Image;
            d.img = bmp;
            d.width = bmp.Width;
            d.height = bmp.Height;
            return d;
        }


        protected override void clean() {
            base.clean();
            bmp = null;

            Address upstream = inputs["inColor"];
            if (upstream == null) {
                return;
            }
            DataBlob dataIn = upstream.node.getData(upstream.port);
            if (dataIn == null) {
                return;
            }

            if (dataIn.type == DataBlob.Type.Image && dataIn.img != null) {
                bmp = dataIn.img.Clone(new Rectangle(0, 0, dataIn.img.Width, dataIn.img.Height), dataIn.img.PixelFormat);
            } else {
                return;
            }
        }
    }
}
