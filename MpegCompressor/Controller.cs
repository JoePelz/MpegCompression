using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class Controller {
        private PropertyPage viewProps;
        private Viewport viewLeft;
        private Viewport viewRight;
        private NodeView viewNodes;

        public Controller(PropertyPage props, NodeView nodes, Viewport left, Viewport right) {
            
            viewProps = props;
            viewLeft  = left;
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
            Node nIDCT = new DCT();
            Node nCHtC = new ChannelsToColor();
            Node nCHtC2 = new ChannelsToColor();
            Node nCHtC3 = new ChannelsToColor();
            Node nCS3 = new ColorSpace();
            Node nCS2 = new ColorSpace();
            Node nChunkTest = new TestChunker();
            Node nWriter = new WriteChannels();

            Node nReader = new ReadChannels();
            Node nCHtC4 = new ChannelsToColor();
            Node nIDCT2 = new DCT();
            Node nCS4 = new ColorSpace();

            nRead.setPos(-110, 25);
            nChunkTest.setPos(0, -10);
            nCS1.setPos(0, 50);
            nCtCH.setPos(110, 50);
            nSS.setPos(220, 75);
            nCHtC.setPos(330, 50);
            nCS2.setPos(440, 50);
            nDCT.setPos(330, 110);
            nCHtC2.setPos(440, 110);
            nIDCT.setPos(440, 170);
            nWriter.setPos(440, 230);
            nCHtC3.setPos(550, 170);
            nCS3.setPos(660, 170);

            nReader.setPos(-110, 290);
            nIDCT2.setPos(10, 290);
            nCHtC4.setPos(130, 290);
            nCS4.setPos(250, 290);
            
            (nRead as ReadImage).setPath("C:\\temp\\sunmid.bmp");
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nSS as Subsample).setOutSamples(Subsample.Samples.s420);
            (nCS2 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCS3 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCS4 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nIDCT as DCT).setInverse(true);
            (nIDCT2 as DCT).setInverse(true);

            Node.connect(nRead, "outColor", nChunkTest, "inColor");
            Node.connect(nRead, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCH, "inColor");
            Node.connect(nCtCH, "outChannels", nSS, "inChannels");
            Node.connect(nSS, "outChannels", nDCT, "inChannels");
            Node.connect(nSS, "outChannels", nCHtC, "inChannels");
            Node.connect(nDCT, "outChannels", nCHtC2, "inChannels");
            Node.connect(nCHtC, "outColor", nCS2, "inColor");
            Node.connect(nDCT, "outChannels", nIDCT, "inChannels");
            Node.connect(nDCT, "outChannels", nWriter, "inChannels");
            Node.connect(nIDCT, "outChannels", nCHtC3, "inChannels");
            Node.connect(nCHtC3, "outColor", nCS3, "inColor");

            Node.connect(nReader, "outChannels", nIDCT2, "inChannels");
            Node.connect(nIDCT2, "outChannels", nCHtC4, "inChannels");
            Node.connect(nCHtC4, "outColor", nCS4, "inColor");

            viewNodes.addNode(nRead);
            viewNodes.addNode(nCS1);
            viewNodes.addNode(nCtCH);
            viewNodes.addNode(nSS);
            viewNodes.addNode(nDCT);
            viewNodes.addNode(nIDCT);
            viewNodes.addNode(nCHtC);
            viewNodes.addNode(nCHtC2);
            viewNodes.addNode(nCHtC3);
            viewNodes.addNode(nCS3);
            viewNodes.addNode(nCS2);
            viewNodes.addNode(nWriter);
            viewNodes.addNode(nChunkTest);

            viewNodes.addNode(nReader);
            viewNodes.addNode(nIDCT2);
            viewNodes.addNode(nCHtC4);
            viewNodes.addNode(nCS4);
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
