using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class Subsample : ChannelNode {
        public enum Samples { s444, s422, s411, s420 }
        private static string[] options
            = new string[4] {
                "4:4:4",
                "4:2:2",
                "4:1:1",
                "4:2:0" };
        private Samples outSamples;

        public Subsample() {
            //default setup
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("SubSample");

            setExtra(options[(int)samples] + " to " + options[(int)outSamples]);

            //Will need to choose output sample space.
            p = new Property();
            p.createChoices(options, (int)outSamples, "output sample space");
            p.eValueChanged += P_eValueChanged;
            properties["outSamples"] = p;
        }

        public void setOutSamples(Samples samples) {
            outSamples = samples;
            properties["outSamples"].setSelection((int)samples);
            setExtra(options[(int)samples] + " to " + options[(int)outSamples]);
            soil();
        }
        
        private void P_eValueChanged(object sender, EventArgs e) {
            setOutSamples((Samples)properties["outSamples"].getSelection());
        }

        public override DataBlob getData(string port) {
            DataBlob data = base.getData(port);
            data.samplingMode = outSamples;
            return data;
        }
        
        protected override void clean() {
            base.clean();
            
            setExtra(options[(int)samples] + " to " + options[(int)outSamples]);

            //Transform data
            convertSrcChannels();
        }
        
        private void upsample() {
            int size4 = channels[0].Length; //Y channel. Should be full length.
            int size422 = (width + 1) / 2 * height;
            int size420 = (width + 1) / 2 * (height + 1) / 2;
            int size411 = (width + 3) / 4 * height;

            byte[] newG;
            byte[] newB;

            int iNew, iOld;

            switch (samples) {
                case Samples.s444:
                    //4:4:4 should have channels 1 and 2 be size4
                    if (channels[1].Length != size4 || channels[2].Length != size4) {
                        channels = null;
                        return;
                    }
                    break;
                case Samples.s422:
                    //4:2:2 should have channels 1 and 2 be size422
                    if (channels[1].Length != size422 || channels[2].Length != size422) {
                        channels = null;
                        return;
                    }
                    newG = new byte[size4];
                    newB = new byte[size4];
                    iOld = 0;
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            iNew = y * width + x;
                            //pixel iterates over every pixel in the full size image.
                            if (x % 2 == 0) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                if (x == width - 1)
                                    iOld++;
                            } else if (x % 2 == 1) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                iOld++;
                            }
                        }
                    }
                    channels[1] = newG;
                    channels[2] = newB;
                    break;
                case Samples.s420:
                    //4:2:0 should have channels 1 and 2 be size1
                    if (channels[1].Length != size420 || channels[2].Length != size420) {
                        channels = null;
                        return;
                    }
                    newG = new byte[size4];
                    newB = new byte[size4];
                    iOld = -1;
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            iNew = y * width + x;

                            if (x % 2 == 0) {
                                iOld++;
                            }
                            if (y % 2 == 1 && x == 0) {
                                iOld -= (width + 1) / 2;
                            }

                            newG[iNew] = channels[1][iOld];
                            newB[iNew] = channels[2][iOld];
                        }
                    }
                    channels[1] = newG;
                    channels[2] = newB;
                    break;
                case Samples.s411:
                    //4:1:1 should have channels 1 and 2 be size411
                    if (channels[1].Length != size411 || channels[2].Length != size411) {
                        channels = null;
                        return;
                    }
                    newG = new byte[size4];
                    newB = new byte[size4];
                    iOld = 0;
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            iNew = y * width + x;
                            //pixel iterates over every pixel in the full size image.
                            if (x % 4 < 3) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                if (x == width - 1)
                                    iOld++;
                            } else if (x % 4 == 3) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                iOld++;
                            }
                        }
                    }
                    channels[1] = newG;
                    channels[2] = newB;
                    break;
            }
        }
        
        private void convertSrcChannels() {

            if (channels == null) {
                return;
            }

            upsample();

            //drop elements as needed
            //create BMP of r dimensions.
            //write channels into bmp, duplicating as needed.
            int size4 = channels[0].Length; //Y channel. Should be full length.
            int size422 = (width + 1) / 2 * height;
            int size420 = (width + 1) / 2 * (height + 1) / 2;
            int size411 = (width + 3) / 4 * height;
            int iNew, iOld;
            byte[] newG;
            byte[] newB;

            switch (outSamples) {
                case Samples.s444:
                    break; //done! :D
                case Samples.s422:
                    newG = new byte[size422];
                    newB = new byte[size422];
                    iNew = 0;
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x+= 2) {
                            iOld = y * width + x;
                            if (x % 2 == 0) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                iNew++;
                            }
                        }
                    }
                    channels[1] = newG;
                    channels[2] = newB;
                    break;
                case Samples.s420:
                    newG = new byte[size420];
                    newB = new byte[size420];
                    iNew = 0;
                    for (int y = 0; y < height; y += 2) {
                        for (int x = 0; x < width; x += 2) {
                            iOld = y * width + x;
                            if (x % 2 == 0 && y % 2 == 0) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                iNew++;
                            }
                        }
                    }
                    channels[1] = newG;
                    channels[2] = newB;
                    break;
                case Samples.s411:
                    newG = new byte[size411];
                    newB = new byte[size411];
                    iNew = 0;
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x += 4) {
                            iOld = y * width + x;
                            if (x % 4 == 0) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                iNew++;
                            }
                        }
                    }
                    channels[1] = newG;
                    channels[2] = newB;
                    break;
            }

            if (bmp != null) {
                bmp.Dispose();
            }
            bmp = channelsToBitmap(channels, outSamples, width, height);
        }

        public static Bitmap channelsToBitmap(byte[][] channels, Samples mode, int width, int height) {
            if (channels == null) {
                return null;
            }

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

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
            int iY = 0, iCrb = 0;
            switch (mode) {
                case Samples.s444:
                    for (int y = 0; y < bmpData.Height; y++) {
                        for (int x = 0; x < bmpData.Width; x++) {
                            pixel = y * bmpData.Stride + x * 3;
                            rgbValues[pixel + 2] = channels[0][iY];
                            rgbValues[pixel + 1] = channels[1][iCrb];
                            rgbValues[pixel] = channels[2][iCrb];
                            iY++;
                            iCrb++;
                        }
                    }
                    break;
                case Samples.s422:
                    iY = -1;
                    iCrb = -1;
                    for (int y = 0; y < bmpData.Height; y++) {
                        for (int x = 0; x < bmpData.Width; x++) {
                            pixel = y * bmpData.Stride + x * 3;

                            iY++;
                            if (x % 2 == 0) {
                                iCrb++;
                            }
                            rgbValues[pixel + 2] = channels[0][iY];
                            rgbValues[pixel + 1] = channels[1][iCrb];
                            rgbValues[pixel] = channels[2][iCrb];
                        }
                    }
                    break;
                case Samples.s420:
                    iY = -1;
                    iCrb = -1;
                    for (int y = 0; y < bmpData.Height; y++) {
                        for (int x = 0; x < bmpData.Width; x++) {
                            pixel = y * bmpData.Stride + x * 3;

                            iY++;
                            if (x % 2 == 0) {
                                iCrb++;
                            }
                            if (y % 2 == 1 && x == 0) {
                                iCrb -= (width + 1) / 2;
                            }
                            rgbValues[pixel + 2] = channels[0][iY];
                            rgbValues[pixel + 1] = channels[1][iCrb];
                            rgbValues[pixel] = channels[2][iCrb];
                        }
                    }
                    break;
                case Samples.s411:
                    iY = -1;
                    iCrb = -1;
                    for (int y = 0; y < bmpData.Height; y++) {
                        for (int x = 0; x < bmpData.Width; x++) {
                            pixel = y * bmpData.Stride + x * 3;

                            iY++;
                            if (x % 4 == 0) {
                                iCrb++;
                            }
                            rgbValues[pixel + 2] = channels[0][iY];
                            rgbValues[pixel + 1] = channels[1][iCrb];
                            rgbValues[pixel] = channels[2][iCrb];
                        }
                    }
                    break;
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

    }
}
