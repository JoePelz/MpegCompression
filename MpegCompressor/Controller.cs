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
            
            nRead.setPos(-180, 0);
            nCS1.setPos(-55, 0);
            nCtCH.setPos(65, 0);
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
            Node nR1 = new ReadImage(viewNodes, 0, -50);
            Node nCtCh1 = new ColorToChannels(viewNodes, 180, -50);
            Node nR2 = new ReadImage(viewNodes, 0, 50);
            Node nCtCh2 = new ColorToChannels(viewNodes, 180, 50);
            Node nM = new MoVecDecompose(viewNodes, 330, 0);
            Node nC = new MoVecCompose(viewNodes, 520, 20);

            Node nSS1 = new Subsample(viewNodes, 330, 150);
            Node nSS2 = new Subsample(viewNodes, 330, 250);
            Node nM2 = new MoVecDecompose(viewNodes, 470, 150);
            Node nC2 = new MoVecCompose(viewNodes, 650, 200);

            Node.connect(nR1, "outColor", nCtCh1, "inColor");
            Node.connect(nR2, "outColor", nCtCh2, "inColor");
            Node.connect(nCtCh1, "outChannels", nM, "inChannelsNow");
            Node.connect(nCtCh2, "outChannels", nM, "inChannelsPast");
            Node.connect(nM, "outVectors", nC, "inVectors");
            Node.connect(nM, "outChannels", nC, "inChannels");
            Node.connect(nCtCh2, "outChannels", nC, "inChannelsPast");

            Node.connect(nCtCh1, "outChannels", nSS1, "inChannels");
            Node.connect(nCtCh2, "outChannels", nSS2, "inChannels");
            Node.connect(nSS1, "outChannels", nM2, "inChannelsNow");
            Node.connect(nSS2, "outChannels", nM2, "inChannelsPast");
            Node.connect(nM2, "outVectors", nC2, "inVectors");
            Node.connect(nM2, "outChannels", nC2, "inChannels");
            Node.connect(nSS2, "outChannels", nC2, "inChannelsPast");

            (nR1 as ReadImage).setPath("C:\\temp\\barbieA.tif");
            (nR2 as ReadImage).setPath("C:\\temp\\barbieB.tif");
            //(nR1 as ReadImage).setPath("C:\\temp\\nomadA.jpg");
            //(nR2 as ReadImage).setPath("C:\\temp\\nomadB.jpg");
            //(nR1 as ReadImage).setPath("C:\\temp\\sunA.bmp");
            //(nR2 as ReadImage).setPath("C:\\temp\\sunB.bmp");
            //(nR1 as ReadImage).setPath("C:\\temp\\stripA.bmp");
            //(nR2 as ReadImage).setPath("C:\\temp\\stripB.bmp");
            (nSS1 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS1 as Subsample).setPadded(true);
            (nSS2 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setPadded(true);
        }

        public void mpegTest() {
            //first frame
            Node nRead1 = new ReadImage(viewNodes, 0, 0);
            Node nCS1 = new ColorSpace(viewNodes, 130, 0);
            Node nCtCh1 = new ColorToChannels(viewNodes, 260, 0);
            Node nSS1 = new Subsample(viewNodes, 390, 0);
            Node nDCT1 = new DCT(viewNodes, 520, 0);
            Node nIDCT1 = new DCT(viewNodes, 520, 100);

            //second frame
            Node nRead2 = new ReadImage(viewNodes, 0, 200);
            Node nCS2 = new ColorSpace(viewNodes, 130, 200);
            Node nCtCh2 = new ColorToChannels(viewNodes, 260, 200);
            Node nSS2 = new Subsample(viewNodes, 390, 200);
            Node nMoVec2 = new MoVecDecompose(viewNodes, 520, 200);
            Node nDCT2 = new DCT(viewNodes, 680, 200);
            Node nIDCT2 = new DCT(viewNodes, 680, 270);
            Node nIMoVec2 = new MoVecCompose(viewNodes, 680, 360);
            
            //Third frame
            Node nRead3 = new ReadImage(viewNodes, 0, 500);
            Node nCS3 = new ColorSpace(viewNodes, 130, 500);
            Node nCtCh3 = new ColorToChannels(viewNodes, 260, 500);
            Node nSS3 = new Subsample(viewNodes, 390, 500);
            Node nMoVec3 = new MoVecDecompose(viewNodes, 520, 500);
            Node nDCT3 = new DCT(viewNodes, 680, 500);

            Node nWrite = new WriteMultiChannel(viewNodes, 1000, 200);

            Node.connect(nRead1, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCh1, "inColor");
            Node.connect(nCtCh1, "outChannels", nSS1, "inChannels");
            Node.connect(nSS1, "outChannels", nDCT1, "inChannels");
            Node.connect(nDCT1, "outChannels", nIDCT1, "inChannels");
            Node.connect(nIDCT1, "outChannels", nMoVec2, "inChannelsPast");
            Node.connect(nIDCT1, "outChannels", nIMoVec2, "inChannelsPast");

            Node.connect(nRead2, "outColor", nCS2, "inColor");
            Node.connect(nCS2, "outColor", nCtCh2, "inColor");
            Node.connect(nCtCh2, "outChannels", nSS2, "inChannels");
            Node.connect(nSS2, "outChannels", nMoVec2, "inChannelsNow");
            Node.connect(nMoVec2, "outChannels", nDCT2, "inChannels");
            Node.connect(nDCT2, "outChannels", nIDCT2, "inChannels");
            Node.connect(nIDCT2, "outChannels", nIMoVec2, "inChannels");
            //Node.connect(nMoVec2, "outChannels", nIMoVec2, "inChannels");
            Node.connect(nMoVec2, "outVectors", nIMoVec2, "inVectors");
            Node.connect(nIMoVec2, "outChannels", nMoVec3, "inChannelsPast");

            Node.connect(nRead3, "outColor", nCS3, "inColor");
            Node.connect(nCS3, "outColor", nCtCh3, "inColor");
            Node.connect(nCtCh3, "outChannels", nSS3, "inChannels");
            Node.connect(nSS3, "outChannels", nMoVec3, "inChannelsNow");
            Node.connect(nMoVec3, "outChannels", nDCT3, "inChannels");

            Node.connect(nDCT1, "outChannels", nWrite, "inChannels1");
            Node.connect(nDCT2, "outChannels", nWrite, "inChannels2");
            Node.connect(nMoVec2, "outVectors", nWrite, "inVectors2");
            Node.connect(nDCT3, "outChannels", nWrite, "inChannels3");
            Node.connect(nMoVec3, "outVectors", nWrite, "inVectors3");


            (nRead1 as ReadImage).setPath("C:\\temp\\barbieA.tif");
            (nRead2 as ReadImage).setPath("C:\\temp\\barbieB.tif");
            (nRead3 as ReadImage).setPath("C:\\temp\\barbieC.tif");
            (nIDCT1 as DCT).setInverse(true);
            (nIDCT1 as DCT).rename("IDCT");
            (nIDCT2 as DCT).setInverse(true);
            (nIDCT2 as DCT).rename("IDCT");
            (nSS1 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setPadded(true);
            (nSS3 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS3 as Subsample).setPadded(true);
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nCS2 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nCS3 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nWrite as WriteMultiChannel).setPath("C:\\temp\\testVid.mdct");
        }

        public void buildGraph() {
            //DCTTest();
            //readWriteTest();
            //mergeTest();
            //moVecTest();
            mpegTest();
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
