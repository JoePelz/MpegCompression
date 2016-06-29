using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NodeShop.NodeProperties;

namespace NodeShop.Nodes {
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
        }
    }
}
