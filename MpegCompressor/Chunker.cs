using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class Chunker {
        private int chunkSize;
        private int width, height;
        private int stride;
        private int Bpp;
        private int chunksWide;
        private int chunksHigh;
        private byte[] buffer = new byte[64];

        public Chunker(int chunkSize, int width, int height, int stride, int Bpp) {
            this.chunkSize = chunkSize;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.Bpp = Bpp;
            chunksWide = (width + chunkSize - 1) / chunkSize;
            chunksHigh = (height + chunkSize - 1) / chunkSize;
        }

        public int getNumChunks() {
            return chunksHigh * chunksWide;
        }

        public System.Collections.IEnumerable getIterators() {
            for (int i = 0; i < getNumChunks(); i++) {
                yield return getChunk(i);
            }
        }

        private System.Collections.IEnumerable getChunk(int chunkIndex) {
            //assert index = [0..getNumChunks)
            if (chunkIndex >= getNumChunks()) {
                yield break;
            }

            int px = (chunkIndex % chunksWide) * chunkSize;
            int py = (chunkIndex / chunksWide) * chunkSize;
            int output = px * Bpp + py * stride;
            //output is index of the pixel at the top left of the desired chunk.
            for (int y = 0; y < chunkSize; y++) {
                for (int x = 0; x < chunkSize; x++) {
                    if (px + x >= width || py + y >= height) {
                        //out of bounds. What do we do?
                        yield return -1;
                    } else {
                        yield return output;
                    }
                    output += Bpp; //iterate one pixel to the right
                }
                output -= chunkSize * Bpp; //carriage return
                output += stride; //line feed
            }
        }

        public int chunkIndexToPixelIndex(int chunkIndex) {
            int px = (chunkIndex % chunksWide) * chunkSize;
            int py = (chunkIndex / chunksWide) * chunkSize;
            return px * Bpp + py * stride;
        }

        public void getBlock(byte[] channel, byte[] result, int iChunk) {
            int iDest = 0;
            foreach (int pixel in getChunk(iChunk)) {
                for (int c = 0; c < Bpp; c++) {
                    result[iDest++] = (pixel == -1 ? (byte)0 : channel[pixel + c]);
                }
            }
        }

        public void setBlock(byte[] channel, byte[] newBytes, int iChunk) {
            int iDest = 0;
            foreach (int pixel in getChunk(iChunk)) {
                if (pixel != -1) {
                    for (int c = 0; c < Bpp; c++) {
                        channel[pixel + c] = newBytes[iDest++];
                    }
                }
            }
        }

        public void getZigZag8Block(byte[] channel, byte[] result, int iChunk) {
            getBlock(channel, buffer, iChunk);
            int iDest = 0;
            foreach (int zig in zigZag8Index()) {
                result[iDest++] = buffer[zig];
            }
        }

        public void setZigZag8Block(byte[] channel, byte[] newBytes, int iChunk) {
            int iDest = 0;
            foreach (int zig in zigZag8Index()) {
                buffer[zig] = newBytes[iDest++];
            }
            setBlock(channel, buffer, iChunk);
        }

        public static System.Collections.Generic.IEnumerable<int> zigZag8Index() {
            //yield order:
            /*
              0  1  8 16  9  2  3 10
             17 24 32 25 18 11  4  5
             12 19 26 33 40 48 41 34
             27 20 13  6  7 14 21 28
             35 42 49 56 57 50 43 36
             29 22 15 23 30 37 44 51
             58 59 52 45 38 31 39 46
             53 60 61 54 47 55 62 63

            from
              0  1  2  3  4  5  6  7
              8  9 10 11 12 13 14 15
             16 17 18 19 20 21 22 23
             24 25 26 27 28 29 30 31
             32 33 34 35 36 37 38 39
             40 41 42 43 44 45 46 47
             48 49 50 51 52 53 54 55
             56 57 58 59 60 61 62 63
            */

            byte[] indices = {
                  0,  1,  8, 16,  9,  2,  3, 10,
                 17, 24, 32, 25, 18, 11,  4,  5,
                 12, 19, 26, 33, 40, 48, 41, 34,
                 27, 20, 13,  6,  7, 14, 21, 28,
                 35, 42, 49, 56, 57, 50, 43, 36,
                 29, 22, 15, 23, 30, 37, 44, 51,
                 58, 59, 52, 45, 38, 31, 39, 46,
                 53, 60, 61, 54, 47, 55, 62, 63
            };
            foreach (int i in indices) {
                yield return i;
            }
            yield break;
        }
    }
}
