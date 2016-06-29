using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeShop.NodeProperties;
using System.Drawing;

namespace NodeShop.Nodes {
    public class WriteChannels : Node {
        protected string outPath;
        //byte values are -128 to 127, but stored in a uint type
        protected const byte rleToken = 128;

        public WriteChannels(): base() { }
        public WriteChannels(NodeView graph) : base(graph) { }
        public WriteChannels(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }

        protected override void init() {
            base.init();
            setPath("C:\\temp\\testfile.dct");
            rename("WriteChannels");
        }

        public void setPath(string path) {
            outPath = path;
            properties["path"].sValue = path;

            int lastSlash = path.LastIndexOf('\\') + 1;
            lastSlash = lastSlash == -1 ? 0 : lastSlash;

            setExtra(path.Substring(lastSlash));
        }

        private void pathChanged(object sender, EventArgs e) {
            setPath(properties["path"].sValue);
        }

        protected override void createProperties() {
            base.createProperties();

            //create filepath property
            Property p = new PropertyString("", "Image path to save");
            p.eValueChanged += pathChanged;
            properties.Add("path", p);

            p = new PropertyButton("Save", "save image to file");
            p.eValueChanged += save;
            properties.Add("save", p);

            p = new PropertyButton("Check", "check file stats");
            p.eValueChanged += check;
            properties.Add("check", p);

            properties.Add("inChannels", new PropertyChannels(true, false));
        }

        protected virtual void check(object sender, EventArgs e) {
            int read_width = 0;
            int read_height = 0;
            int read_cwidth = 0;
            int read_cheight = 0;
            int read_quality = 0;
            int read_samples = 0;

            using (Stream stream = new FileStream(outPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.Default)) {
                    read_width = reader.ReadUInt16();
                    read_height = reader.ReadUInt16();
                    read_cwidth = reader.ReadUInt16();
                    read_cheight = reader.ReadUInt16();
                    read_quality = reader.ReadByte();
                    read_samples = reader.ReadByte();
                }
            }

            System.Windows.Forms.MessageBox.Show(
                "image width: " + state.imageWidth + " = " + read_width +
                "\nimage height: " + state.imageHeight + " = " + read_height +
                "\nchannel width: " + state.channelWidth + " = " + read_cwidth +
                "\nchannel height: " + state.channelHeight + " = " + read_cheight +
                "\nquality: " + state.quantizeQuality + " = " + read_quality +
                "\nsamples: " + state.samplingMode + " = " + (DataBlob.Samples)read_samples
                , "File Information");

        }

