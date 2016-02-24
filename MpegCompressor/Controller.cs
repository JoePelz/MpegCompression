using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.Nodes;

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

        public void DCTTest() {
            Node nRead = new ReadImage();
            Node nCS1 = new ColorSpace();
            Node nCtCH = new ColorToChannels();
            Node nSS = new Subsample();
            Node nDCT = new DCT();
            Node nIDCT = new DCT();
            Node nCHtC2 = new ChannelsToColor();
            Node nCHtC3 = new ChannelsToColor();
            Node nCS3 = new ColorSpace();
            
            nRead.setPos(-160, 0);
            nCS1.setPos(-35, 0);
            nCtCH.setPos(90, 0);
            nSS.setPos(200, 0);
            nDCT.setPos(320, 0);
            nCHtC2.setPos(440, -60);
            nIDCT.setPos(440, 60);
            nCHtC3.setPos(550, 60);
            nCS3.setPos(660, 60);

            (nRead as ReadImage).setPath("C:\\temp\\sunmid.bmp");
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nSS as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nCS3 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nIDCT as DCT).setInverse(true);
            
            Node.connect(nRead, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCH, "inColor");
            Node.connect(nCtCH, "outChannels", nSS, "inChannels");
            Node.connect(nSS, "outChannels", nDCT, "inChannels");
            Node.connect(nDCT, "outChannels", nCHtC2, "inChannels");
            Node.connect(nDCT, "outChannels", nIDCT, "inChannels");
            Node.connect(nIDCT, "outChannels", nCHtC3, "inChannels");
            Node.connect(nCHtC3, "outColor", nCS3, "inColor");

            viewNodes.addNode(nRead);
            viewNodes.addNode(nCS1);
            viewNodes.addNode(nCtCH);
            viewNodes.addNode(nSS);
            viewNodes.addNode(nDCT);
            viewNodes.addNode(nIDCT);
            viewNodes.addNode(nCHtC2);
            viewNodes.addNode(nCHtC3);
            viewNodes.addNode(nCS3);
        }

        public void readWriteTest() {
            Node nRead = new ReadImage();
            Node nCS1 = new ColorSpace();
            Node nCtCH = new ColorToChannels();
            Node nSS = new Subsample();
            Node nDCT = new DCT();
            Node nWriter = new WriteChannels();

            Node nReader = new ReadChannels();
            Node nCHtC4 = new ChannelsToColor();
            Node nIDCT2 = new DCT();
            Node nCS4 = new ColorSpace();



            nRead.setPos(-150, 0);
            nCS1.setPos(10, 0);
            nCtCH.setPos(130, 0);
            nSS.setPos(250, 0);
            nDCT.setPos(370, 0);
            nWriter.setPos(490, 0);

            nReader.setPos(-150, 100);
            nIDCT2.setPos(10, 100);
            nCHtC4.setPos(130, 100);
            nCS4.setPos(250, 100);

            (nRead as ReadImage).setPath("C:\\temp\\sunmid.bmp");
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nSS as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nCS4 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nIDCT2 as DCT).setInverse(true);
            
            Node.connect(nRead, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCH, "inColor");
            Node.connect(nCtCH, "outChannels", nSS, "inChannels");
            Node.connect(nSS, "outChannels", nDCT, "inChannels");
            Node.connect(nDCT, "outChannels", nWriter, "inChannels");

            Node.connect(nReader, "outChannels", nIDCT2, "inChannels");
            Node.connect(nIDCT2, "outChannels", nCHtC4, "inChannels");
            Node.connect(nCHtC4, "outColor", nCS4, "inColor");

            viewNodes.addNode(nRead);
            viewNodes.addNode(nCS1);
            viewNodes.addNode(nCtCH);
            viewNodes.addNode(nSS);
            viewNodes.addNode(nDCT);
            viewNodes.addNode(nWriter);

            viewNodes.addNode(nReader);
            viewNodes.addNode(nIDCT2);
            viewNodes.addNode(nCHtC4);
            viewNodes.addNode(nCS4);
        }

        public void mergeTest() {
            Node nR1 = new ReadImage();
            Node nR2 = new ReadImage();
            Node nM = new Merge();

            nR1.setPos(0, -50);
            nR2.setPos(0, 50);
            nM.setPos(180, 0);

            (nR1 as ReadImage).setPath("C:\\temp\\uv.jpg");
            (nR2 as ReadImage).setPath("C:\\temp\\lena.tif");

            Node.connect(nR1, "outColor", nM, "inColorA");
            Node.connect(nR2, "outColor", nM, "inColorB");

            viewNodes.addNode(nR1);
            viewNodes.addNode(nR2);
            viewNodes.addNode(nM);
        }

        public void moVecTest() {
            Node nR1 = new ReadImage();
            Node nCtCh1 = new ColorToChannels();
            Node nR2 = new ReadImage();
            Node nCtCh2 = new ColorToChannels();
            Node nM = new MoVecDecompose();
            Node nC = new MoVecCompose();
            Node nChtC = new ChannelsToColor();

            nR1.setPos(0, -50);
            nCtCh1.setPos(180, -50);
            nR2.setPos(0, 50);
            nCtCh2.setPos(180, 50);
            nM.setPos(330, 0);
            nC.setPos(520, 50);
            nChtC.setPos(700, 50);

            (nR1 as ReadImage).setPath("C:\\temp\\lena.tif");
            (nR2 as ReadImage).setPath("C:\\temp\\uv.jpg");
            //(nR1 as ReadImage).setPath("C:\\temp\\nomadA.jpg");
            //(nR2 as ReadImage).setPath("C:\\temp\\nomadB.jpg");
            //(nR1 as ReadImage).setPath("C:\\temp\\sunA.bmp");
            //(nR2 as ReadImage).setPath("C:\\temp\\sunB.bmp");
            //(nR1 as ReadImage).setPath("C:\\temp\\stripA.bmp");
            //(nR2 as ReadImage).setPath("C:\\temp\\stripB.bmp");

            Node.connect(nR1, "outColor", nCtCh1, "inColor");
            Node.connect(nR2, "outColor", nCtCh2, "inColor");
            Node.connect(nCtCh1, "outChannels", nM, "inChannelsNow");
            Node.connect(nCtCh2, "outChannels", nM, "inChannelsPast");

            Node.connect(nM, "outVectors", nC, "inVectors");
            Node.connect(nM, "outChannels", nC, "inChannels");
            Node.connect(nCtCh2, "outChannels", nC, "inChannelsPast");
            Node.connect(nC, "outChannels", nChtC, "inChannels");

            viewNodes.addNode(nR1);
            viewNodes.addNode(nCtCh1);
            viewNodes.addNode(nR2);
            viewNodes.addNode(nCtCh2);
            viewNodes.addNode(nM);
            viewNodes.addNode(nC);
            viewNodes.addNode(nChtC);
        }

        public void buildGraph() {
            //DCTTest();
            //readWriteTest();
            //mergeTest();
            moVecTest();
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
