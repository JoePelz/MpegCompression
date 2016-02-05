﻿using System;
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
        }

        public void buildGraph() {
            Node n1 = new ReadImage();
            Node n2 = new ColorSpace();
            Node n3 = new ColorSpace();
            Node n4 = new Subsample();
            Node n5 = new Subsample();
            n5.rename("bogus");

            Node.connect(n1, "outColor", n2, "inColor");
            Node.connect(n2, "outColor", n3, "inColor");
            Node.connect(n2, "outColor", n4, "inChannels");
            Node.connect(n4, "outChannels", n5, "inChannels");
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