        protected virtual void save(object sender, EventArgs e) {
            if (!readAndValidateInput()) {
                return;
            }

            using (Stream stream = new BufferedStream(new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None))) {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default)) {
                    writeHeader(writer, state);
                    writeChannels(writer, state);
                }
            }

            
            long sizeAll = new FileInfo(outPath).Length;
            int sizeBmp = (state.imageHeight * state.imageWidth * 3);

            String msgBox = String.Format("Image as bitmap: {0} Bytes\nImage compressed: {1} Bytes\nCompression ratio: {2:0.00} : 1\nor {3:0.00}% smaller.",
                sizeBmp,
                sizeAll,
                (double)sizeBmp / sizeAll,
                ((double)sizeBmp - sizeAll) / sizeBmp * 100);

            System.Windows.Forms.MessageBox.Show(msgBox, "Compression Info");
        }

        protected static void writeChannels(BinaryWriter writer, DataBlob ch) {
            Chunker c = new Chunker(8, ch.channelWidth, ch.channelHeight, ch.channelWidth, 1);
            writeChannel(writer, ch.channels[0], c);
            Size s = Subsample.deduceCbCrSize(ch);
            c = new Chunker(8, s.Width, s.Height, s.Width, 1);
            writeChannel(writer, ch.channels[1], c);
            writeChannel(writer, ch.channels[2], c);
        }

        protected static void writeChannel(BinaryWriter writer, byte[] channel, Chunker c) {
            byte count, prev, val;
            var indexer = Chunker.zigZag8Index();
            byte[] data = new byte[64];
            for (int i = 0; i < c.getNumChunks(); i++) {
                c.getBlock(channel, data, i);
                count = 0;
                prev = data[0];
                foreach (int index in indexer) {
                    val = data[index];
                    if (val == prev) {
                        count++;
                    } else {
                        if (prev == rleToken || count >= 3) {
                            writer.Write(rleToken);
                            writer.Write(count);
                        } else if (count == 2) {
                            writer.Write((prev));
                        }
                        writer.Write((prev));
                        prev = val;
                        count = 1;
                    }
                }
                //write out the last token
                if (prev == rleToken || count >= 3) {
                    writer.Write(rleToken);
                    writer.Write(count);
                } else if (count == 2) {
                    writer.Write(prev);
                }
                writer.Write(prev);
                //final chunk written out
            } //channel written out
        }

        protected static void writeHeader(BinaryWriter writer, DataBlob info) {
            writer.Write((short)info.imageWidth);
            writer.Write((short)info.imageHeight);
            writer.Write((short)info.channelWidth);
            writer.Write((short)info.channelHeight);
            writer.Write((byte)info.quantizeQuality);
            writer.Write((byte)info.samplingMode);
        }

        private bool readAndValidateInput() {
            Address upstream = properties["inChannels"].input;
            if (upstream == null) return false;

            state = upstream.node.getData(upstream.port);

            if (state == null || state.type != DataBlob.Type.Channels || state.channels == null)
                return false;
            
            return true;
        }

        private void save_old(object sender, EventArgs e) {
            clean();

            if (state == null || state.channels == null) {
                return;
            }
            //data to save: image width, image height, quality factor, y channel, Cr channel, Cb channel
            //perform zig-zag RLE on channels.
            //
            //16 bits for width
            //16 bits for height
            // 8 bits for quality factor
            // 8 bits for channel sampling mode
            //[width x height] bytes for y channel (uncompressed)
            //[(width + 1) / 2 + (height + 1) / 2] bytes for Cr channel
            //[(width + 1) / 2 + (height + 1) / 2] bytes for Cb channel

            //RLE:
            //for each channel
            //  for each chunk
            //    for each value
            //      value != prev: 
            //        prev == token || 
            //        count >= 3: write token, write count, write prev, prev = value, count = 1
            //        count == 2: write prev, write prev, prev = value, count = 1
            //        count == 1: write prev, prev = value
            //      vale == prev:
            //        count++
            //    write prev as above
            //    next chunk...

            byte prev, count, val;
            using (Stream stream = new BufferedStream(new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None))) {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default)) {
                    writer.Write((short)state.imageWidth);
                    writer.Write((short)state.imageHeight);
                    writer.Write((short)state.channelWidth);
                    writer.Write((short)state.channelHeight);
                    writer.Write((byte)state.quantizeQuality);
                    writer.Write((byte)state.samplingMode);
                    byte[] data = new byte[64];
                    Chunker c = new Chunker(8, state.channelWidth, state.channelHeight, state.channelWidth, 1);
                    var indexer = Chunker.zigZag8Index();
                    for (int i = 0; i < c.getNumChunks(); i++) {
                        c.getBlock(state.channels[0], data, i);
                        count = 0;
                        prev = data[0];
                        foreach (int index in indexer) {
                            val = data[index];
                            if (val == prev) {
                                count++;
                            } else {
                                if (prev == rleToken || count >= 3) {
                                    writer.Write(rleToken);
                                    writer.Write(count);
                                    writer.Write(prev);
                                } else if (count == 2) {
                                    writer.Write(prev);
                                    writer.Write(prev);
                                } else {
                                    writer.Write(prev);
                                }
                                prev = val;
                                count = 1;
                            }
                        }
                        //write out the last prev
                        if (prev == rleToken || count >= 3) {
                            writer.Write(rleToken);
                            writer.Write(count);
                            writer.Write(prev);
                        } else if (count == 2) {
                            writer.Write(prev);
                            writer.Write(prev);
                        } else {
                            writer.Write(prev);
                        } //chunk written out
                    } //channel written out

                    //

                    switch (state.samplingMode) {
                        case DataBlob.Samples.s444:
                            //just use the existing chunker
                            break;
                        case DataBlob.Samples.s411:
                            c = new Chunker(8, (state.channelWidth + 3) / 4, state.channelHeight, (state.channelWidth + 3) / 4, 1);
                            break;
                        case DataBlob.Samples.s420:
                            c = new Chunker(8, (state.channelWidth + 1) / 2, (state.channelHeight + 1) / 2, (state.channelWidth + 1) / 2, 1);
                            break;
                        case DataBlob.Samples.s422:
                            c = new Chunker(8, (state.channelWidth + 1) / 2, state.channelHeight, (state.channelWidth + 1) / 2, 1);
                            break;
                    }
                    indexer = Chunker.zigZag8Index();
                    for (int channel = 1; channel < state.channels.Length; channel++) {
                        for (int i = 0; i < c.getNumChunks(); i++) {
                            c.getBlock(state.channels[channel], data, i);
                            count = 0;
                            prev = data[0];
                            foreach (int index in indexer) {
                                val = data[index];
                                if (val == prev) {
                                    count++;
                                } else {
                                    if (prev == rleToken || count >= 3) {
                                        writer.Write(rleToken);
                                        writer.Write(count);
                                        writer.Write(prev);
                                    } else if (count == 2) {
                                        writer.Write(prev);
                                        writer.Write(prev);
                                    } else {
                                        writer.Write(prev);
                                    }
                                    prev = val;
                                    count = 1;
                                }
                            }
                            //write out the last prev
                            if (prev == rleToken || count >= 3) {
                                writer.Write(rleToken);
                                writer.Write(count);
                                writer.Write(prev);
                            } else if (count == 2) {
                                writer.Write(prev);
                                writer.Write(prev);
                            } else {
                                writer.Write(prev);
                            } //chunk written out
                        } //channel written out
                    } // all channels written out
                }
            }

            FileInfo fi = new FileInfo(outPath);

            String msgBox = String.Format("Image as bitmap: {0} Bytes\nImage compressed: {1} Bytes\nCompression ratio: {2:0.00} : 1\nor {3:0.00}% smaller.",
                (state.imageHeight * state.imageWidth * 3),
                fi.Length,
                (state.imageHeight * state.imageWidth * 3.0) / fi.Length,
                ((state.imageHeight * state.imageWidth * 3.0) - fi.Length) / (state.imageHeight * state.imageWidth * 3.0) * 100);

            System.Windows.Forms.MessageBox.Show(msgBox, "Compression Info");
        }

    }
}
