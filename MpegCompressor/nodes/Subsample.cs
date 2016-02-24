using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class Subsample : ChannelNode {
        private static string[] options
            = new string[4] {
                "4:4:4",
                "4:2:2",
                "4:1:1",
                "4:2:0" };
        private DataBlob.Samples outSamples;

        public Subsample(): base() { }
        public Subsample(NodeView graph) : base(graph) { }
        public Subsample(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("Subsample");
        }

        public static Size getCbCrSize(Size ySize, DataBlob.Samples mode) {
            switch (mode) {
                case DataBlob.Samples.s444:
                    break;
                case DataBlob.Samples.s422:
                    ySize.Width = (ySize.Width + 1) / 2;
                    break;
                case DataBlob.Samples.s420:
                    ySize.Width = (ySize.Width + 1) / 2;
                    ySize.Height = (ySize.Height + 1) / 2;
                    break;
                case DataBlob.Samples.s411:
                    ySize.Width = (ySize.Width + 3) / 4;
                    break;
            }
            return ySize;
        }

        public static Size getPaddedCbCrSize(Size ySize, DataBlob.Samples mode) {
            switch (mode) {
                case DataBlob.Samples.s444:
                    break;
                case DataBlob.Samples.s422:
                    ySize.Width = (ySize.Width + 1) / 2;
                    break;
                case DataBlob.Samples.s420:
                    ySize.Width = (ySize.Width + 1) / 2;
                    ySize.Height = (ySize.Height + 1) / 2;
                    break;
                case DataBlob.Samples.s411:
                    ySize.Width = (ySize.Width + 3) / 4;
                    break;
            }
            if (ySize.Width % 8 != 0) {
                ySize.Width += 8 - (ySize.Width % 8);
            }
            if (ySize.Height % 8 != 0) {
                ySize.Height += 8 - (ySize.Height % 8);
            }
            return ySize;
        }

        public static Size deduceCbCrSize(DataBlob data) {
            Size size = getCbCrSize(new Size(data.channelWidth, data.channelHeight), data.samplingMode);
            if (data.channels[1].Length == size.Width * size.Height) {
                return size;
            }
            size = getPaddedCbCrSize(new Size(data.channelWidth, data.channelHeight), data.samplingMode);
            if (data.channels[1].Length == size.Width * size.Height) {
                return size;
            }
            size.Width = 0;
            size.Height = 0;
            return size;
        }

        public static int getCbCrStride(DataBlob data) {
            Size unpadded = getCbCrSize(new Size(data.channelWidth, data.channelHeight), data.samplingMode);
            if (data.channels[1].Length == unpadded.Width * unpadded.Height) {
                return unpadded.Width;
            }
            Size padded = getPaddedCbCrSize(new Size(data.channelWidth, data.channelHeight), data.samplingMode);
            if (data.channels[1].Length == padded.Width * padded.Height) {
                return padded.Width;
            }
            return -1;
        }

        protected override void createProperties() {
            base.createProperties();
            
            //Will need to choose output sample space.
            Property p = new Property(false, false);
            p.createChoices(options, (int)outSamples, "output sample space");
            p.eValueChanged += P_eValueChanged;
            properties["outSamples"] = p;
        }

        public void setOutSamples(DataBlob.Samples samples) {
            outSamples = samples;
            properties["outSamples"].setSelection((int)samples);
            setExtra(options[(int)samples] + " to " + options[(int)outSamples]);
            soil();
        }
        
        private void P_eValueChanged(object sender, EventArgs e) {
            setOutSamples((DataBlob.Samples)properties["outSamples"].getSelection());
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
            int size444 = state.channels[0].Length; //Y channel. Should be full length.
            int size422 = (state.channelWidth + 1) / 2 * state.channelHeight;
            int size420 = (state.channelWidth + 1) / 2 * (state.channelHeight + 1) / 2;
            int size411 = (state.channelWidth + 3) / 4 * state.channelHeight;

            byte[] newG;
            byte[] newB;

            int iNew, iOld;

            switch (state.samplingMode) {
                case DataBlob.Samples.s444:
                    //4:4:4 should have channels 1 and 2 be size4
                    if (state.channels[1].Length != size444 || state.channels[2].Length != size444) {
                        state.channels = null;
                        return;
                    }
                    break;
                case DataBlob.Samples.s422:
                    //4:2:2 should have channels 1 and 2 be size422
                    if (state.channels[1].Length != size422 || state.channels[2].Length != size422) {
                        state.channels = null;
                        return;
                    }
                    newG = new byte[size444];
                    newB = new byte[size444];
                    iOld = 0;
                    for (int y = 0; y < state.channelHeight; y++) {
                        for (int x = 0; x < state.channelWidth; x++) {
                            iNew = y * state.channelWidth + x;
                            //pixel iterates over every pixel in the full size image.
                            if (x % 2 == 0) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                if (x == state.channelWidth - 1)
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
                case DataBlob.Samples.s420:
                    //4:2:0 should have channels 1 and 2 be size1
                    if (state.channels[1].Length != size420 || state.channels[2].Length != size420) {
                        state.channels = null;
                        return;
                    }
                    newG = new byte[size444];
                    newB = new byte[size444];
                    iOld = -1;
                    for (int y = 0; y < state.channelHeight; y++) {
                        for (int x = 0; x < state.channelWidth; x++) {
                            iNew = y * state.channelWidth + x;

                            if (x % 2 == 0) {
                                iOld++;
                            }
                            if (y % 2 == 1 && x == 0) {
                                iOld -= (state.channelWidth + 1) / 2;
                            }

                            newG[iNew] = state.channels[1][iOld];
                            newB[iNew] = state.channels[2][iOld];
                        }
                    }
                    state.channels[1] = newG;
                    state.channels[2] = newB;
                    break;
                case DataBlob.Samples.s411:
                    //4:1:1 should have channels 1 and 2 be size411
                    if (state.channels[1].Length != size411 || state.channels[2].Length != size411) {
                        state.channels = null;
                        return;
                    }
                    newG = new byte[size444];
                    newB = new byte[size444];
                    iOld = 0;
                    for (int y = 0; y < state.channelHeight; y++) {
                        for (int x = 0; x < state.channelWidth; x++) {
                            iNew = y * state.channelWidth + x;
                            //pixel iterates over every pixel in the full size image.
                            if (x % 4 < 3) {
                                newG[iNew] = state.channels[1][iOld];
                                newB[iNew] = state.channels[2][iOld];
                                if (x == state.channelWidth - 1)
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
            int size444 = state.channels[0].Length; //Y channel. Should be full length.
            int size422 = ((state.channelWidth + 1) / 2) * state.channelHeight;
            int size420 = ((state.channelWidth + 1) / 2) * ((state.channelHeight + 1) / 2);
            int size411 = ((state.channelWidth + 3) / 4) * state.channelHeight;
            int iNew, iOld;
            byte[] newG;
            byte[] newB;

            switch (outSamples) {
                case DataBlob.Samples.s444:
                    break; //done! :D
                case DataBlob.Samples.s422:
                    newG = new byte[size422];
                    newB = new byte[size422];
                    iNew = 0;
                    for (int y = 0; y < state.channelHeight; y++) {
                        for (int x = 0; x < state.channelWidth; x+= 2) {
                            iOld = y * state.channelWidth + x;
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
                case DataBlob.Samples.s420:
                    newG = new byte[size420];
                    newB = new byte[size420];
                    iNew = 0;
                    for (int y = 0; y < state.channelHeight; y += 2) {
                        for (int x = 0; x < state.channelWidth; x += 2) {
                            iOld = y * state.channelWidth + x;
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
                case DataBlob.Samples.s411:
                    newG = new byte[size411];
                    newB = new byte[size411];
                    iNew = 0;
                    for (int y = 0; y < state.channelHeight; y++) {
                        for (int x = 0; x < state.channelWidth; x += 4) {
                            iOld = y * state.channelWidth + x;
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
        }
        
    }
}
