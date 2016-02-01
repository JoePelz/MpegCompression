using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class Controller {
        private Project model;
        private PropertyPage viewProps;
        private Viewport viewLeft;
        private Viewport viewRight;
        private NodeView viewNodes;

        public Controller(PropertyPage props, NodeView nodes, Viewport left, Viewport right) {
            model = new Project();

            viewProps = props;
            viewLeft = left;
            viewRight = right;
            viewNodes = nodes;
            viewNodes.eSelectionChanged += OnSelectionChange;

            Property p1 = new Property();
            Property p2 = new Property();
            Property p3 = new Property();
            p1.setInt(52, 50, 100, "Number of pigeons");
            p2.setInt(30, 0, 1000, "length of coastline (km)");
            p3.setInt(10, -40, 60, "ambient temperature (C)");
            viewProps.addProperty(p1);
            viewProps.addProperty(p3);
            viewProps.addProperty(p2);

            Node n1 = new Node();
            Node n2 = new Node();
            viewNodes.addNode(n1);
            viewNodes.addNode(n2);
        }

        public void OnSelectionChange(object sender, EventArgs e) {
            viewProps.clearProperties();
            Node selection = viewNodes.getSelection();
            if (selection != null) {
                viewProps.addProperties(selection.getProperties());
            }
        }
    }
}
