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
        
        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("SubSample");
            
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
        
        protected override void clean() {
            base.clean();
            
            if (state == null || state.channels == null) {
                return;
            }

            setExtra(options[(int)state.samplingMode] + " to " + options[(int)outSamples]);

            //Transform data
            convertSrcChannels();

            state.samplingMode = outSamples;
        }
        
        private void upsample() {
            int size4 = state.channels[0].Length; //Y channel. Should be full length.
            int size422 = (state.width + 1) / 2 * state.height;
            int size420 = (state.width + 1) / 2 * (state.height + 1) / 2;
            int size411 = (state.width + 3) / 4 * state.height;

            byte[] newG;
            byte[] newB;

            int iNew, iOld;

            switch (state.samplingMode) {
                case Samples.s444:
                    //4:4:4 should have channels 1 and 2 be size4
                    if (state.channels[1].Length != size4 || state.channels[2].Length != size4) {
                        state.channels = null;
                        return;
                    }
                    break;
                case Samples.s422:
                    //4:2:2 should have channels 1 and 2 be size422
                    if (state.channels[1].Length != size422 || state.channels[2].Length != size422) {
                        state.channels = null;
                        return;
                    }
                    newG = new byte[size4];
                    newB = new byte[size4];
                    iOld = 0;
                    for (int y = 0; y < state.height; y++) {
                        for (int x = 0; x < state.width; x++) {
                            iNew = y * state.width + x;
                            //pixel iterates over every pixel in the full size image.
                            if (x % 2 == 0) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                if (x == state.width - 1)
                                    iOld++;
                            } else if (x % 2 == 1) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                iOld++;
                            }
                        }
                    }
                    state.channels[1] = newG;
                    state.channels[2] = newB;
                    break;
                case Samples.s420:
                    //4:2:0 should have channels 1 and 2 be size1
                    if (state.channels[1].Length != size420 || state.channels[2].Length != size420) {
                        state.channels = null;
                        return;
                    }
                    newG = new byte[size4];
                    newB = new byte[size4];
                    iOld = -1;
                    for (int y = 0; y < state.height; y++) {
                        for (int x = 0; x < state.width; x++) {
                            iNew = y * state.width + x;

                            if (x % 2 == 0) {
                                iOld++;
                            }
                            if (y % 2 == 1 && x == 0) {
                                iOld -= (state.width + 1) / 2;
                            }

                            newG[iNew] = state.channels[1][iOld];
                            newB[iNew] = state.channels[2][iOld];
                        }
                    }
                    state.channels[1] = newG;
                    state.channels[2] = newB;
                    break;
                case Samples.s411:
                    //4:1:1 should have channels 1 and 2 be size411
                    if (state.channels[1].Length != size411 || state.channels[2].Length != size411) {
                        state.channels = null;
                        return;
                    }
                    newG = new byte[size4];
                    newB = new byte[size4];
                    iOld = 0;
                    for (int y = 0; y < state.height; y++) {
                        for (int x = 0; x < state.width; x++) {
                            iNew = y * state.width + x;
                            //pixel iterates over every pixel in the full size image.
                            if (x % 4 < 3) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                if (x == state.width - 1)
                                    iOld++;
                            } else if (x % 4 == 3) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                iOld++;
                            }
                        }
                    }
                    state.channels[1] = newG;
                    state.channels[2] = newB;
                    break;
            }
        }
        
        private void convertSrcChannels() {

            if (state.channels == null) {
                return;
            }

            upsample();

            //drop elements as needed
            //create BMP of r dimensions.
            //write channels into bmp, duplicating as needed.
            int size4 = state.channels[0].Length; //Y channel. Should be full length.
            int size422 = (state.width + 1) / 2 * state.height;
            int size420 = (state.width + 1) / 2 * (state.height + 1) / 2;
            int size411 = (state.width + 3) / 4 * state.height;
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
                    for (int y = 0; y < state.height; y++) {
                        for (int x = 0; x < state.width; x+= 2) {
                            iOld = y * state.width + x;
                            if (x % 2 == 0) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                iNew++;
                            }
                        }
                    }
                    state.channels[1] = newG;
                    state.channels[2] = newB;
                    break;
                case Samples.s420:
                    newG = new byte[size420];
                    newB = new byte[size420];
                    iNew = 0;
                    for (int y = 0; y < state.height; y += 2) {
                        for (int x = 0; x < state.width; x += 2) {
                            iOld = y * state.width + x;
                            if (x % 2 == 0 && y % 2 == 0) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                iNew++;
                            }
                        }
                    }
                    state.channels[1] = newG;
                    state.channels[2] = newB;
                    break;
                case Samples.s411:
                    newG = new byte[size411];
                    newB = new byte[size411];
                    iNew = 0;
                    for (int y = 0; y < state.height; y++) {
                        for (int x = 0; x < state.width; x += 4) {
                            iOld = y * state.width + x;
                            if (x % 4 == 0) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                iNew++;
                            }
                        }
                    }
                    state.channels[1] = newG;
                    state.channels[2] = newB;
                    break;
            }

            state.bmp = channelsToBitmap(state.channels, outSamples, state.width, state.height);
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
