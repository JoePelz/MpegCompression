using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    class ReadChannels : Node {
        private string inPath;
        //byte values are -128 to 127, but stored in a uint type
        private byte rleToken = 128;

        public ReadChannels() {
            rename("ReadChannels");
            setPath("C:\\temp\\testfile.dct");
        }

        protected override void createProperties() {
            base.createProperties();

            //create filepath property
            Property p = new Property(false, false);
            p.createString("", "Image path to save");
            p.eValueChanged += pathChanged;
            properties.Add("path", p);

            p = new Property(false, false);
            p.createButton("open", "load channels from file");
            p.eValueChanged += open;
            properties.Add("open", p);

            p = new Property(false, false);
            p.createButton("info", "stats on the file read");
            p.eValueChanged += (a, b) => { check();  };
            properties.Add("info", p);

            properties.Add("outChannels", new Property(false, true));
        }

        protected override void clean() {
            base.clean();
        }

        private void check() {
            if (state == null) {
                System.Windows.Forms.MessageBox.Show("No state");
                return;
            }

            System.Windows.Forms.MessageBox.Show(
                "image width: " + state.imageWidth +
                "\nimage height: " + state.imageHeight + 
                "\nchannel width: " + state.channelWidth + 
                "\nchannel height: " + state.channelHeight +
                "\nquality setting: " + state.quantizeQuality +
                "\nsampling mode: " + state.samplingMode
                , "File Information");
        }


        private void open(object sender, EventArgs e) {
            state = new DataBlob();
            state.type = DataBlob.Type.Channels;
            state.channels = new byte[3][];
            using (Stream stream = new FileStream(inPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.Default)) {
                    state.imageWidth = reader.ReadUInt16();
                    state.imageHeight = reader.ReadUInt16();
                    state.channelWidth = reader.ReadUInt16();
                    state.channelHeight = reader.ReadUInt16();
                    state.quantizeQuality = reader.ReadByte();
                    state.samplingMode = (DataBlob.Samples)reader.ReadByte();
                    
                    state.channels[0] = new byte[state.channelWidth * state.channelHeight];

                    byte[] data = new byte[64];
                    byte count, val;

                    //======================
                    //===== Y Channel ======
                    //======================
                    Chunker c = new Chunker(8, state.channelWidth, state.channelHeight, state.channelWidth, 1);
                    var indexer = Chunker.zigZag8Index();
                    for (int iChunk = 0; iChunk < c.getNumChunks(); iChunk++) {
                        for (int iPixel = 0; iPixel < 64;) {
                            val = reader.ReadByte();
                            if (val != rleToken) {
                                data[iPixel++] = val;
                            } else {
                                count = reader.ReadByte();
                                val = reader.ReadByte();
                                while (count > 0) {
                                    data[iPixel++] = val;
                                    count--;
                                }
                            }
                        }
                        c.setZigZag8Block(state.channels[0], data, iChunk);
                    }

                    //===========================
                    //===== Cr, Cb Channels =====
                    //===========================
                    Size len = Subsample.getPaddedCbCrSize(new Size(state.channelWidth, state.channelHeight), state.samplingMode);
                    state.channels[1] = new byte[len.Width * len.Height];
                    state.channels[2] = new byte[state.channels[1].Length];
                    c = new Chunker(8, len.Width, len.Height, len.Width, 1);
                    
                    indexer = Chunker.zigZag8Index();
                    for (int channel = 1; channel < state.channels.Length; channel++) {
                        for (int iChunk = 0; iChunk < c.getNumChunks(); iChunk++) {
                            for (int iPixel = 0; iPixel < 64;) {
                                val = reader.ReadByte();
                                if (val != rleToken) {
                                    data[iPixel++] = val;
                                } else {
                                    count = reader.ReadByte();
                                    val = reader.ReadByte();
                                    while (count > 0) {
                                        data[iPixel++] = val;
                                        count--;
                                    }
                                }
                            }
                            c.setZigZag8Block(state.channels[channel], data, iChunk);
                        }
                    }
                }
            } //close file
            soil();
        }

        public void setPath(string path) {
            inPath = path;
            properties["path"].setString(path);

            int lastSlash = path.LastIndexOf('\\') + 1;
            lastSlash = lastSlash == -1 ? 0 : lastSlash;

            setExtra(path.Substring(lastSlash));
        }

        private void pathChanged(object sender, EventArgs e) {
            setPath(properties["path"].getString());
        }
    }
}
