using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class ReadChannels : Node {
        private string inPath;
        //byte values are -128 to 127, but stored in a uint type
        private byte rleToken = 128;

        public ReadChannels() {
            rename("ReadChannels");
            setPath("C:\\temp\\testfile.dct");
        }
        
        protected override void createOutputs() {
            outputs.Add("outChannels", new HashSet<Address>());
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("WriteImage");

            //create filepath property
            p = new Property();
            p.createString("", "Image path to save");
            p.eValueChanged += pathChanged;
            properties.Add("path", p);

            p = new Property();
            p.createButton("open", "load channels from file");
            p.eValueChanged += open;
            properties.Add("open", p);
        }

        protected override void clean() {
            base.clean();

            /*
            Chunker c = new Chunker(8, 8, 8, 8, 1);
            byte[] testData = {
              0,  1,  2,  3,  4,  5,  6,  7,
              8,  9, 10, 11, 12, 13, 14, 15,
             16, 17, 18, 19, 20, 21, 22, 23,
             24, 25, 26, 27, 28, 29, 30, 31,
             32, 33, 34, 35, 36, 37, 38, 39,
             40, 41, 42, 43, 44, 45, 46, 47,
             48, 49, 50, 51, 52, 53, 54, 55,
             56, 57, 58, 59, 60, 61, 62, 63
            };
            byte[] xform = new byte[64];
            c.getZigZag8Block(testData, xform, 0);
            byte[] result = new byte[64];
            c.setZigZag8Block(result, 0, xform);
            */
        }

        private void open(object sender, EventArgs e) {
            state = new DataBlob();
            state.type = DataBlob.Type.Channels;
            state.channels = new byte[3][];
            using (Stream stream = new FileStream(inPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.Default)) {
                    state.width = reader.ReadUInt16();
                    state.height = reader.ReadUInt16();
                    reader.ReadByte(); //quality doesn't matter yet.
                    state.samplingMode = (Subsample.Samples)reader.ReadByte();
                    
                    state.channels[0] = new byte[state.width * state.height];

                    byte[] data = new byte[64];
                    byte count, val;

                    //======================
                    //===== Y Channel ======
                    //======================
                    Chunker c = new Chunker(8, state.width, state.height, state.width, 1);
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
                    switch (state.samplingMode) {
                        case Subsample.Samples.s411:
                            state.channels[1] = new byte[(state.width + 3) / 4 * state.height];
                            c = new Chunker(8, (state.width + 3) / 4, state.height, (state.width + 3) / 4, 1);
                            break;
                        case Subsample.Samples.s420:
                            state.channels[1] = new byte[(state.width + 1) / 2 * (state.height + 1) / 2];
                            c = new Chunker(8, (state.width + 1) / 2, (state.height + 1) / 2, (state.width + 1) / 2, 1);
                            break;
                        case Subsample.Samples.s422:
                            state.channels[1] = new byte[(state.width + 1) / 2 * state.height];
                            c = new Chunker(8, (state.width + 1) / 2, state.height, (state.width + 1) / 2, 1);
                            break;
                        case Subsample.Samples.s444:
                            state.channels[1] = new byte[state.channels[0].Length];
                            break;
                    }
                    state.channels[2] = new byte[state.channels[1].Length];

                    
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
