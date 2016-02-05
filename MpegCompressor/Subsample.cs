using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class Subsample : Node {
        private enum Samples { s444, s422, s411, s420 }
        private byte[][] channels;
        private Bitmap bmp;
        private static string[] options
            = new string[4] {
                "4:4:4",
                "4:2:2",
                "4:1:1",
                "4:2:0" };
        private Samples inSamples;
        private Samples outSamples;
        private int width;

        public Subsample() {
            //default setup
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("SubSample");

            setExtra(inSamples.ToString().Remove(0, 1) + " to " + outSamples.ToString().Remove(0, 1));

            //Will need to choose output sample space.
            p = new Property();
            p.createChoices(options, (int)inSamples, "input sample space");
            p.eValueChanged += P_eValueChanged;
            properties["inSamples"] = p;
            p = new Property();
            p.createChoices(options, (int)outSamples, "output sample space");
            p.eValueChanged += P_eValueChanged;
            properties["outSamples"] = p;
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            inSamples = (Samples)properties["inSamples"].getSelection();
            outSamples = (Samples)properties["outSamples"].getSelection();

            setExtra(inSamples.ToString().Remove(0, 1) + " to " + outSamples.ToString().Remove(0, 1));

            soil();
            Invalidate();
        }

        protected override void createInputs() {
            inputs.Add("inChannels", null);
        }

        protected override void createOutputs() {
            outputs.Add("outChannels", new HashSet<Address>());
        }

        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Channels;
            d.channels = channels;
            d.width = width;
            return d;
        }

        protected override void clean() {
            base.clean();
            bmp = null;
            channels = null;
            
            //Acquire source
            Address upstream = inputs["inChannels"];
            if (upstream == null) {
                return;
            }
            DataBlob dataIn = upstream.node.getData(upstream.port);
            if (dataIn == null) {
                return;
            }

            //Acquire data from source
            if (dataIn.type == DataBlob.Type.Image) {
                if (dataIn.img == null)
                    return;
                bmpToChannels(dataIn.img);
            } else if (dataIn.type == DataBlob.Type.Channels) {
                if (dataIn.channels == null)
                    return;
                //copy channels to local arrays
                channels = new byte[dataIn.channels.Length][];
                for(int c = 0; c < channels.Length; c++) {
                    channels[c] = (byte[])dataIn.channels[c].Clone();
                    width = dataIn.width;
                }
            }

            //Transform data
            if (dataIn.type == DataBlob.Type.Image) {
                convertSrcImage();
            } else if (dataIn.type == DataBlob.Type.Channels) {
                convertSrcChannels();
            }
        }

        private void bmpToChannels(Bitmap bmp) {
            width = bmp.Width;

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
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.
                    
                    channels[0][iY] = rgbValues[pixel + 2];
                    channels[1][iY] = rgbValues[pixel + 1];
                    channels[2][iY++] = rgbValues[pixel];
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }
        
        private void upsample() {
            //determine existing space
            int size4 = channels[0].Length; //Y channel. Should be full length.
            int height = size4 / width;
            int size2 = (width + 1) / 2 * height;
            int size1 = (width + 1) / 2 * (height + 1) / 2;
            byte[] newG;
            byte[] newB;

            int iNew, iOld;

            switch (inSamples) {
                case Samples.s444:
                    //4:4:4 should have channels 1 and 2 be size4
                    if (channels[1].Length != size4 || channels[2].Length != size4) {
                        channels = null;
                        throw new InvalidCastException("upsample, 444: channel sizes mismatch. ");
                    }
                    break;
                case Samples.s422:
                    //4:2:2 should have channels 1 and 2 be size2
                    if (channels[1].Length != size2 || channels[2].Length != size2) {
                        channels = null;
                        throw new InvalidCastException("upsample, 422: channel sizes mismatch. ");
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
                    if (channels[1].Length != size1 || channels[2].Length != size1) {
                        channels = null;
                        throw new InvalidCastException("upsample, 420: channel sizes mismatch. ");
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
                            if (y % 2 == 0 && x % 2 == 0) {
                                if (iNew != 0)
                                    iOld += (width + 1) / 2;
                            } else if (y % 2 == 1 && x % 2 == 0) {
                                iOld -= (width + 1) / 2;
                            }

                            newG[iNew] = channels[1][iOld];
                            newB[iNew] = channels[2][iOld];


                            //pixel iterates over every pixel in the full size image.
                            if (x % 2 == 0 && y % 2 == 0) {
                                if (x == width - 1)
                                    iOld++;
                            } else if (x % 2 == 1 && y % 2 == 0) {
                                newG[iNew] = channels[1][iOld];
                                newB[iNew] = channels[2][iOld];
                                iOld++;
                            }
                        }
                    }
                    channels[1] = newG;
                    channels[2] = newB;
                    break;
                case Samples.s411:
                    //4:1:1 should have channels 1 and 2 be size1
                    if (channels[1].Length != size1 || channels[2].Length != size1) {
                        channels = null;
                        throw new InvalidCastException("upsample, 411: channel sizes mismatch. ");
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

        private void convertSrcImage() {
            //start with the bmp.
            //lock bmp and break pixels into channels.
            //drop data from g and b channels
            //duplicate g and b channels of bmp to fill gaps
            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            channels = new byte[3][];
            channels[0] = new byte[bmp.Width * bmp.Height]; //1:1 for Y channel
            channels[1] = new byte[((bmp.Width + 1) / 2) * ((bmp.Height + 1) / 2)]; //1:4 for Cr channel
            channels[2] = new byte[((bmp.Width + 1) / 2) * ((bmp.Height + 1) / 2)]; //1:4 for Cb channel
            
            int pixel;
            int iY=0, iCrb=0;

            for (int y = 0; y < bmpData.Height; y++) {
                for (int x = 0; x < bmpData.Width; x++) {
                    pixel = y * bmpData.Stride + x * 3; //assuming 3 channels. Sorry.

                    //Y channel, unchanged
                    channels[0][iY++] = rgbValues[pixel + 2];
                    
                    // a b c
                    // d e f
                    // g h i

                    //case: a
                    if (x % 2 == 0 && y % 2 == 0) {
                        channels[1][iCrb] = rgbValues[pixel + 1];
                        channels[2][iCrb] = rgbValues[pixel];
                        iCrb++;
                    }
                    //case b
                    else if (x % 2 == 1 && y % 2 == 0) {
                        rgbValues[pixel] = rgbValues[pixel - 3]; //grab from pixel to left
                        rgbValues[pixel + 1] = rgbValues[pixel + 1 - 3]; //grab from pixel to left
                    }
                    //case d
                    else if (x % 2 == 0 && y % 2 == 1) {
                        rgbValues[pixel] = rgbValues[pixel - bmpData.Stride]; //grab from pixel above
                        rgbValues[pixel + 1] = rgbValues[pixel + 1 - bmpData.Stride]; //grab from pixel above
                    }
                    //case e
                    else {
                        rgbValues[pixel] = rgbValues[pixel - bmpData.Stride - 3]; //grab from pixel above and left
                        rgbValues[pixel + 1] = rgbValues[pixel + 1 - bmpData.Stride - 3]; //grab from pixel above and left
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);

        }

        private void convertSrcChannels() {

            upsample();
            
            //drop elements as needed
            //create BMP of r dimensions.
            //write channels into bmp, duplicating as needed.



            throw new NotImplementedException();
        }

        public override Bitmap view() {
            base.view();
            if (bmp == null) {
                return null;
            }
            //create bitmap from data
            return bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
        }

        public override Rectangle getExtents() {
            return new Rectangle(-bmp.Width / 2, -bmp.Height / 2, bmp.Width, bmp.Height);
        }
    }
}
