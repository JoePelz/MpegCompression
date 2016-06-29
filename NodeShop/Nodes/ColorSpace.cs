using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeShop.NodeProperties;

namespace NodeShop.Nodes {
    public class ColorSpace : ColorNode {
        public enum Space { RGB, HSV, YCrCb };
        private static string[] options = new string[3] {"RGB", "HSV", "YCrCb" };
        
        private Space inSpace = Space.RGB;
        private Space outSpace = Space.RGB;

        public ColorSpace(): base() { }
        public ColorSpace(NodeView graph) : base(graph) { }
        public ColorSpace(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }
        
        protected override void init() {
            base.init();
            rename("ColorSpace");
        }

        protected override void createProperties() {
            base.createProperties();

            setExtra(inSpace.ToString() + " to " + outSpace.ToString());

            //Will need to choose input color space
            //and output color space.
            Property p = new PropertySelection(options, (int)inSpace, "input color space");
            p.eValueChanged += P_eValueChanged;
            properties["inSpace"] = p;
            p = new PropertySelection(options, (int)outSpace, "output color space");
            p.eValueChanged += P_eValueChanged;
            properties["outSpace"] = p;
        }

        public void setInSpace(Space space) {
            inSpace = space;
            properties["inSpace"].nValue = (int)space;
            setExtra(inSpace.ToString() + " to " + outSpace.ToString());
            soil();
        }

        public void setOutSpace(Space space) {
            outSpace = space;
            properties["outSpace"].nValue = (int)space;
            setExtra(inSpace.ToString() + " to " + outSpace.ToString());
            soil();
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            inSpace = (Space)properties["inSpace"].nValue;
            outSpace = (Space)properties["outSpace"].nValue;

            setExtra(inSpace.ToString() + " to " + outSpace.ToString());

            soil();
        }

        protected override void processPixels(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            switch (inSpace) {
                case Space.RGB:
                    Array.Copy(inValues, outValues, outValues.Length);
                    break;
                case Space.HSV:
                    HSVtoRGB(inValues, outValues, w, h, xstep, ystep);
                    break;
                case Space.YCrCb:
                    YCrCbtoRGB(inValues, outValues, w, h, xstep, ystep);
                    break;
            }
            switch (outSpace) {
                case Space.RGB:
                    break;
                case Space.HSV:
                    RGBtoHSV(outValues, outValues, w, h, xstep, ystep);
                    break;
                case Space.YCrCb:
                    RGBtoYCrCb(outValues, outValues, w, h, xstep, ystep);
                    break;
            }
        }
        
        private static void YCrCbtoRGB(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            float Y, Cr, Cb;
            int r, g, b;
            int pixel;
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    pixel = y * ystep + x * xstep;

                    Cb = inValues[pixel] - 128;
                    Cr = inValues[pixel + 1] - 128;
                    Y = inValues[pixel + 2];
                    
                    r = (int)(Y + 1.400 * Cr);
                    g = (int)(Y - 0.343 * Cb - 0.711 * Cr);
                    b = (int)(Y + 1.765 * Cb);
                    
                    r = r < 0 ? 0 : (r > 255 ? 255 : r);
                    g = g < 0 ? 0 : (g > 255 ? 255 : g);
                    b = b < 0 ? 0 : (b > 255 ? 255 : b);
                    outValues[pixel] = (byte)b;
                    outValues[pixel + 1] = (byte)g;
                    outValues[pixel + 2] = (byte)r;
                }
            }
        }
        private static void HSVtoRGB(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            float H, S, V;
            float C, X, m;
            float r, g, b;
            int pixel;
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    pixel = y * ystep + x * xstep;

                    V = inValues[pixel] / 255.0f;
                    S = inValues[pixel + 1] / 255.0f;
                    H = inValues[pixel + 2] / 255.0f * 359.0f;

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

                    outValues[pixel] = (byte)((b + m) * 255);
                    outValues[pixel + 1] = (byte)((g + m) * 255);
                    outValues[pixel + 2] = (byte)((r + m) * 255);
                }
            }
        }


        private static void RGBtoYCrCb(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            float r, g, b;
            byte Y, Cr, Cb;

            int pixel;
            //It is necessary to do width and height separately because
            //  there may be padding bytes at the edge of the image to
            //  make each row fit into a multiple of 4 bytes
            //  e.g. image is 3 pixels wide, 24bpp. 
            //       that's 9 bytes per row, but stride would be 12
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    pixel = y * ystep + x * xstep;

                    b = inValues[pixel];
                    g = inValues[pixel + 1];
                    r = inValues[pixel + 2];

                    Y  = (byte)(      ( 0.299 * r) + ( 0.587 * g) + ( 0.114 * b));
                    Cb = (byte)(128 + (-0.169 * r) + (-0.331 * g) + ( 0.500 * b));
                    Cr = (byte)(128 + ( 0.500 * r) + (-0.419 * g) + (-0.081 * b));

                    outValues[pixel] = Cb;
                    outValues[pixel + 1] = Cr;
                    outValues[pixel + 2] = Y;
                }
            }
        }
        private static void RGBtoHSV(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            byte min, delta, V;
            byte r, g, b;
            int pixel;
            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    pixel = y * ystep + x * xstep;

                    b = inValues[pixel];
                    g = inValues[pixel + 1];
                    r = inValues[pixel + 2];

                    V = Math.Max(r, Math.Max(g, b));
                    outValues[pixel] = V;

                    min = Math.Min(r, Math.Min(g, b));
                    delta = (byte)(V - min);

                    if (delta == 0) {
                        outValues[pixel + 1] = 0;
                        outValues[pixel + 2] = 0;
                    } else {
                        outValues[pixel + 1] = (byte)(delta * 255 / V);
                        if (r == V) {
                            outValues[pixel + 2] = (byte)(60.0 * ((g - b) / (double)delta % 6) / 359 * 255);
                        } else if (g == V) {
                            outValues[pixel + 2] = (byte)(60.0 * ((b - r) / (double)delta + 2) / 359 * 255);
                        } else if (b == V) {
                            outValues[pixel + 2] = (byte)(60.0 * ((r - g) / (double)delta + 4) / 359 * 255);
                        }
                    }
                }
            }
        }
    }
}
