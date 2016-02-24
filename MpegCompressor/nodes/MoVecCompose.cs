using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class MoVecCompose : Node {
        private const int chunkSize = 8;

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

            properties.Add("inVectors", new Property(true, false));
            properties.Add("inChannels", new Property(true, false));
            properties.Add("inChannelsPast", new Property(true, false));
            properties.Add("outChannels", new Property(false, true));
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

            DataBlob stateVectors = upstreamVectors.node.getData(upstreamVectors.port);
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
                
                restoreChunk(state.channels[1], past[1], diff[1], vectors[1][i], pixelTL, state.channelWidth);
                restoreChunk(state.channels[2], past[2], diff[2], vectors[2][i], pixelTL, state.channelWidth);
            }
        }

        private void restoreChunk(byte[] dest, byte[] past, byte[] diff, byte vector, int pixelTL, int channelWidth) {
            int offX = ((vector & 0xf0) >> 4) - 7;
            int offY = (vector & 0x0f) - 7;

            int srcPixel;
            int dstPixel;
            int initialX = pixelTL % channelWidth;
            int initialY = pixelTL / channelWidth;
            int maxY = dest.Length / channelWidth;
            for (int y = 0; y < chunkSize; y++) {
                if (initialY + y >= maxY) {
                    break;
                }
                for (int x = 0; x < chunkSize; x++) {
                    dstPixel = pixelTL + y * channelWidth + x;
                    srcPixel = dstPixel + offY * channelWidth + offX;
                    if (initialX + x >= channelWidth) {
                        break;
                    }

                    if (initialY + y + offY >= maxY || 
                        initialX + x + offX >= channelWidth ||
                        initialY + y + offY < 0 ||
                        initialX + x + offX < 0) {
                        dest[dstPixel] = (byte)(diff[dstPixel] - 127);
                    } else {
                        dest[dstPixel] = (byte)(past[srcPixel] + diff[dstPixel] - 127);
                    }
                }
            }

        }
        
    }
}
