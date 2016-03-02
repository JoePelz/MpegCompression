using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.NodeProperties;

namespace MpegCompressor.Nodes {
    public class MoVecCompose : Node {
        private int[] testData = new int[256];
        private const int chunkSize = 8;
        DataBlob stateVectors;

        public MoVecCompose(): base() { }
        public MoVecCompose(NodeView graph) : base(graph) { }
        public MoVecCompose(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("Rebuild Frame");
            setExtra("from vectors");
        }

        protected override void createProperties() {
            base.createProperties();

            properties.Add("inChannelsPast", new PropertyChannels(true, false));
            properties.Add("inChannels", new PropertyChannels(true, false));
            properties.Add("inVectors", new PropertyVectors(true, false));
            properties.Add("outChannels", new PropertyChannels(false, true));
        }

        protected override void clean() {
            base.clean();
            state = null;

            Address upstreamDiff = properties["inChannels"].input;
            if (upstreamDiff == null) {
                return;
            }
            Address upstreamPast = properties["inChannelsPast"].input;
            if (upstreamPast == null) {
                return;
            }
            Address upstreamVectors = properties["inVectors"].input;
            if (upstreamVectors == null) {
                return;
            }


            DataBlob stateDiff = upstreamDiff.node.getData(upstreamDiff.port);
            if (stateDiff == null) {
                return;
            }
            DataBlob statePast = upstreamPast.node.getData(upstreamPast.port);
            if (statePast == null) {
                return;
            }

            stateVectors = upstreamVectors.node.getData(upstreamVectors.port);
            if (stateVectors == null) {
                return;
            }


            if (stateDiff.type != DataBlob.Type.Channels || stateDiff.channels == null) 
                return;
            if (statePast.type != DataBlob.Type.Channels || statePast.channels == null) 
                return;
            if (stateVectors.type != DataBlob.Type.Vectors || stateVectors.channels == null)
                return;

            state = stateDiff.clone();
            byte[][] newChannels = new byte[state.channels.Length][];
            newChannels[0] = new byte[state.channels[0].Length];
            newChannels[1] = new byte[state.channels[1].Length];
            newChannels[2] = new byte[state.channels[2].Length];
            state.channels = newChannels;
            state.bmp = null;

            reassemble(statePast.channels, stateDiff.channels, stateVectors.channels);
        }

        private void reassemble(byte[][] past, byte[][] diff, byte[][] vectors) {

            Chunker c = new Chunker(chunkSize, state.channelWidth, state.channelHeight, state.channelWidth, 1);
            int pixelTL;
            for (int i = 0; i < c.getNumChunks(); i++) {
                pixelTL = c.chunkIndexToPixelIndex(i);

                //update channels to be difference.
                restoreChunk(state.channels[0], past[0], diff[0], vectors[0][i], pixelTL, state.channelWidth);
            }

            //Do the second two channels
            Size smaller = Subsample.deduceCbCrSize(state);
            c = new Chunker(chunkSize, smaller.Width, smaller.Height, smaller.Width, 1);
            for (int i = 0; i < c.getNumChunks(); i++) {
                pixelTL = c.chunkIndexToPixelIndex(i);
                
                restoreChunk(state.channels[1], past[1], diff[1], vectors[1][i], pixelTL, smaller.Width);
                restoreChunk(state.channels[2], past[2], diff[2], vectors[2][i], pixelTL, smaller.Width);
            }
        }

        private void restoreChunk(byte[] dest, byte[] past, byte[] diff, byte offset, int indexTopLeft, int stride) {
            int offsetX = ((offset & 0xf0) >> 4) - 7;
            int offsetY = (offset & 0x0f) - 7;
            int x0 = indexTopLeft % stride;
            int y0 = indexTopLeft / stride;
            int xref, yref;
            int yMax = dest.Length / stride;
            int xMax = stride;
            int targetPixel, refPixel;
            int temp;
            for (int y = y0; y < y0 + 8; y++) {
                if (y >= yMax) break;
                yref = y + offsetY;
                for (int x = x0; x < x0 + 8; x++) {
                    if (x >= xMax) break;
                    xref = x + offsetX;
                    targetPixel = y * stride + x;
                    refPixel = yref * stride + xref;
                    if (xref < 0 || xref >= xMax || yref < 0 || yref >= yMax) {
                        temp = (diff[targetPixel] - 127);
                    } else {
                        temp = diff[targetPixel] + past[refPixel] - 127;
                    }
                    dest[targetPixel] = (byte)(temp > 255 ? 255 : (temp < 0 ? 0 : temp));
                    testData[dest[targetPixel]] += 1;
                }
            }
        }

        public override void viewExtra(Graphics g) {
            //base.viewExtra(g);
            if (state == null) {
                return;
            }
            Chunker c = new Chunker(8, state.channelWidth, state.channelHeight, state.channelWidth, 1);
            int offsetX, offsetY;
            int y = state.channelHeight - 4;
            int x = 4;

            for (int i = 0; i < stateVectors.channels[0].Length; i++) {
                offsetX = ((stateVectors.channels[0][i] & 0xF0) >> 4) - 7;
                offsetY = (stateVectors.channels[0][i] & 0x0F) - 7;
                if (offsetX == 0 && offsetY == 0) {
                    g.FillRectangle(Brushes.BlanchedAlmond, x - 1, y - 1, 2, 2);
                } else {
                    g.DrawLine(Pens.BlanchedAlmond, x, y, x + offsetX, y - offsetY);
                }
                x += 8;
                if (x - 4 >= state.channelWidth) {
                    x = 4;
                    y -= 8;
                }
            }
        }
    }
}
