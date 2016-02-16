using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public abstract class ColorNode : Node {

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
            if (state == null) {
                return;
            }

            state = state.clone();

            if (state.type != DataBlob.Type.Image || state.bmp == null) {
                state = null;
                return;
            }

            state.bmp = state.bmp.Clone(new Rectangle(0, 0, state.bmp.Width, state.bmp.Height), state.bmp.PixelFormat);
        }
    }
}
