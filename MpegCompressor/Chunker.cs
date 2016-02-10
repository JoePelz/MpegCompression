using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class Chunker {
        private int size;
        private int width, height;
        private int stride;
        private int Bpp;
        private int chunksWide;
        private int chunksHigh;

        public Chunker(int size, int width, int height, int stride, int Bpp) {
            this.size = size;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.Bpp = Bpp;
            chunksWide = (width + size - 1) / size;
            chunksHigh = (height + size - 1) / size;
        }

        public int getNumChunks() {
            return chunksHigh * chunksWide;
        }

        public System.Collections.IEnumerable getIterators() {
            for (int i = 0; i < getNumChunks(); i++) {
                yield return getChunk(i);
            }
        }

        private System.Collections.IEnumerable getChunk(int index) {
            //assert index = [0..getNumChunks)
            if (index >= getNumChunks()) {
                yield break;
            }

            int px = (index % chunksWide) * size;
            int py = (index / chunksWide) * size;
            int output = px * Bpp + py * stride;
            //output is index of the pixel at the top left of the desired chunk.
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    if (px + x >= width || py + y >= height) {
                        //out of bounds. What do we do?
                        yield return -1;
                    } else {
                        yield return output;
                    }
                    output += Bpp; //iterate one pixel to the right
                }
                output -= size * Bpp; //carriage return
                output += stride; //line feed
            }
        }

        public byte[] getBlock(byte[] channel, int iChunk) {
            byte[] result = new byte[size * size * Bpp];
            int iDest = 0;
            foreach (int pixel in getChunk(iChunk)) {
                for (int c = 0; c < Bpp; c++) {
                    result[iDest++] = pixel == -1 ? (byte)0 : channel[pixel + c];
                }
            }
            return result;
        }

        public void setBlock(byte[] channel, int iChunk, byte[] newBytes) {
            int iDest = 0;
            foreach (int pixel in getChunk(iChunk)) {
                if (pixel != -1) {
                    for (int c = 0; c < Bpp; c++) {
                        channel[pixel + c] = newBytes[iDest++];
                    }
                }
            }
        }
    }
}
