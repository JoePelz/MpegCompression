﻿using System;
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
        }

        public void buildGraph() {
            readWriteTest();
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
