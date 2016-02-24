using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public abstract class ColorNode : Node {

        protected override void createProperties() {
            base.createProperties();
            properties.Add("inColor", new Property(true, false));
            properties.Add("outColor", new Property(false, true));
        }

        protected override void clean() {
            base.clean();

            Address upstream = properties["inColor"].input;
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
