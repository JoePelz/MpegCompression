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
            Node nRead = new ReadImage();
            Node nCS1 = new ColorSpace();
            Node nCtCH = new ColorToChannels();
            Node nSS = new Subsample();
            Node nDCT = new DCT();
            Node nCHtC = new ChannelsToColor();
            Node nCHtC2 = new ChannelsToColor();
            Node nCS2 = new ColorSpace();
            Node nChunkTest = new TestChunker();

            nRead.pos = new System.Drawing.Point(-110, 25);
            nChunkTest.pos = new System.Drawing.Point(0, -10);
            nCS1.pos = new System.Drawing.Point(0, 50);
            nCtCH.pos = new System.Drawing.Point(110, 50);
            nSS.pos = new System.Drawing.Point(220, 75);
            nCHtC.pos = new System.Drawing.Point(330, 50);
            nCS2.pos = new System.Drawing.Point(440, 50);
            nDCT.pos = new System.Drawing.Point(330, 110);
            nCHtC2.pos = new System.Drawing.Point(440, 110);

            (nRead as ReadImage).setPath("C:\\temp\\sunrise.bmp");
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nSS as Subsample).setOutSamples(Subsample.Samples.s420);
            (nCS2 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);

            Node.connect(nRead, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCH, "inColor");
            Node.connect(nCtCH, "outChannels", nSS, "inChannels");
            Node.connect(nSS, "outChannels", nDCT, "inChannels");
            Node.connect(nSS, "outChannels", nCHtC, "inChannels");
            Node.connect(nDCT, "outChannels", nCHtC2, "inChannels");
            Node.connect(nCHtC, "outColor", nCS2, "inColor");
            Node.connect(nRead, "outColor", nChunkTest, "inColor");

            viewNodes.addNode(nRead);
            viewNodes.addNode(nCS1);
            viewNodes.addNode(nCtCH);
            viewNodes.addNode(nSS);
            viewNodes.addNode(nDCT);
            viewNodes.addNode(nCHtC);
            viewNodes.addNode(nCHtC2);
            viewNodes.addNode(nCS2);
            viewNodes.addNode(nChunkTest);
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
