using MpegCompressor.NodeProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class Grade : ColorNode {
        public Grade(): base() { }
        public Grade(NodeView graph) : base(graph) { }
        public Grade(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }

        protected override void init() {
            base.init();
            rename("Grade");
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertyFloat(0, -100, 100, "Black point");
            p.eValueChanged += (s, e) => { soil(); };
            properties["black"] = p;

            p = new PropertyFloat(1, -100, 100, "White point");
            p.eValueChanged += (s, e) => { soil(); };
            properties["white"] = p;

            p = new PropertyFloat(1, -100, 100, "Multiply");
            p.eValueChanged += (s, e) => { soil(); };
            properties["mult"] = p;

            p = new PropertyFloat(0, -100, 100, "Offset");
            p.eValueChanged += (s, e) => { soil(); };
            properties["add"] = p;
        }

        protected override void clean() {
            base.clean();

            if (state == null) {
                return;
            }

            BitmapData bmpData = state.bmp.LockBits(
                                new Rectangle(0, 0, state.bmp.Width, state.bmp.Height),
                                ImageLockMode.ReadWrite,
                                state.bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * state.bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            float black = properties["black"].fValue;
            float white = properties["white"].fValue;
            float mult  = properties["mult" ].fValue;
            float add   = properties[ "add" ].fValue;
            float val;
            int bands = 3;
            int pixel;
            for (int band = 0; band < bands; band++) {
                //the non-edge pixels
                for (int y = 0; y < bmpData.Height; y++) {
                    for (int x = 0; x < bmpData.Width; x++) {
                        pixel = y * bmpData.Stride + x * bands; //assuming 3 channels. Sorry.
                        val = rgbValues[pixel + band];

                        val /= 255;
                        val -= black;
                        val /= (white - black);


                        val *= mult;
                        val += add;

                        val = val > 1 ? 1 : val < 0 ? 0 : val;
                        rgbValues[pixel + band] = (byte)(val * 255);
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            state.bmp.UnlockBits(bmpData);
        }
    }
}
