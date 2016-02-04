using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class ColorSpace : Node {
        private Bitmap bmp;
        private static string[] options 
            = new string[2] {
                "RGB to YCrCb",
                "YCrCb to RGB" };
        private int mode = 0;
        
        protected override void createInputs() {
            inputs.Add("inColor", null);
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("ColorSpace");

            //Will need to choose input color space
            //and output color space.
            p = new Property();
            p.createChoices(options, mode, "Color transformation");
            p.eValueChanged += P_eValueChanged;
            properties["space"] = p;
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            mode = (sender as Property).getSelection();
            soil();
            Invalidate();
        }

        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Image;
            d.img = bmp;
            return d;
        }

        protected override void clean() {
            base.clean();
            DataBlob upstream = inputs["inColor"].node.getData(inputs["inColor"].port);
            if (upstream.img != null) {
                bmp = upstream.img.Clone(new Rectangle(0, 0, upstream.img.Width, upstream.img.Height), upstream.img.PixelFormat);
                if (mode == 0)
                    RGBtoYCrCb(bmp);
                else if (mode == 1)
                    YCrCbtoRGB(bmp);
            } else {
                bmp = null;
            }
        }

        private static void RGBtoYCrCb(Bitmap bmp) {
            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            float r, g, b;
            byte Y, Cr, Cb;

            int pixel;
            //It is necessary to do width and height separately because
            //  there may be padding bytes at the edge of the image to
            //  make each row fit into a multiple of 4 bytes
            //  e.g. image is 3 pixels wide, 24bpp. 
            //       that's 9 bytes per row, but stride would be 12
            for (int y = 0; y < bmpData.Height; y++) {
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.

                    b = rgbValues[pixel];
                    g = rgbValues[pixel + 1];
                    r = rgbValues[pixel + 2];

                    Y = (byte)((0.299 * r) + (0.587 * g) + (0.114 * b));
                    Cb = (byte)(128 - (0.168736 * r) - (0.331264 * g) + (0.500000 * b));
                    Cr = (byte)(128 + (0.500000 * r) - (0.418688 * g) - (0.081312 * b));

                    rgbValues[pixel] = Cb;
                    rgbValues[pixel + 1] = Cr;
                    rgbValues[pixel + 2] = Y;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }
        
        private static void YCrCbtoRGB(Bitmap bmp) {
            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            float Y, Cr, Cb;
            int r, g, b;
            int pixel;
            for (int y = 0; y < bmpData.Height; y++) {
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.

                    Cb = rgbValues[pixel] - 128;
                    Cr = rgbValues[pixel + 1] - 128;
                    Y = rgbValues[pixel + 2];

                    r = (int)(Y + 1.40200 * Cr);
                    g = (int)(Y - 0.34414 * Cb - 0.71414 * Cr);
                    b = (int)(Y + 1.77200 * Cb);
                    r = r < 0 ? 0 : r > 255 ? 255 : r;
                    g = g < 0 ? 0 : g > 255 ? 255 : g;
                    b = b < 0 ? 0 : b > 255 ? 255 : b;
                    
                    rgbValues[pixel] = (byte)b;
                    rgbValues[pixel + 1] = (byte)g;
                    rgbValues[pixel + 2] = (byte)r;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }

        public override Bitmap view() {
            base.view();
            if (bmp == null) {
                return null;
            }
            return bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
        }

        public override Rectangle getExtents() {
            if (bmp == null) {
                return new Rectangle(0, 0, 0, 0);
            }
            return new Rectangle(-bmp.Width / 2, -bmp.Height / 2, bmp.Width, bmp.Height);
        }
    }
}
