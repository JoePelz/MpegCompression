using MpegCompressor.NodeProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.Nodes {
    public class Convolve : ColorNode {
        double[,] kernel;

        public Convolve(): base() { }
        public Convolve(NodeView graph) : base(graph) { }
        public Convolve(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }
        
        protected override void init() {
            base.init();
            rename("Convolve");
            kernel = new double[,]{  
                { 0.111111, 0.111111, 0.111111},
                { 0.111111, 0.111111, 0.111111},
                { 0.111111, 0.111111, 0.111111}
            };
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertyMatrix();
            p.eValueChanged += kernelChanged;
        }

        private void kernelChanged(object sender, EventArgs e) {
            soil();
        }

        private class PropertyMatrix : MpegCompressor.NodeProperties.Property {
            private NumericUpDown[,] floaters;

            public PropertyMatrix() : base(false, false, NodeProperties.Type.CUSTOM) {
                sLabel = "Convolution Kernel";
                int rows = 3, cols = 3;
                floaters = new NumericUpDown[rows, cols];
                for (int row = 0; row < rows; row++) {
                    for (int col = 0; col < cols; col++) {
                        floaters[row, col] = new NumericUpDown();
                        floaters[row, col].Size = new System.Drawing.Size(120, 20);
                        floaters[row, col].Increment = (decimal)0.01;
                        floaters[row, col].DecimalPlaces = 3;
                        floaters[row, col].Name = "fUpDown" + row + col;
                        floaters[row, col].Value = (decimal)0.111111;
                        floaters[row, col].ValueChanged += fireEvent;
                        floaters[row, col].Maximum = 100;
                        floaters[row, col].Minimum = -100;
                    }
                }
            }
        }
    }
}
