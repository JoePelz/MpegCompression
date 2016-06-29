using NodeShop.NodeProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NodeShop.Nodes {
    public class Convolve : ColorNode {
        float[,] kernel;

        public Convolve(): base() { }
        public Convolve(NodeView graph) : base(graph) { }
        public Convolve(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }
        
        protected override void init() {
            base.init();
            rename("Convolve");
            kernel = new float[,]{  
                { 0.111111f, 0.111111f, 0.111111f},
                { 0.111111f, 0.111111f, 0.111111f},
                { 0.111111f, 0.111111f, 0.111111f}
            };
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertyMatrix();
            p.eValueChanged += kernelChanged;
            properties["kernel"] = p;
        }

        private void kernelChanged(object sender, EventArgs e) {
            PropertyMatrix pm = (properties["kernel"] as PropertyMatrix);
            if (pm == null) {
                //casting failed
                return;
            }

            for (int row = 0; row < PropertyMatrix.rows; row++) {
                for (int col = 0; col < PropertyMatrix.cols; col++) {
                    kernel[row, col] = (float)pm.floaters[row, col].Value;
                }
            }

            soil();
        }

        protected override void processPixels(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            float val;
            int pixel;
            int index;
            for (int band = 0; band < xstep; band++) {
                //the non-edge pixels
                for (int y = 1; y < h - 1; y++) {
                    for (int x = 1; x < w - 1; x++) {
                        pixel = (y - 1) * ystep + (x - 1) * xstep; //assuming 3 channels. Sorry.

                        val = 0;
                        for (int row = 0; row < 3; row++) {
                            for (int col = 0; col < 3; col++) {
                                val += inValues[(pixel + band) + row * ystep + col * xstep] * kernel[row, col];
                            }
                        }

                        val = val > 255 ? 255 : val < 0 ? 0 : val;
                        outValues[pixel + band + xstep + ystep] = (byte)val;
                    }
                }
                //top & bottom pixels
                for (int x = 1; x < w - 1; x++) {
                    //top
                    pixel = (0 - 1) * ystep + (x - 1) * xstep; 
                    val = 0;
                    for (int row = 1; row < 3; row++) {
                        for (int col = 0; col < 3; col++) {
                            val += inValues[(pixel + band) + row * ystep + col * xstep] * kernel[row, col];
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    outValues[pixel + band + xstep + ystep] = (byte)val;

                    //bottom
                    pixel = (h - 2) * ystep + (x - 1) * xstep;
                    val = 0;
                    for (int row = 0; row < 2; row++) {
                        for (int col = 0; col < 3; col++) {
                            val += inValues[(pixel + band) + row * ystep + col * xstep] * kernel[row, col];
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    outValues[pixel + band + xstep + ystep] = (byte)val;
                }

                //left and right pixels
                for (int y = 0; y < h; y++) {
                    //LEFT
                    pixel = (y - 1) * ystep + (0 - 1) * xstep; //assuming 3 channels. Sorry.
                    val = 0;
                    for (int row = 0; row < 3; row++) {
                        for (int col = 1; col < 3; col++) {
                            index = (pixel + band) + row * ystep + col * xstep;
                            if (index >= 0 && index < inValues.Length) {
                                val += inValues[index] * kernel[row, col];
                            }
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    outValues[pixel + band + xstep + ystep] = (byte)val;

                    //RIGHT
                    pixel = (y - 1) * ystep + (w - 2) * xstep; //assuming 3 channels. Sorry.
                    val = 0;
                    for (int row = 0; row < 3; row++) {
                        for (int col = 0; col < 2; col++) {
                            index = (pixel + band) + row * ystep + col * xstep;
                            if (index >= 0 && index < inValues.Length) {
                                val += inValues[index] * kernel[row, col];
                            }
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    outValues[pixel + band + xstep + ystep] = (byte)val;
                }
            }
        }

        private class PropertyMatrix : NodeShop.NodeProperties.Property {
            public NumericUpDown[,] floaters;
            public const int rows = 3;
            public const int cols = 3;

            public PropertyMatrix() : base(false, false, NodeProperties.Type.CUSTOM) {
                sLabel = "Convolution Kernel";
                floaters = new NumericUpDown[rows, cols];
                for (int row = 0; row < rows; row++) {
                    for (int col = 0; col < cols; col++) {
                        floaters[row, col] = new NumericUpDown();
                        ((System.ComponentModel.ISupportInitialize)(floaters[row, col])).BeginInit();
                        floaters[row, col].Size = new System.Drawing.Size(50, 20);
                        floaters[row, col].Increment = (decimal)0.01;
                        floaters[row, col].DecimalPlaces = 3;
                        floaters[row, col].Name = "fUpDown" + row + col;
                        floaters[row, col].Value = (decimal)0.111111;
                        floaters[row, col].ValueChanged += fireEvent;
                        floaters[row, col].Maximum = 100;
                        floaters[row, col].Minimum = -100;
                        floaters[row, col].GotFocus += onElementFocus;
                    }
                }
                updateLayout();
            }

            private void onElementFocus(object sender, EventArgs e) {
                (sender as NumericUpDown).Select(0, 6); //select everything, really. 
            }

            private void updateLayout() {
                SuspendLayout();

                resetLayout();

                TableLayoutPanel layout = new TableLayoutPanel();

                layout.ColumnCount = 3;
                layout.RowCount = 3;
                layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33));
                layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34));
                layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33));
                layout.RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33));
                layout.RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34));
                layout.RowStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33));

                for (int col = 0; col < cols; col++) {
                    for (int row = 0; row < rows; row++) {
                        layout.Controls.Add(floaters[row, col]);
                    }
                }

                //layout.Size = new System.Drawing.Size(180, 80);
                layout.BackColor = System.Drawing.Color.Azure;
                layout.ForeColor = System.Drawing.Color.RosyBrown;
                layout.Dock = DockStyle.Fill;

                Controls.Add(layout, 0, 0);

                ResumeLayout();

                for (int col = 0; col < cols; col++) {
                    for (int row = 0; row < rows; row++) {
                        ((System.ComponentModel.ISupportInitialize)(floaters[row, col])).EndInit();
                    }
                }


            }

            public override string ToString() {
                StringBuilder sb = new StringBuilder(rows * cols * 2);
                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                        sb.Append(((float)floaters[row, col].Value).ToString() + ",");
                sb.Length = sb.Length - 1;
                return sb.ToString();
            }

            public override void FromString(string data) {
                string[] entries = data.Split(',');
                for (int row = 0; row < rows; row++)
                    for (int col = 0; col < cols; col++)
                        floaters[row, col].Value = decimal.Parse(entries[row * rows + col]);

            }
        }
    }
}
