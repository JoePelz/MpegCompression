using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class TestChunker : Node {
        int chunkSize;

        public TestChunker() {
            rename("Chunk Tester!");
            setChunkSize(4);
        }

        protected override void createInputs() {
            inputs.Add("inColor", null);
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
        }

        protected override void createProperties() {
            Property p = new Property();
            p.createInt(4, 2, 16, "Chunk size to use");
            p.eValueChanged += P_eValueChanged;
            properties["chunkSize"] = p;
        }

        public void setChunkSize(int size) {
            chunkSize = Math.Min(Math.Max(2, size), 16);
            properties["chunkSize"].setInt(chunkSize);
            setExtra("Chunk Size: " + chunkSize);
            soil();
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            setChunkSize(properties["chunkSize"].getInt());
        }

        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Image;
            d.img = bmp;
            return d;
        }

        protected override void clean() {
            base.clean();
            Address upstream = inputs["inColor"];
            if (upstream == null) {
                return;
            }
            DataBlob dataIn = upstream.node.getData(upstream.port);
            if (dataIn == null) {
                return;
            }

            if (dataIn.type == DataBlob.Type.Image && dataIn.img != null) {
                bmp = dataIn.img.Clone(new Rectangle(0, 0, dataIn.img.Width, dataIn.img.Height), dataIn.img.PixelFormat);
            } else {
                bmp = null;
                return;
            }

            drawChunks();
        }

        private void drawChunks() {
            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);


            //Using the iterator method
            /*
            Chunker c = new Chunker(chunkSize, bmpData.Width, bmpData.Height, bmpData.Stride, 3);
            int pixel;
            int nPixels = chunkSize * chunkSize;
            foreach (System.Collections.IEnumerable iterator in c.getIterators()) {
                pixel = 0;
                foreach (int index in iterator) {
                    if (index != -1) {
                        rgbValues[index] = (byte)(pixel * 256 / nPixels);
                        rgbValues[index + 1] = (byte)(pixel * 256 / nPixels);
                        rgbValues[index + 2] = (byte)(pixel * 256 / nPixels);
                    }
                    pixel++;
                }
            }
            */

            //Using the chunk pull/push method
            Chunker c = new Chunker(chunkSize, bmpData.Width, bmpData.Height, bmpData.Stride, 3);
            byte[] data;
            for(int i = 0; i < c.getNumChunks(); i++) {
                data = c.getBlock(rgbValues, i);
                for(int j = 0; j < data.Length; j++) {
                    data[j] = (byte)(j * 255 / (data.Length - 1));
                }
                c.setBlock(rgbValues, i, data);
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }
    }
}
