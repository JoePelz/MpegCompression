using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class ColorToChannels : Node {
        private byte[][] channels;
        private int width;
        private int height;
        
        public ColorToChannels() {
            rename("Color");
            setExtra("to Channels");
        }

        protected override void createInputs() {
            inputs.Add("inColor", null);
        }

        protected override void createOutputs() {
            outputs.Add("outChannels", new HashSet<Address>());
        }
        
        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Channels;
            d.channels = channels;
            d.width = width;
            d.height = height;
            d.samplingMode = Subsample.Samples.s444;
            return d;
        }

        protected override void clean() {
            base.clean();

            bmp = null;
            channels = null;

            //Acquire source
            Address upstream = inputs["inColor"];
            if (upstream == null) {
                return;
            }
            DataBlob dataIn = upstream.node.getData(upstream.port);
            if (dataIn == null || dataIn.img == null) {
                return;
            }
            bmpToChannels(bmp);
        }
        
        private void bmpToChannels(Bitmap bmp) {
            width = bmp.Width;
            height = bmp.Height;

            BitmapData bmpData = bmp.LockBits(
                               new Rectangle(0, 0, bmp.Width, bmp.Height),
                               ImageLockMode.ReadOnly,
                               bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            channels = new byte[3][];
            channels[0] = new byte[bmp.Width * bmp.Height];
            channels[1] = new byte[bmp.Width * bmp.Height];
            channels[2] = new byte[bmp.Width * bmp.Height];

            int pixel;
            int iY = 0;

            for (int y = 0; y < bmpData.Height; y++) {
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.

                    channels[0][iY] = rgbValues[pixel + 2];
                    channels[1][iY] = rgbValues[pixel + 1];
                    channels[2][iY++] = rgbValues[pixel];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }
    }
}
