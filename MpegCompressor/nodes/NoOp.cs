using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.Nodes {
    public class NoOp : Node {

        public NoOp(): base() { }
        public NoOp(NodeView graph) : base(graph) { }
        public NoOp(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("NoOp");
        }

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
        }
    }
}
