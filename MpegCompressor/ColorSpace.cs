using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class ColorSpace : Node {
        private enum Space { RGB, HSV, YCrCb };

        private Bitmap bmp;
        private static string[] options 
            = new string[3] {
                "RGB",
                "HSV",
                "YCrCb" };
        private Space inSpace = Space.RGB;
        private Space outSpace = Space.RGB;

        protected override void createInputs() {
            inputs.Add("inColor", null);
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("ColorSpace");

            setExtra(inSpace.ToString() + " to " + outSpace.ToString());

            //Will need to choose input color space
            //and output color space.
            p = new Property();
            p.createChoices(options, (int)inSpace, "input color space");
            p.eValueChanged += P_eValueChanged;
            properties["inSpace"] = p;
            p = new Property();
            p.createChoices(options, (int)outSpace, "output color space");
            p.eValueChanged += P_eValueChanged;
            properties["outSpace"] = p;
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            inSpace = (Space)properties["inSpace"].getSelection();
            outSpace = (Space)properties["outSpace"].getSelection();

            setExtra(inSpace.ToString() + " to " + outSpace.ToString());

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
            Address upstream = inputs["inColor"];
            if (upstream == null) {
                return;
            }
            DataBlob dataIn = upstream.node.getData(upstream.port);
            if (dataIn == null) {
                return;
            }

            if (dataIn.type == DataBlob.Type.Channels) {
                //TODO: disallow this. We shouldn't silently change the channel dimensions. Only do this is channels are 4:4:4 already.
                bmp = Subsample.channelsToBitmap(dataIn.channels, dataIn.samplingMode, dataIn.width);
            } else if (dataIn.type == DataBlob.Type.Image) {
                bmp = dataIn.img.Clone(new Rectangle(0, 0, dataIn.img.Width, dataIn.img.Height), dataIn.img.PixelFormat);
            } else {
                bmp = null;
                return;
            }
            
            if (inSpace == outSpace)
                return;

            switch (inSpace) {
                case Space.RGB:
                    //doNothing
                    break;
                case Space.HSV:
                    HSVtoRGB(bmp);
                    break;
                case Space.YCrCb:
                    YCrCbtoRGB(bmp);
                    break;
            }
            switch(outSpace) {
                case Space.RGB:
                    break;
                case Space.HSV:
                    RGBtoHSV(bmp);
                    break;
                case Space.YCrCb:
                    RGBtoYCrCb(bmp);
                    break;
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
            if (bmp == null) {
                return;
            }

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

        private static void HSVtoRGB(Bitmap bmp) {
            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            float H, S, V;
            float C, X, m;
            float r, g, b;
            int pixel;
            for (int y = 0; y < bmpData.Height; y++) {
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.

                    V = rgbValues[pixel] / 255.0f;
                    S = rgbValues[pixel + 1] / 255.0f;
                    H = rgbValues[pixel + 2] / 255.0f * 359.0f;

                    C = V * S;
                    X = C * (1 - Math.Abs((H / 60.0f) % 2 - 1));
                    m = V - C;
                    if (H < 60) {
                        r = C; g = X; b = 0;
                    } else if (H < 120) {
                        r = X; g = C; b = 0;
                    } else if (H < 180) {
                        r = 0; g = C; b = X;
                    } else if (H < 240) {
                        r = 0; g = X; b = C;
                    } else if (H < 300) {
                        r = X; g = 0; b = C;
                    } else {
                        r = C; g = 0; b = X;
                    }

                    rgbValues[pixel] = (byte)((b + m) * 255);
                    rgbValues[pixel + 1] = (byte)((g + m) * 255);
                    rgbValues[pixel + 2] = (byte)((r + m) * 255);
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }

        private static void RGBtoHSV(Bitmap bmp) {
            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            byte min;
            byte delta;
            byte V;
            byte r, g, b;
            int pixel;
            for (int y = 0; y < bmpData.Height; y++) {
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.

                    b = rgbValues[pixel];
                    g = rgbValues[pixel + 1];
                    r = rgbValues[pixel + 2];

                    V = Math.Max(r, Math.Max(g, b));
                    rgbValues[pixel] = V;

                    min = Math.Min(r, Math.Min(g, b));
                    delta = (byte)(V - min);

                    if (delta == 0) {
                        rgbValues[pixel + 1] = 0;
                        rgbValues[pixel + 2] = 0;
                    } else {
                        rgbValues[pixel + 1] = (byte)(delta * 255 / V);
                        if (r == V) {
                            rgbValues[pixel + 2] = (byte)(60.0 * ((g - b) / (double)delta % 6) / 359 * 255);
                        } else if (g == V) {
                            rgbValues[pixel + 2] = (byte)(60.0 * ((b - r) / (double)delta + 2) / 359 * 255);
                        } else if (b == V) {
                            rgbValues[pixel + 2] = (byte)(60.0 * ((r - g) / (double)delta + 4) / 359 * 255);
                        }
                    }
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
