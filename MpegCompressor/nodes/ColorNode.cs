using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.NodeProperties;

namespace MpegCompressor.Nodes {
    public abstract class ColorNode : Node {

        public ColorNode(): base() { }
        public ColorNode(NodeView graph) : base(graph) { }
        public ColorNode(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void createProperties() {
            base.createProperties();
            properties.Add("inColor", new PropertyColor(true, false));
            properties.Add("outColor", new PropertyColor(false, true));
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

            if (state.type != DataBlob.Type.Image || state.bmp == null) {
                state = null;
                return;
            }

            state = state.clone();

            state.bmp = state.bmp.Clone(new Rectangle(0, 0, state.bmp.Width, state.bmp.Height), state.bmp.PixelFormat);
        }
    }
}
