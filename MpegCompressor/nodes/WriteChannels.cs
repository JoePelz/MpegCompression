using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class WriteChannels : ChannelNode {
        private string outPath;
        //byte values are -128 to 127, but stored in a uint type
        private byte rleToken = 128;

        public WriteChannels() {
            setPath("C:\\temp\\testfile.dct");
        }
        
        protected override void createOutputs() {
            //went back and forth on this, but decided to remove output
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("WriteChannels");

            //create filepath property
            p = new Property();
            p.createString("", "Image path to save");
            p.eValueChanged += pathChanged;
            properties.Add("path", p);

            p = new Property();
            p.createButton("Save", "save image to file");
            p.eValueChanged += save;
            properties.Add("save", p);

            p = new Property();
            p.createButton("Check", "check file stats");
            p.eValueChanged += check;
            properties.Add("check", p);
        }

        private void check(object sender, EventArgs e) {
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
                "\nquality: " + 130 + " = " + read_quality +
                "\nsamples: " + state.samplingMode + " = " + (Subsample.Samples)read_samples
                , "File Information");

        }

        private void save(object sender, EventArgs e) {
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
                    writer.Write((byte)130);
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
                        case Subsample.Samples.s411:
                            c = new Chunker(8, (state.channelWidth + 3) / 4, state.channelHeight, (state.channelWidth + 3) / 4, 1);
                            break;
                        case Subsample.Samples.s420:
                            c = new Chunker(8, (state.channelWidth + 1) / 2, (state.channelHeight + 1) / 2, (state.channelWidth + 1) / 2, 1);
                            break;
                        case Subsample.Samples.s422:
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
        }

        public void setPath(string path) {
            outPath = path;
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
