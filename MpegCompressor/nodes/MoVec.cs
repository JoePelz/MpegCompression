using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class MoVec : Node {
        private const int searchRange = 15;
        private const int chunkSize = 8;
        private byte[] vectors;

        public MoVec() {
            rename("Motion Vectors");
        }

        protected override void createProperties() {
            base.createProperties();

            properties.Add("inChannelNow",  new Property(true, false));
            properties.Add("inChannelPast", new Property(true, false));
            properties.Add("outVectors",    new Property(false, true));
            properties.Add("outChannels",   new Property(false, true));
        }

        protected override void clean() {
            base.clean();

            Address upstreamNow = properties["inChannelNow"].input;
            if (upstreamNow == null) {
                return;
            }
            Address upstreamPast = properties["inChannelPast"].input;
            if (upstreamPast == null) {
                return;
            }

            state = upstreamNow.node.getData(upstreamNow.port);
            if (state == null) {
                return;
            }
            DataBlob statePast = upstreamPast.node.getData(upstreamPast.port);
            if (statePast == null) {
                return;
            }

            state = state.clone();

            if (state.type != DataBlob.Type.Channels || state.channels == null) {
                state = null;
                return;
            }
            if (statePast.type != DataBlob.Type.Channels || statePast.channels == null) {
                return;
            }

            //check resolutions match
            if (state.channels[0].Length != statePast.channels[0].Length
                || state.channels[1].Length != statePast.channels[1].Length
                || state.channels[2].Length != statePast.channels[2].Length) {
                return;
            }

            //create copy of channels for local use.
            byte[][] newChannels = new byte[state.channels.Length][];
            for (int channel = 0; channel < newChannels.Length; channel++) {
                newChannels[channel] = (byte[])state.channels[channel].Clone();
            }
            state.channels = newChannels;

            calcMoVec(statePast.channels);
        }

        private void calcMoVec(byte[][] channels) {
            //for each channel
            //chunk state.channels into 8x8 blocks
            //compare each block with blocks surrounding them in the arg channels 
            //over x = [-7,7] (range 15 values)
            //and  y = [-7,7] (range 15 values)
            Chunker c = new Chunker(chunkSize, state.channelWidth, state.channelHeight, state.channelWidth, 1);
            int pixelTL;
            byte offset;
            vectors = new byte[c.getNumChunks()];
            for (int i = 0; i < c.getNumChunks(); i++) {
                pixelTL = c.chunkIndexToPixelIndex(i);
                //find best match given search area
                offset = findOffsetVector(state.channels[0], channels[0], pixelTL, state.channelWidth);
                //save best match vector
                vectors[i] = offset;
                //update channels to be difference.
                setDiff(state.channels[0], channels[0], pixelTL, offset, state.channelWidth);
            }
        }

        private void setDiff(byte[] goal, byte[] searchArea, int indexTopLeft, byte offset, int stride) {
            int offsetX = ((offset & 0xf0) >> 4) - 7;
            int offsetY = (offset & 0x0f) - 7;
            int pixelGoal, pixelSearchArea;
            int diff = 0;
            for(int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    pixelGoal = indexTopLeft + y * stride + x;
                    pixelSearchArea = pixelGoal + offsetY * stride + offsetX;
                    if (pixelGoal < 0 || pixelGoal >= goal.Length) {
                        continue;
                    }
                    if (pixelSearchArea < 0 || pixelSearchArea >= searchArea.Length) {
                        continue;
                    }
                    diff = (goal[pixelGoal] - searchArea[pixelSearchArea]);
                    goal[pixelGoal] = (byte)diff;
                }
            }
        }

        private byte findOffsetVector(byte[] goal, byte[] searchArea, int indexTopLeft, int stride) {
            int pixel = 0;
            int diff;
            int minDiff = int.MaxValue;
            int offX = 0;
            int offY = 0;
            for (int yo = -7; yo < 8; yo++) {
                for (int xo = -7; xo < 8; xo++) {
                    pixel = indexTopLeft + yo * stride + xo;
                    diff = SAD(goal, indexTopLeft, searchArea, pixel, stride);
                    if (diff < minDiff) {
                        minDiff = diff;
                        offX = xo;
                        offY = yo;
                    }
                }
            }
            offX += 7; //-7..7 => 0..14
            offY += 7;
            return (byte)((offX << 4) | offY);

        }

        private int SAD(byte[] a, int startA, byte[] b, int startB, int stride) {
            int sad = 0, ia, ib;
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    ia = y * stride + x;
                    ib = ia + startB;
                    ia = ia + startA;
                    if (ia < 0 || ia >= a.Length ||
                        ib < 0 || ib >= b.Length) {
                        continue;
                    }
                    sad += a[ia] > b[ib] ? a[ia] - b[ib] : b[ib] - a[ia];
                }
            }
            return sad;
        }

        public override Bitmap view() {
            base.view();
            if (state == null) {
                return null;
            }
            Bitmap bmp = new Bitmap(state.imageWidth, state.imageHeight, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            //order: B,G,R,  B,G,R,  ...
            int channelIndex = 0, counter = 0;
            for (int y = 0; y < state.imageHeight; y++) {
                channelIndex = y * state.channelWidth;
                counter = y * state.imageWidth;
                for (int x = 0; x < state.imageWidth; x++) {
                    rgbValues[counter] = state.channels[0][channelIndex];
                    rgbValues[counter + 1] = state.channels[0][channelIndex];
                    rgbValues[counter + 2] = state.channels[0][channelIndex];
                    counter += 3;
                    channelIndex += 1;
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
            state.bmp = bmp;
            return bmp;
        }

        public override void viewExtra(Graphics g) {
            base.viewExtra(g);
            /* Vectors:
                0 1
                2 3
            */
            Chunker c = new Chunker(8, state.imageWidth, state.imageHeight, state.imageWidth, 1);
            int offsetX, offsetY;
            int y = state.imageHeight - 4;
            int x = 4;

            for (int i = 0; i < vectors.Length; i++) {
                offsetX = ((vectors[i] & 0xF0) >> 4) - 7;
                offsetY = (vectors[i] & 0x0F) - 7;
                g.DrawLine(Pens.BlanchedAlmond, x, y, x + offsetX, y + offsetY);
                x += 8;
                if (x + 4 > state.imageWidth) {
                    x = 4;
                    y -= 8;
                }
            }
        }
    }
}
