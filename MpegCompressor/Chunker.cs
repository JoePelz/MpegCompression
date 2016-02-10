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
            for(int i = 0; i < getNumChunks(); i++) {
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
            yield return 2;
        }
    }
}
