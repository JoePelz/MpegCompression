using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class MoVecCompose : Node {
        public MoVecCompose() {
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


            if (stateDiff.type != DataBlob.Type.Channels || stateDiff.channels == null) {
                return;
            }
            if (statePast.type != DataBlob.Type.Channels || statePast.channels == null) {
                return;
            }
            if (stateVectors.type != DataBlob.Type.Vectors || stateVectors.channels == null) {
                return;
            }

            state = stateDiff.clone();

        }

        public override Bitmap view() {
            base.view();
            if (state == null) {
                return null;
            } else if (state.bmp != null) {
                return state.bmp;
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
                counter = y * bmpData.Stride;
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

    }
}
