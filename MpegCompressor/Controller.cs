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
            Node nRead = new ReadImage(viewNodes, -180, 0);
            Node nCS1 = new ColorSpace(viewNodes, -55, 0);
            Node nCtCH = new ColorToChannels(viewNodes, 65, 0);
            Node nSS = new Subsample(viewNodes, 200, 0);
            Node nDCT = new DCT(viewNodes, 320, 0);
            Node nIDCT = new DCT(viewNodes, 440, 0);
            Node nCHtC3 = new ChannelsToColor(viewNodes, 550, 0);
            Node nCS3 = new ColorSpace(viewNodes, 660, 0);
            
            (nRead as ReadImage).setPath("C:\\temp\\sunmid.bmp");
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nSS as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nCS3 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nIDCT as DCT).setInverse(true);
            (nIDCT as DCT).rename("IDCT");

            Node.connect(nRead, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCH, "inColor");
            Node.connect(nCtCH, "outChannels", nSS, "inChannels");
            Node.connect(nSS, "outChannels", nDCT, "inChannels");
            Node.connect(nDCT, "outChannels", nIDCT, "inChannels");
            Node.connect(nIDCT, "outChannels", nCHtC3, "inChannels");
            Node.connect(nCHtC3, "outColor", nCS3, "inColor");
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
            Node nR1 = new ReadImage(viewNodes, -50, -50);
            Node nCtCh1 = new ColorToChannels(viewNodes, 130, -50);
            Node nR2 = new ReadImage(viewNodes, -50, 50);
            Node nCtCh2 = new ColorToChannels(viewNodes, 130, 50);
            Node nM = new MoVecDecompose(viewNodes, 330, 10);
            Node nC = new MoVecCompose(viewNodes, 520, -50);

            Node nSS1 = new Subsample(viewNodes, 330, 150);
            Node nSS2 = new Subsample(viewNodes, 330, 250);
            Node nM2 = new MoVecDecompose(viewNodes, 500, 250);
            Node nC2 = new MoVecCompose(viewNodes, 700, 200);

            Node.connect(nR1, "outColor", nCtCh1, "inColor");
            Node.connect(nR2, "outColor", nCtCh2, "inColor");
            Node.connect(nCtCh1, "outChannels", nM, "inChannelsPast");
            Node.connect(nCtCh2, "outChannels", nM, "inChannelsNow");
            Node.connect(nM, "outVectors", nC, "inVectors");
            Node.connect(nCtCh1, "outChannels", nC, "inChannelsPast");
            Node.connect(nM, "outChannels", nC, "inChannels");

            Node.connect(nCtCh1, "outChannels", nSS1, "inChannels");
            Node.connect(nCtCh2, "outChannels", nSS2, "inChannels");
            Node.connect(nSS1, "outChannels", nM2, "inChannelsPast");
            Node.connect(nSS2, "outChannels", nM2, "inChannelsNow");
            Node.connect(nM2, "outVectors", nC2, "inVectors");
            Node.connect(nM2, "outChannels", nC2, "inChannels");
            Node.connect(nSS1, "outChannels", nC2, "inChannelsPast");

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

        public void mpegWriteTest() {
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

            Node nWrite = new WriteMulti3Channel(viewNodes, 1000, 200);

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


            //(nRead1 as ReadImage).setPath("C:\\temp\\barbieA.tif");
            //(nRead2 as ReadImage).setPath("C:\\temp\\barbieB.tif");
            //(nRead3 as ReadImage).setPath("C:\\temp\\barbieC.tif");
            (nRead1 as ReadImage).setPath("C:\\temp\\bmA.tif");
            (nRead2 as ReadImage).setPath("C:\\temp\\bmB.tif");
            (nRead3 as ReadImage).setPath("C:\\temp\\bmC.tif");
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
            (nWrite as WriteMulti3Channel).setPath("C:\\temp\\testVid.mdct");
        }

        public void mpegReadTest() {
            Node nRM = new ReadMulti3Channel(viewNodes, 0, 0);
            Node nIDCT1 = new DCT(viewNodes, 150, -100);
            Node nIDCT2 = new DCT(viewNodes, 150, 0);
            Node nIDCT3 = new DCT(viewNodes, 150, 100);
            Node nIMoVec2 = new MoVecCompose(viewNodes, 300, -50);
            Node nIMoVec3 = new MoVecCompose(viewNodes, 300, 100);
            Node nChtC1 = new ChannelsToColor(viewNodes, 550, -100);
            Node nChtC2 = new ChannelsToColor(viewNodes, 550, 0);
            Node nChtC3 = new ChannelsToColor(viewNodes, 550, 100);
            Node nCS1 = new ColorSpace(viewNodes, 700, -100);
            Node nCS2 = new ColorSpace(viewNodes, 700, 0);
            Node nCS3 = new ColorSpace(viewNodes, 700, 100);

            Node.connect(nRM, "outChannels1", nIDCT1, "inChannels");
            Node.connect(nRM, "outChannels2", nIDCT2, "inChannels");
            Node.connect(nRM, "outChannels3", nIDCT3, "inChannels");
            Node.connect(nIDCT1, "outChannels", nIMoVec2, "inChannelsPast");
            Node.connect(nIDCT2, "outChannels", nIMoVec2, "inChannels");
            Node.connect(nRM, "outVectors2", nIMoVec2, "inVectors");
            Node.connect(nIMoVec2, "outChannels", nIMoVec3, "inChannelsPast");
            Node.connect(nIDCT3, "outChannels", nIMoVec3, "inChannels");
            Node.connect(nRM, "outVectors3", nIMoVec3, "inVectors");
            Node.connect(nIDCT1, "outChannels", nChtC1, "inChannels");
            Node.connect(nIMoVec2, "outChannels", nChtC2, "inChannels");
            Node.connect(nIMoVec3, "outChannels", nChtC3, "inChannels");
            Node.connect(nChtC1, "outColor", nCS1, "inColor");
            Node.connect(nChtC2, "outColor", nCS2, "inColor");
            Node.connect(nChtC3, "outColor", nCS3, "inColor");


            (nIDCT1 as DCT).setInverse(true);
            (nIDCT1 as DCT).rename("IDCT");
            (nIDCT2 as DCT).setInverse(true);
            (nIDCT2 as DCT).rename("IDCT");
            (nIDCT3 as DCT).setInverse(true);
            (nIDCT3 as DCT).rename("IDCT");
            (nRM as ReadMulti3Channel).setPath("C:\\temp\\testVid.mdct");
            (nCS1 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCS2 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCS3 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
        }

        public void readWriteMultiTest() {
            Node nR = new ReadImage(viewNodes, 0, 0);
            Node nCtCh = new ColorToChannels(viewNodes, 150, 0);
            Node nMoVec = new MoVecDecompose(viewNodes, 300, 50);
            Node nWM = new WriteMulti3Channel(viewNodes, 500, 0);

            Node.connect(nR, "outColor", nCtCh, "inColor");
            Node.connect(nCtCh, "outChannels", nMoVec, "inChannelsPast");
            Node.connect(nCtCh, "outChannels", nMoVec, "inChannelsNow");
            Node.connect(nCtCh, "outChannels", nWM, "inChannels1");
            Node.connect(nMoVec, "outVectors", nWM, "inVectors2");
            Node.connect(nMoVec, "outChannels", nWM, "inChannels2");
            Node.connect(nMoVec, "outVectors", nWM, "inVectors3");
            Node.connect(nMoVec, "outChannels", nWM, "inChannels3");

            (nWM as WriteMulti3Channel).setPath("C:\\temp\\wrm.mdct");
            
            Node nRM = new ReadMulti3Channel(viewNodes, 700, 0);
            (nRM as ReadMulti3Channel).setPath("C:\\temp\\wrm.mdct");

        }

        public void mpegNoDCTTest() {
            //first frame
            Node nRead1 = new ReadImage(viewNodes, 0, 0);
            Node nCS1 = new ColorSpace(viewNodes, 130, 0);
            Node nCtCh1 = new ColorToChannels(viewNodes, 260, 0);
            Node nSS1 = new Subsample(viewNodes, 390, 0);

            //second frame
            Node nRead2 = new ReadImage(viewNodes, 0, 200);
            Node nCS2 = new ColorSpace(viewNodes, 130, 200);
            Node nCtCh2 = new ColorToChannels(viewNodes, 260, 200);
            Node nSS2 = new Subsample(viewNodes, 390, 200);
            Node nMoVec2 = new MoVecDecompose(viewNodes, 520, 200);
            Node nIMoVec2 = new MoVecCompose(viewNodes, 680, 360);

            //Third frame
            Node nRead3 = new ReadImage(viewNodes, 0, 500);
            Node nCS3 = new ColorSpace(viewNodes, 130, 500);
            Node nCtCh3 = new ColorToChannels(viewNodes, 260, 500);
            Node nSS3 = new Subsample(viewNodes, 390, 500);
            Node nMoVec3 = new MoVecDecompose(viewNodes, 520, 500);

            Node nWrite = new WriteMulti3Channel(viewNodes, 1000, 200);
            Node nRead = new ReadMulti3Channel(viewNodes, 1200, 200);

            Node nIMoVecR1 = new MoVecCompose(viewNodes, 1400, 200);
            Node nIMoVecR2 = new MoVecCompose(viewNodes, 1400, 360);
            Node nChtC1 = new ChannelsToColor(viewNodes, 1600, 100);
            Node nChtC2 = new ChannelsToColor(viewNodes, 1600, 200);
            Node nChtC3 = new ChannelsToColor(viewNodes, 1600, 300);
            Node nCSR1 = new ColorSpace(viewNodes, 1750, 100);
            Node nCSR2 = new ColorSpace(viewNodes, 1750, 200);
            Node nCSR3 = new ColorSpace(viewNodes, 1750, 300);

            Node nSSTest = new Subsample(viewNodes, 1400, 0);
            (nSSTest as Subsample).setOutSamples(DataBlob.Samples.s420);
            Node.connect(nRead, "outChannels2", nSSTest, "inChannels");


            Node.connect(nRead, "outChannels1", nIMoVecR1, "inChannelsPast");
            Node.connect(nRead, "outVectors2", nIMoVecR1, "inVectors");
            Node.connect(nRead, "outChannels2", nIMoVecR1, "inChannels");
            Node.connect(nIMoVecR1, "outChannels", nIMoVecR2, "inChannelsPast");
            Node.connect(nRead, "outVectors3", nIMoVecR2, "inVectors");
            Node.connect(nRead, "outChannels3", nIMoVecR2, "inChannels");
            Node.connect(nRead, "outChannels1", nChtC1, "inChannels");
            Node.connect(nIMoVecR1, "outChannels", nChtC2, "inChannels");
            Node.connect(nIMoVecR2, "outChannels", nChtC3, "inChannels");
            Node.connect(nChtC1, "outColor", nCSR1, "inColor");
            Node.connect(nChtC2, "outColor", nCSR2, "inColor");
            Node.connect(nChtC3, "outColor", nCSR3, "inColor");
            (nCSR1 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCSR2 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCSR3 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);


            Node.connect(nRead1, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCh1, "inColor");
            Node.connect(nCtCh1, "outChannels", nSS1, "inChannels");
            Node.connect(nSS1, "outChannels", nMoVec2, "inChannelsPast");
            Node.connect(nSS1, "outChannels", nIMoVec2, "inChannelsPast");

            Node.connect(nRead2, "outColor", nCS2, "inColor");
            Node.connect(nCS2, "outColor", nCtCh2, "inColor");
            Node.connect(nCtCh2, "outChannels", nSS2, "inChannels");
            Node.connect(nSS2, "outChannels", nMoVec2, "inChannelsNow");
            Node.connect(nMoVec2, "outVectors", nIMoVec2, "inVectors");
            Node.connect(nMoVec2, "outChannels", nIMoVec2, "inChannels");
            Node.connect(nIMoVec2, "outChannels", nMoVec3, "inChannelsPast");

            Node.connect(nRead3, "outColor", nCS3, "inColor");
            Node.connect(nCS3, "outColor", nCtCh3, "inColor");
            Node.connect(nCtCh3, "outChannels", nSS3, "inChannels");
            Node.connect(nSS3, "outChannels", nMoVec3, "inChannelsNow");

            Node.connect(nSS1, "outChannels", nWrite, "inChannels1");
            Node.connect(nMoVec2, "outChannels", nWrite, "inChannels2");
            Node.connect(nMoVec2, "outVectors", nWrite, "inVectors2");
            Node.connect(nMoVec3, "outChannels", nWrite, "inChannels3");
            Node.connect(nMoVec3, "outVectors", nWrite, "inVectors3");


            //(nRead1 as ReadImage).setPath("C:\\temp\\bsA.tif");
            //(nRead2 as ReadImage).setPath("C:\\temp\\bsB.tif");
            //(nRead3 as ReadImage).setPath("C:\\temp\\bsC.tif");
            (nRead1 as ReadImage).setPath("C:\\temp\\barbieA.tif");
            (nRead2 as ReadImage).setPath("C:\\temp\\barbieB.tif");
            (nRead3 as ReadImage).setPath("C:\\temp\\barbieC.tif");
            (nSS1 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS1 as Subsample).setPadded(true);
            (nSS2 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setPadded(true);
            (nSS3 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS3 as Subsample).setPadded(true);
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nCS2 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nCS3 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nWrite as WriteMulti3Channel).setPath("C:\\temp\\testVid.mdct");
            (nRead as ReadMulti3Channel).setPath("C:\\temp\\testVid.mdct");
        }

        public void nomadTest() {
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
            Node nDCT2 = new DCT(viewNodes, 680, 200); Node.connect(nRead1, "outColor", nCS1, "inColor");
            Node nWrite = new WriteMulti2Channel(viewNodes, 850, 100);
            Node.connect(nCS1, "outColor", nCtCh1, "inColor");
            Node.connect(nCtCh1, "outChannels", nSS1, "inChannels");
            Node.connect(nSS1, "outChannels", nDCT1, "inChannels");
            Node.connect(nDCT1, "outChannels", nIDCT1, "inChannels");
            Node.connect(nIDCT1, "outChannels", nMoVec2, "inChannelsPast");
            Node.connect(nRead2, "outColor", nCS2, "inColor");
            Node.connect(nCS2, "outColor", nCtCh2, "inColor");
            Node.connect(nCtCh2, "outChannels", nSS2, "inChannels");
            Node.connect(nSS2, "outChannels", nMoVec2, "inChannelsNow");
            Node.connect(nMoVec2, "outChannels", nDCT2, "inChannels");
            Node.connect(nDCT1, "outChannels", nWrite, "inChannels1");
            Node.connect(nDCT2, "outChannels", nWrite, "inChannels2");
            Node.connect(nMoVec2, "outVectors", nWrite, "inVectors2");
            
            (nRead1 as ReadImage).setPath("C:\\temp\\nomadA.jpg");
            (nRead2 as ReadImage).setPath("C:\\temp\\nomadB.jpg");
            (nIDCT1 as DCT).setInverse(true);
            (nIDCT1 as DCT).rename("IDCT");
            (nSS1 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setPadded(true);
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nCS2 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nWrite as WriteMulti2Channel).setPath("C:\\temp\\testVid.mdct");

            Node nRead = new ReadMulti2Channel(viewNodes, 1000, 100);
            Node nIDCTR1 = new DCT(viewNodes, 1200, 100);
            Node nIDCTR2 = new DCT(viewNodes, 1200, 200);
            Node nIMoVec = new MoVecCompose(viewNodes, 1350, 200);
            Node nChtCR1 = new ChannelsToColor(viewNodes, 1500, 100);
            Node nChtCR2 = new ChannelsToColor(viewNodes, 1500, 200);
            Node nCSR1 = new ColorSpace(viewNodes, 1650, 100);
            Node nCSR2 = new ColorSpace(viewNodes, 1650, 200);
            Node.connect(nRead, "outChannels1", nIDCTR1, "inChannels");
            Node.connect(nRead, "outChannels2", nIDCTR2, "inChannels");
            Node.connect(nRead, "outVectors2", nIMoVec, "inVectors");
            Node.connect(nIDCTR1, "outChannels", nIMoVec, "inChannelsPast");
            Node.connect(nIDCTR2, "outChannels", nIMoVec, "inChannels");
            Node.connect(nIDCTR1, "outChannels", nChtCR1, "inChannels");
            Node.connect(nIMoVec, "outChannels", nChtCR2, "inChannels");
            Node.connect(nChtCR1, "outColor", nCSR1, "inColor");
            Node.connect(nChtCR2, "outColor", nCSR2, "inColor");
            (nIDCTR1 as DCT).setInverse(true);
            (nIDCTR2 as DCT).setInverse(true);
            (nRead as ReadMulti2Channel).setPath("C:\\temp\\testVid.mdct");
            (nCSR1 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCSR2 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
        }

        public void nomadTest2() {
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
            Node nDCT2 = new DCT(viewNodes, 680, 200); Node.connect(nRead1, "outColor", nCS1, "inColor");
            Node.connect(nCS1, "outColor", nCtCh1, "inColor");
            Node.connect(nCtCh1, "outChannels", nSS1, "inChannels");
            Node.connect(nSS1, "outChannels", nDCT1, "inChannels");
            Node.connect(nDCT1, "outChannels", nIDCT1, "inChannels");
            Node.connect(nIDCT1, "outChannels", nMoVec2, "inChannelsPast");
            Node.connect(nRead2, "outColor", nCS2, "inColor");
            Node.connect(nCS2, "outColor", nCtCh2, "inColor");
            Node.connect(nCtCh2, "outChannels", nSS2, "inChannels");
            Node.connect(nSS2, "outChannels", nMoVec2, "inChannelsNow");
            Node.connect(nMoVec2, "outChannels", nDCT2, "inChannels");

            Node nIDCT3 = new DCT(viewNodes, 850, 200);
            Node nIMoVec2 = new MoVecCompose(viewNodes, 1000, 200);
            Node nChtC1 = new ChannelsToColor(viewNodes, 1150, 100);
            Node nChtC2 = new ChannelsToColor(viewNodes, 1150, 200);
            Node nCS3 = new ColorSpace(viewNodes, 1300, 100);
            Node nCS4 = new ColorSpace(viewNodes, 1300, 200);
            Node.connect(nDCT2, "outChannels", nIDCT3, "inChannels");
            Node.connect(nIDCT1, "outChannels", nIMoVec2, "inChannelsPast");
            Node.connect(nIDCT3, "outChannels", nIMoVec2, "inChannels");
            Node.connect(nMoVec2, "outVectors", nIMoVec2, "inVectors");
            Node.connect(nIDCT1, "outChannels", nChtC1, "inChannels");
            Node.connect(nIMoVec2, "outChannels", nChtC2, "inChannels");
            Node.connect(nChtC1, "outColor", nCS3, "inColor");
            Node.connect(nChtC2, "outColor", nCS4, "inColor");
            (nIDCT3 as DCT).setInverse(true);
            (nIDCT3 as DCT).rename("IDCT");
            (nCS3 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);
            (nCS4 as ColorSpace).setInSpace(ColorSpace.Space.YCrCb);



            //(nRead1 as ReadImage).setPath("C:\\temp\\barbieA.tif");
            //(nRead2 as ReadImage).setPath("C:\\temp\\barbieB.tif");
            //(nRead3 as ReadImage).setPath("C:\\temp\\barbieC.tif");
            (nRead1 as ReadImage).setPath("C:\\temp\\nomadA.jpg");
            (nRead2 as ReadImage).setPath("C:\\temp\\nomadB.jpg");
            (nIDCT1 as DCT).setInverse(true);
            (nIDCT1 as DCT).rename("IDCT");
            (nSS1 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setOutSamples(DataBlob.Samples.s420);
            (nSS2 as Subsample).setPadded(true);
            (nCS1 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
            (nCS2 as ColorSpace).setOutSpace(ColorSpace.Space.YCrCb);
        }

        public void buildGraph() {
            //DCTTest();
            //readWriteTest();
            //mergeTest();
            moVecTest();
            //mpegWriteTest();
            //mpegReadTest();
            //readWriteMultiTest();
            //mpegNoDCTTest();
            //nomadTest();
            //nomadTest2();
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
                }
                return true;
            } else if (keys == Keys.D2) {
                n = viewNodes.getSelection();
                if (n != null) {
                    //load right view with selected node
                    viewRight.setSource(n);
                    viewRight.Invalidate();
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
