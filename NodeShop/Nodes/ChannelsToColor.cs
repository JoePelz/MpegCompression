using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeShop.NodeProperties;

namespace NodeShop.Nodes {
    class ChannelsToColor : Node {

        public ChannelsToColor(): base() { }
        public ChannelsToColor(NodeView graph) : base(graph) { }
        public ChannelsToColor(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("To Color");
            setExtra("from Channels");
        }


        protected override void createProperties() {
            base.createProperties();
            properties.Add("inChannels", new PropertyChannels(true, false));
            properties.Add("outColor", new PropertyColor(false, true));
        }

        protected override void clean() {
            base.clean();

            //Acquire source
            Address upstream = properties["inChannels"].input;
            if (upstream == null) {
                return;
            }
            state = upstream.node.getData(upstream.port);
            if (state == null) {
                return;
            }

            state = state.clone();

            if (state.type != DataBlob.Type.Channels || state.channels == null) {
                state = null;
                return;
            }

            state.bmp = channelsToBitmap(state);
            state.type = DataBlob.Type.Image;
        }

        public static Bitmap channelsToBitmap(DataBlob data) {
            if (data == null || data.channels == null) {
                return null;
            }

            Bitmap bmp = new Bitmap(data.imageWidth, data.imageHeight, PixelFormat.Format24bppRgb);

            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;


            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);
            int pixel;
            int iY, iCrb;
            int channelYStride = data.channelWidth;
            //TODO: don't assume padded. :P
            int channelCRBStride = Subsample.getCbCrStride(data);

            for (int y = 0; y < data.imageHeight; y++) {
                pixel = y * bmpData.Stride;
                iY = y * channelYStride;
                iCrb = (data.samplingMode == DataBlob.Samples.s420 ? y / 2 : y) * channelCRBStride;
                for (int x = 0; x < data.imageWidth; x++) {
                    rgbValues[pixel + 2] = data.channels[0][iY];
                    rgbValues[pixel + 1] = data.channels[1][iCrb];
                    rgbValues[pixel] = data.channels[2][iCrb];
                    pixel += 3;
                    iY++;

                    if (data.samplingMode == DataBlob.Samples.s420 || data.samplingMode == DataBlob.Samples.s422) {
                        if (x % 2 == 1) {
                            iCrb++;
                        }
                    } else if (data.samplingMode == DataBlob.Samples.s444) {
                        iCrb++;
                    } else if (data.samplingMode == DataBlob.Samples.s411) {
                        if (x % 4 == 3) {
                            iCrb++;
                        }
                    }

                }
            }
            
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
