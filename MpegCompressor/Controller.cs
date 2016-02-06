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

            buildGraph();
            viewLeft.focusView();
            viewRight.focusView();
            viewNodes.focusView();
        }

        public void buildGraph() {
            Node n1 = new ReadImage();
            Node n2 = new ColorSpace();
            Node n3 = new Subsample();
            Node n4 = new Subsample();
            Node n5 = new ColorSpace();
            
            n1.pos = new System.Drawing.Point(0, 0);
            n2.pos = new System.Drawing.Point(110, 50);
            n3.pos = new System.Drawing.Point(220, 100);
            n4.pos = new System.Drawing.Point(330, 100);
            n5.pos = new System.Drawing.Point(440, 50);

            (n2 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (n3 as Subsample).setOutSamples(Subsample.Samples.s420);
            (n4 as Subsample).setOutSamples(Subsample.Samples.s444);
            (n5 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);

            Node.connect(n1, "outColor", n2, "inColor");
            Node.connect(n2, "outColor", n3, "inChannels");
            Node.connect(n3, "outChannels", n4, "inChannels");
            Node.connect(n4, "outChannels", n5, "inColor");

            viewNodes.addNode(n1);
            viewNodes.addNode(n2);
            viewNodes.addNode(n3);
            viewNodes.addNode(n4);
            viewNodes.addNode(n5);
        }

        public void OnSelectionChange(object sender, EventArgs e) {
            viewProps.clearProperties();
            Node selection = viewNodes.getSelection();
            if (selection != null) {
                viewProps.showProperties(selection);
            }
        }

        public bool HotKeys(Keys keys) {
            Node n;
            if (keys == Keys.D1) {
                n = viewNodes.getSelection();
                if (n != null) {
                    //load left view with selected node
                    viewLeft.setSource(n);
                    viewLeft.Invalidate();
                    viewNodes.Invalidate();
                }
                return true;
            } else if (keys == Keys.D2) {
                n = viewNodes.getSelection();
                if (n != null) {
                    //load right view with selected node
                    viewRight.setSource(n);
                    viewRight.Invalidate();
                    viewNodes.Invalidate();
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
