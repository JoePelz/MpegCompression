using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class ColorToChannels : Node {
        
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

        protected override void clean() {
            base.clean();

            //Acquire source
            Address upstream = inputs["inColor"];
            if (upstream == null) {
                return;
            }
            state = upstream.node.getData(upstream.port);
            if (state == null) {
                return;
            }

            state = state.clone();
            
            if (state.bmp == null) {
                state = null;
                return;
            }

            bmpToChannels(state.bmp);
            state.type = DataBlob.Type.Channels;
        }
        
        private void bmpToChannels(Bitmap bmp) {

            BitmapData bmpData = bmp.LockBits(
                               new Rectangle(0, 0, bmp.Width, bmp.Height),
                               ImageLockMode.ReadOnly,
                               bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            state.channels = new byte[3][];
            state.channels[0] = new byte[bmp.Width * bmp.Height];
            state.channels[1] = new byte[bmp.Width * bmp.Height];
            state.channels[2] = new byte[bmp.Width * bmp.Height];

            int pixel;
            int iY = 0;

            for (int y = 0; y < bmpData.Height; y++) {
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.

                    state.channels[0][iY] = rgbValues[pixel + 2];
                    state.channels[1][iY] = rgbValues[pixel + 1];
                    state.channels[2][iY++] = rgbValues[pixel];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }
    }
}
