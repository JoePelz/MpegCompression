using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using NodeShop.NodeProperties;
using System.Drawing.Imaging;

namespace NodeShop.Nodes {
    public class ReadImage : Node {
        private string filepath;

        public ReadImage(): base() { }
        public ReadImage(NodeView graph) : base(graph) { }
        public ReadImage(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }

        protected override void init() {
            base.init();
            setPath("C:\\temp\\uv.jpg");
            rename("ReadImage");
        }

        protected override void createProperties() {
            base.createProperties();

            //create filepath property
            Property p = new PropertyString("", "Image file to open");
            p.eValueChanged += OnPathChange;
            properties.Add("path", p);

            properties.Add("outColor", new PropertyChannels(false, true));
        }

        public void setPath(string path) {
            filepath = path;
            properties["path"].sValue = path;

            int lastSlash = path.LastIndexOf('\\') + 1;
            lastSlash = lastSlash == -1 ? 0 : lastSlash;

            setExtra(path.Substring(lastSlash));
            soil();
        }

        private void OnPathChange(object sender, EventArgs e) {
            setPath(properties["path"].sValue);
        }

        protected override void clean() {
            base.clean();
            //load image from path, if it exists
            state = new DataBlob();

            try {
                state.bmp = new Bitmap(filepath);
                state.bmp = state.bmp.Clone(new Rectangle(0, 0, state.bmp.Width, state.bmp.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                state.imageWidth = state.bmp.Width;
                state.imageHeight = state.bmp.Height;
                state.channelWidth = state.imageWidth;
                state.channelHeight = state.imageHeight;
                bmpToChannels(ref state.channels, state.bmp);
                state.type = DataBlob.Type.Channels;
            } catch (Exception) {
                state = null;
            }
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
                    channels[0][iY] = rgbValues[pixel + 2];  // red
                    channels[1][iY] = rgbValues[pixel + 1];  // green
                    channels[2][iY++] = rgbValues[pixel];    // blue
                    pixel += 3;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }
    }
}
