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

        protected override void createProperties() {
            base.createProperties();
            properties.Add("inColor", new Property(true, false));
            properties.Add("outChannels", new Property(false, true));
        }

        protected override void clean() {
            base.clean();

            //Acquire source
            Address upstream = properties["inColor"].input;
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

            state.channelWidth = state.imageWidth;
            state.channelHeight = state.imageHeight;
            //Bitmap temp = new Bitmap(state.channelWidth, state.channelHeight, PixelFormat.Format24bppRgb);
            //Graphics g = Graphics.FromImage(temp);
            //g.DrawImage(state.bmp, 0, 0, state.imageWidth, state.imageHeight);
            //g.Dispose();
            bmpToChannels(ref state.channels, state.bmp);
            state.type = DataBlob.Type.Channels;
            state.bmp = null;
        }
        
        private void bmpToChannels(ref byte[][] channels, Bitmap bmp) {

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
                pixel = y * bmpData.Stride;
                for (int x = 0; x < bmpData.Width; x++) {
                    channels[0][iY] = rgbValues[pixel + 2];
                    channels[1][iY] = rgbValues[pixel + 1];
                    channels[2][iY++] = rgbValues[pixel];
                    pixel += 3;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }
    }
}
