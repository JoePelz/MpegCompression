using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.NodeProperties;

namespace MpegCompressor.Nodes {
    class DCT : ChannelNode {
        private const int chunkSize = 8;
        private bool isInverse;
        /// <summary>
        /// Luminance quantization table. 
        /// Table 9.2, pg 284, Fundamentals of Multimedia textbook.
        /// </summary>
        private static byte[,] quantizationY_orig = 
            {
            {16, 11, 10, 16, 24, 40, 51, 61},
            {12, 12, 14, 19, 26, 58, 60, 55},
            {14, 13, 16, 24, 40, 57, 69, 56},
            {14, 17, 22, 29, 51, 87, 80, 62},
            {18, 22, 37, 56, 68, 109, 103, 77},
            {24, 35, 55, 64, 81, 104, 113, 92},
            {49, 64, 78, 87, 103, 121, 120, 101},
            {72, 92, 95, 98, 112, 100, 103, 99}
            };
        /// <summary>
        /// Chrominance quantization table. 
        /// Table 9.2, pg 284, Fundamentals of Multimedia textbook.
        /// </summary>
        private static byte[,] quantizationC_orig =
            {
            {17, 18, 24, 47, 99, 99, 99, 99},
            {18, 21, 26, 66, 99, 99, 99, 99},
            {24, 26, 56, 99, 99, 99, 99, 99},
            {47, 66, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99},
            {99, 99, 99, 99, 99, 99, 99, 99}
            };

        private byte[,] quantizationY;
        private byte[,] quantizationC;

        public DCT(): base() { }
        public DCT(NodeView graph) : base(graph) { }
        public DCT(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("DCT");
            quantizationC = new byte[8, 8];
            quantizationY = new byte[8, 8];
            generateQTables(50);
        }

        private void generateQTables(int qf) {
            double scaling_factor;
            // qf is the user-selected compression quality
            // qf == [1..100] (but should not be less than 10)
            // Q is the default Quantization Matrix
            // Qx is the scaled Quantization Matrix
            // Q1 is a Quantization Matrix which is all 1’s
            if (qf >= 50) {
                scaling_factor = (100.0 - qf) / 50.0;
            }else {
                scaling_factor = (50.0 / qf);
            }
            if (scaling_factor != 0) { // if qf is not 100
                for (int y = 0; y < 8; y++) {
                    for (int x = 0; x < 8; x++) {
                        quantizationC[x, y] = (byte)Math.Min(Math.Round(quantizationC_orig[x, y] * scaling_factor), 255);
                        quantizationY[x, y] = (byte)Math.Min(Math.Round(quantizationY_orig[x, y] * scaling_factor), 255);
                    }
                }
            } else {
                for (int y = 0; y < 8; y++) {
                    for (int x = 0; x < 8; x++) {
                        quantizationC[x, y] = 1; // no quantization
                        quantizationY[x, y] = 1; // no quantization
                    }
                }
            }
            state.quantizeQuality = qf;
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertyCheckbox("Inverse");
            p.eValueChanged += P_eValueChanged; ;
            properties["isInverse"] = p;

            p = new PropertyInt(50, 10, 100, "Quantization quality (%)");
            p.eValueChanged += (prop, b) => { soil(); };
            properties["quality"] = p;
        }

        public void setInverse(bool b) {
            isInverse = b;
            properties["isInverse"].bValue = b;
            setExtra(isInverse ? "(Inverse)" : "");
            soil();
        }

        public bool getInverse() {
            return isInverse;
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            setInverse((bool)properties["isInverse"].bValue);
        }

        protected override void clean() {
            base.clean();
            if (state == null || state.channels == null) {
                return;
            }

            if (!isInverse) {
                state.quantizeQuality = properties["quality"].nValue;
            }
            generateQTables(state.quantizeQuality);
            
            padChannels();
            Chunker c = new Chunker(chunkSize, state.channelWidth, state.channelHeight, state.channelWidth, 1);
            
            byte[] data = new byte[chunkSize * chunkSize];
            for (int i = 0; i < c.getNumChunks(); i++) {
                c.getBlock(state.channels[0], data, i);
                data = isInverse ? doIDCT(data, quantizationY) : doDCT(data, quantizationY);
                c.setBlock(state.channels[0], data, i);
            }
            
            Size tempS = Subsample.getPaddedCbCrSize(new Size(state.channelWidth, state.channelHeight), state.samplingMode);
            c = new Chunker(chunkSize, tempS.Width, tempS.Height, tempS.Width, 1);
            for (int i = 0; i < c.getNumChunks(); i++) {
                c.getBlock(state.channels[1], data, i);
                data = isInverse ? doIDCT(data, quantizationC) : doDCT(data, quantizationC);
                c.setBlock(state.channels[1], data, i);
                c.getBlock(state.channels[2], data, i);
                data = isInverse ? doIDCT(data, quantizationC) : doDCT(data, quantizationC);
                c.setBlock(state.channels[2], data, i);
            }
        }

