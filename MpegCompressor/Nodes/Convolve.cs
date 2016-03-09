﻿using MpegCompressor.NodeProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor.Nodes {
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

        protected override void clean() {
            base.clean();

            if (state == null) {
                return;
            }

            BitmapData bmpData = state.bmp.LockBits(
                                new Rectangle(0, 0, state.bmp.Width, state.bmp.Height),
                                ImageLockMode.ReadWrite,
                                state.bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * state.bmp.Height;
            byte[] rgbValues = new byte[nBytes];
            byte[] resultValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            float val;
            int bands = 3;
            int pixel;
            int index;
            for (int band = 0; band < bands; band++) {
                //the non-edge pixels
                for (int y = 1; y < bmpData.Height - 1; y++) {
                    for (int x = 1; x < bmpData.Width - 1; x++) {
                        pixel = (y-1) * bmpData.Stride + (x-1) * bands; //assuming 3 channels. Sorry.

                        val = 0;
                        for(int row = 0; row < 3; row++) {
                            for (int col = 0; col < 3; col++) {
                                val += rgbValues[(pixel + band) + row * bmpData.Stride + col * bands] * kernel[row, col];
                            }
                        }

                        val = val > 255 ? 255 : val < 0 ? 0 : val;
                        resultValues[pixel + band + bands + bmpData.Stride] = (byte)val;
                    }
                }
                //top & bottom pixels
                for (int x = 1; x < bmpData.Width-1; x++) {
                    //top
                    pixel = (0 - 1) * bmpData.Stride + (x - 1) * bands; //assuming 3 channels. Sorry.
                    val = 0;
                    for (int row = 1; row < 3; row++) {
                        for (int col = 0; col < 3; col++) {
                            val += rgbValues[(pixel + band) + row * bmpData.Stride + col * bands] * kernel[row, col];
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    resultValues[pixel + band + bands + bmpData.Stride] = (byte)val;

                    //bottom
                    pixel = (bmpData.Height - 2) * bmpData.Stride + (x - 1) * bands; //assuming 3 channels. Sorry.
                    val = 0;
                    for (int row = 0; row < 2; row++) {
                        for (int col = 0; col < 3; col++) {
                            val += rgbValues[(pixel + band) + row * bmpData.Stride + col * bands] * kernel[row, col];
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    resultValues[pixel + band + bands + bmpData.Stride] = (byte)val;
                }

                //left and right pixels
                for (int y = 0; y < bmpData.Height; y++) {
                    //LEFT
                    pixel = (y - 1) * bmpData.Stride + (0 - 1) * bands; //assuming 3 channels. Sorry.
                    val = 0;
                    for (int row = 0; row < 3; row++) {
                        for (int col = 1; col < 3; col++) {
                            index = (pixel + band) + row * bmpData.Stride + col * bands;
                            if (index >= 0 && index < rgbValues.Length) {
                                val += rgbValues[index] * kernel[row, col];
                            }
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    resultValues[pixel + band + bands + bmpData.Stride] = (byte)val;

                    //RIGHT
                    pixel = (y - 1) * bmpData.Stride + (bmpData.Width - 2) * bands; //assuming 3 channels. Sorry.
                    val = 0;
                    for (int row = 0; row < 3; row++) {
                        for (int col = 0; col < 2; col++) {
                            index = (pixel + band) + row * bmpData.Stride + col * bands;
                            if (index >= 0 && index < rgbValues.Length) {
                                val += rgbValues[index] * kernel[row, col];
                            }
                        }
                    }
                    val = val > 255 ? 255 : val < 0 ? 0 : val;
                    resultValues[pixel + band + bands + bmpData.Stride] = (byte)val;
                }
            }


            System.Runtime.InteropServices.Marshal.Copy(resultValues, 0, ptr, nBytes);

            state.bmp.UnlockBits(bmpData);
        }

        private class PropertyMatrix : MpegCompressor.NodeProperties.Property {
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
                (sender as NumericUpDown).Select(0, 6);
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
        }
    }
}
