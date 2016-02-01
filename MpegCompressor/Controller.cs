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

        public bool HotKeys(Keys keys) {
            Node n;
            if (keys == Keys.D1) {
                n = viewNodes.getSelection();
                if (n != null) {
                    //load left view with selected node
                }
                return true;
            } else if (keys == Keys.D2) {
                n = viewNodes.getSelection();
                if (n != null) {
                    //load right view with selected node
                }
                return true;
            }

            /*  Also available:
            switch (keys) {
                case Keys.C | Keys.Control:
                    doStuff();
                    return true;
            }
            */
            return false;
        }
    }
}