        private byte[] doIDCT(byte[] data, byte[,] qTable) {
            byte[] result = new byte[data.Length];
            double bin;

            for (int j = 0; j < chunkSize; j++) {
                for (int i = 0; i < chunkSize; i++) {
                    bin = 0;
                    for (int v = 0; v < chunkSize; v++) {
                        for (int u = 0; u < chunkSize; u++) {
                            bin += (
                                (u == 0 ? 1.0 / Math.Sqrt(2) : 1.0) * (v == 0 ? 1 / Math.Sqrt(2) : 1) / (chunkSize / 2)
                                * Math.Cos(((2 * i + 1) * u * Math.PI) / (2 * chunkSize))
                                * Math.Cos(((2 * j + 1) * v * Math.PI) / (2 * chunkSize))
                                * (sbyte)data[v * chunkSize + u] * qTable[v, u]
                                );
                        }
                    }
                    bin += 128;
                    if (bin > 255)
                        bin = 255;
                    if (bin < 0)
                        bin = 0;
                    result[j * chunkSize + i] = (byte)bin;
                }
            }
            return result;
        }

        private byte[] doDCT(byte[] data, byte[,] qTable) {
            byte[] result = new byte[data.Length];
            double bin;
            //DCT the values, and quantize
            for (int v = 0; v < chunkSize; v++) {
                for (int u = 0; u < chunkSize; u++) {
                    bin = 0;
                    for (int j = 0; j < chunkSize; j++) {
                        for (int i = 0; i < chunkSize; i++) {
                            bin += (float) (Math.Cos(((2 * i + 1) * u * Math.PI) / (2 * chunkSize))
                                 * Math.Cos(((2 * j + 1) * v * Math.PI) / (2 * chunkSize))
                                 * (data[j*chunkSize + i] - 128));
                        }
                    }
                    bin *= ((u == 0 ? 1.0 / Math.Sqrt(2) : 1.0) * (v == 0 ? 1.0 / Math.Sqrt(2) : 1.0)) / (chunkSize/2);
                    //Quantize
                    result[v * chunkSize + u] = (byte)Math.Round(bin / qTable[v, u]);
                }
            }
            return result;
        }

        private void padChannels() {
            //pad the size
            Size ySize = new Size(state.channelWidth, state.channelHeight);
            Size brOldSize = Subsample.deduceCbCrSize(state);
            Size brNewSize = Subsample.getPaddedCbCrSize(ySize, state.samplingMode);
            if (ySize.Width % 8 != 0) {
                ySize.Width += 8 - (ySize.Width % 8);
            }
            if (ySize.Height % 8 != 0) {
                ySize.Height += 8 - (ySize.Height % 8);
            }

            //create padded container
            byte[][] newChannels = new byte[3][];
            newChannels[0] = new byte[ySize.Width * ySize.Height];
            newChannels[1] = new byte[brNewSize.Width * brNewSize.Height];
            newChannels[2] = new byte[newChannels[1].Length];

            //copy array into larger container
            for (int y = 0; y < state.channelHeight; y++) {
                Array.Copy(state.channels[0], y * state.channelWidth, newChannels[0], y * ySize.Width, state.channelWidth);
            }
            for (int y = 0; y < brOldSize.Height; y++) {
                Array.Copy(state.channels[1], y * brOldSize.Width, newChannels[1], y * brNewSize.Width, brOldSize.Width);
                Array.Copy(state.channels[2], y * brOldSize.Width, newChannels[2], y * brNewSize.Width, brOldSize.Width);
                for (int x = brOldSize.Width; x < brNewSize.Width; x++) {
                    newChannels[1][y * brNewSize.Width + x] = 127;
                    newChannels[2][y * brNewSize.Width + x] = 127;
                }
            }

            //update state
            state.channelWidth = ySize.Width;
            state.channelHeight = ySize.Height;
            state.channels = newChannels;
        }
    }
}
