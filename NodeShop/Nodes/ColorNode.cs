using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeShop.NodeProperties;
using System.Drawing.Imaging;

namespace NodeShop.Nodes {
    public abstract class ColorNode : Node {
        protected const int r = 2;
        protected const int g = 1;
        protected const int b = 0;

        public ColorNode(): base() { }
        public ColorNode(NodeView graph) : base(graph) { }
        public ColorNode(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void createProperties() {
            base.createProperties();
            properties.Add("inColor", new PropertyColor(true, false));
            properties.Add("outColor", new PropertyColor(false, true));
        }

        protected override void clean() {
            base.clean();

            Address upstream = properties["inColor"].input;
            if (upstream == null) {
                return;
            }

            state = upstream.node.getData(upstream.port);
            if (state == null) {
                return;
            }

            if (state.type != DataBlob.Type.Image || state.bmp == null) {
                state = null;
                return;
            }

            state = state.clone();

            state.bmp = state.bmp.Clone(new Rectangle(0, 0, state.bmp.Width, state.bmp.Height), state.bmp.PixelFormat);

            processBitmap();
        }

        protected void processBitmap() {
            BitmapData bmpData = state.bmp.LockBits(
                                new Rectangle(0, 0, state.bmp.Width, state.bmp.Height),
                                ImageLockMode.ReadWrite,
                                state.bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * state.bmp.Height;
            byte[] inValues = new byte[nBytes];
            byte[] outValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, inValues, 0, nBytes);

            processPixels(inValues, outValues, bmpData.Width, bmpData.Height, 3, bmpData.Stride);

            System.Runtime.InteropServices.Marshal.Copy(outValues, 0, ptr, nBytes);

            state.bmp.UnlockBits(bmpData);
        }

        protected virtual void processPixels(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            Array.Copy(inValues, outValues, outValues.Length);
        }
    }
}
