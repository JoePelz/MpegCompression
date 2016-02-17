using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class TestChunker : ColorNode {
        int chunkSize;

        public TestChunker() {
            rename("Chunk Tester!");
            setChunkSize(4);
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

        protected override void clean() {
            base.clean();

            if (state == null || state.bmp == null) {
                return;
            }

            drawChunks();
        }

        private void drawChunks() {
            BitmapData bmpData = state.bmp.LockBits(
                                new Rectangle(0, 0, state.bmp.Width, state.bmp.Height),
                                ImageLockMode.ReadWrite,
                                state.bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmpData.Height;
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
            byte[] data = new byte[chunkSize * chunkSize * 3];
            for(int i = 0; i < c.getNumChunks(); i++) {
                c.getBlock(rgbValues, data, i);
                for(int j = 0; j < data.Length; j++) {
                    data[j] = (byte)(j * 255 / (data.Length - 1));
                }
                c.setBlock(rgbValues, data, i);
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            state.bmp.UnlockBits(bmpData);
        }
    }
}
