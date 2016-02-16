using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class DCT : ChannelNode {
        private const int chunkSize = 8;
        private bool isInverse;
        /// <summary>
        /// Luminance quantization table. 
        /// Table 9.2, pg 284, Fundamentals of Multimedia textbook.
        /// </summary>
        private static byte[,] quantizationY = 
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
        private static byte[,] quantizationC =
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

        public DCT() {
            rename("DCT");
        }

        protected override void createProperties() {
            Property p = new Property();
            p.createCheckbox("Inverse");
            p.eValueChanged += P_eValueChanged; ;
            properties["isInverse"] = p;
        }

        public void setInverse(bool b) {
            isInverse = b;
            properties["isInverse"].setChecked(b);
            setExtra(isInverse ? "(Inverse)" : "");
            soil();
        }

        public bool getInverse() {
            return isInverse;
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            setInverse(properties["isInverse"].getChecked());
        }

        protected override void clean() {
            base.clean();
            if (channels == null) {
                return;
            }
            /*
            byte[] testData =
                {
                200, 202, 189, 188, 189, 175, 175, 175,
                200, 203, 198, 188, 189, 182, 178, 175,
                203, 200, 200, 195, 200, 187, 185, 175,
                200, 200, 200, 200, 197, 187, 187, 187,
                200, 205, 200, 200, 195, 188, 187, 175,
                200, 200, 200, 200, 200, 190, 187, 175,
                205, 200, 199, 200, 191, 187, 187, 175,
                210, 200, 200, 200, 188, 185, 187, 186
                };
            byte[] DCT_testData = doDCT(testData, quantizationY);
            byte[] IDCT_testData = doIDCT(DCT_testData, quantizationY);
            */
            /*
            byte[] testData =
                {
                70, 70, 100, 70, 87, 87, 150, 187,
                85, 100, 96, 79, 87, 154, 87, 113,
                100, 85, 116, 79, 70, 87, 86, 196,
                136, 69, 87, 200, 79, 71, 117, 96,
                161, 70, 87, 200, 103, 71, 96, 113,
                161, 123, 147, 133, 113, 113, 85, 161,
                146, 147, 175, 100, 103, 103, 163, 187,
                156, 146, 189, 70, 113, 161, 163, 197
                };
            byte[] DCT_testData = doDCT(testData, quantizationY);
            byte[] IDCT_testData = doIDCT(DCT_testData, quantizationY);
            */

            
            byte[] testData =
                {
                128, 128, 128, 128, 128, 128, 128, 128,
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0
                };
            byte[] DCT_testData = doDCT(testData, quantizationC);
            byte[] IDCT_testData = doIDCT(DCT_testData, quantizationC);
            byte[] DCT_testData2 = doDCT(testData, quantizationC);
            for(int i = 8; i < 64; i++) { DCT_testData2[i] = 0; }
            byte[] IDCT_testData2 = doIDCT(DCT_testData2, quantizationC);





            Chunker c = new Chunker(chunkSize, width, height, width, 1);
            byte[] data = new byte[chunkSize * chunkSize];
            for (int i = 0; i < c.getNumChunks(); i++) {
                c.getBlock(channels[0], data, i);
                data = isInverse ? doIDCT(data, quantizationY) : doDCT(data, quantizationY);
                c.setBlock(channels[0], data, i);
            }

            //with 4:2:0 the width of the Cr/b channel is half that of the Y channel, rounded up
            c = new Chunker(chunkSize, (width+1) / 2, (height+1) / 2, (width+1) / 2, 1);
            for (int i = 0; i < c.getNumChunks(); i++) {
                c.getBlock(channels[1], data, i);
                data = isInverse ? doIDCT(data, quantizationC) : doDCT(data, quantizationC);
                c.setBlock(channels[1], data, i);
                c.getBlock(channels[2], data, i);
                data = isInverse ? doIDCT(data, quantizationC) : doDCT(data, quantizationC);
                c.setBlock(channels[2], data, i);
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
                                * (data[v * chunkSize + u] > 127 ? data[v * chunkSize + u] - 256 : data[v * chunkSize + u]) * qTable[v, u]
                                );
                        }
                    }
                    bin += 128;
                    if (bin > 255) bin = 255;
                    if (bin < 0) bin = 0;
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
    }
}
