using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class Merge : Node {

        public Merge() {
            rename("Merge");
        }

        protected override void createProperties() {
            base.createProperties();

            properties.Add("inColorA", new Property(true, false));
            properties.Add("inColorB", new Property(true, false));
            properties.Add("outColor", new Property(false, true));
        }

        protected override void clean() {
            base.clean();

            Address upstreamA = properties["inColorA"].input;
            if (upstreamA == null) {
                return;
            }
            Address upstreamB = properties["inColorB"].input;
            if (upstreamB == null) {
                return;
            }

            state = upstreamA.node.getData(upstreamA.port);
            if (state == null) {
                return;
            }
            DataBlob stateB = upstreamB.node.getData(upstreamA.port);
            if (stateB == null) {
                return;
            }

            state = state.clone();

            if (state.type != DataBlob.Type.Image || state.bmp == null) {
                state = null;
                return;
            }
            if (stateB.type != DataBlob.Type.Image || stateB.bmp == null) {
                stateB = null;
                return;
            }

            //check resolutions match
            if (state.imageWidth != stateB.imageWidth || state.imageHeight != stateB.imageHeight) {
                return;
            }

            state.bmp = state.bmp.Clone(new Rectangle(0, 0, state.bmp.Width, state.bmp.Height), state.bmp.PixelFormat);
            merge(stateB.bmp);
        }

        public void merge(Bitmap B) {
            BitmapData bmpDataA = state.bmp.LockBits(
                                new Rectangle(0, 0, state.bmp.Width, state.bmp.Height),
                                ImageLockMode.ReadWrite,
                                state.bmp.PixelFormat);
            BitmapData bmpDataB = B.LockBits(
                                new Rectangle(0, 0, B.Width, B.Height),
                                ImageLockMode.ReadWrite,
                                B.PixelFormat);

            IntPtr ptrA = bmpDataA.Scan0;
            IntPtr ptrB = bmpDataB.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpDataA.Stride) * bmpDataA.Height;
            byte[] rgbValuesA = new byte[nBytes];
            byte[] rgbValuesB = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptrA, rgbValuesA, 0, nBytes);
            System.Runtime.InteropServices.Marshal.Copy(ptrB, rgbValuesB, 0, nBytes);

            int rA, gA, bA;
            int rB, gB, bB;

            int pixel;
            for (int y = 0; y < bmpDataA.Height; y++) {
                for (int x = 0; x < bmpDataA.Width; x++) {
                    pixel = y * bmpDataA.Stride + x * 3; //assuming 3 channels. Sorry.

                    bA = rgbValuesA[pixel];
                    gA = rgbValuesA[pixel + 1];
                    rA = rgbValuesA[pixel + 2];

                    bB = rgbValuesB[pixel];
                    gB = rgbValuesB[pixel + 1];
                    rB = rgbValuesB[pixel + 2];

                    rgbValuesA[pixel] = (byte)(bA * bB);
                    rgbValuesA[pixel + 1] = (byte)(gA * gB);
                    rgbValuesA[pixel + 2] = (byte)(rA * rB);
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValuesA, 0, ptrA, nBytes);

            state.bmp.UnlockBits(bmpDataA);
            B.UnlockBits(bmpDataB);
        }
    }
}
