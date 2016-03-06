using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.NodeProperties;

namespace MpegCompressor.Nodes {
    class WriteMulti2Channel : Node {
        private const byte rleToken = 128;
        private string outPath;
        DataBlob inC1, inC2, inV2;

        public WriteMulti2Channel() : base() { }
        public WriteMulti2Channel(NodeView graph) : base(graph) { }
        public WriteMulti2Channel(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("Write Multi");
            setExtra("2x channels");
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertyString("", "Image path to save");
            p.eValueChanged += pathChanged;
            properties.Add("path", p);

            p = new PropertyButton("Save", "save image to file");
            p.eValueChanged += save;
            properties.Add("save", p);

            p = new PropertyButton("Check", "check file stats");
            p.eValueChanged += check;
            properties.Add("check", p);

            properties.Add("inChannels1", new PropertyChannels(true, false));
            properties.Add("inVectors2", new PropertyVectors(true, false));
            properties.Add("inChannels2", new PropertyChannels(true, false));
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
                "image width: " + inC1.imageWidth + " = " + read_width +
                "\nimage height: " + inC1.imageHeight + " = " + read_height +
                "\nchannel width: " + inC1.channelWidth + " = " + read_cwidth +
                "\nchannel height: " + inC1.channelHeight + " = " + read_cheight +
                "\nquality: " + inC1.quantizeQuality + " = " + read_quality +
                "\nsamples: " + inC1.samplingMode + " = " + (DataBlob.Samples)read_samples
                , "File Information");
        }

        private void save(object sender, EventArgs e) {
            if (!validateInput()) {
                return;
            }

            using (Stream stream = new BufferedStream(new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None))) {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default)) {
                    writeHeader(writer);
                    writeChannels(writer, inC1);
                }
            }
            long sizeIFrame = (new FileInfo(outPath)).Length;
            using (Stream stream = new BufferedStream(new FileStream(outPath, FileMode.Append, FileAccess.Write, FileShare.None))) {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default)) {
                    writeChannels(writer, inV2);
                    writeChannels(writer, inC2);
                }
            }

            long sizeAll = new FileInfo(outPath).Length;
            int bitmaps = inC1.imageHeight * inC2.imageWidth * 6;

            String msgBox = String.Format(
                "2 Frames as bitmaps: {0} Bytes\n"
                + "2 Frames as 2 jpegs: {1} Bytes\n"
                + "2 Frames as mpeg: {2} Bytes\n"
                + "Bitmap to mpeg: {3:0.00} : 1\n"
                + "mpeg is {4:0.00}% smaller than bitmaps.",
                bitmaps,
                sizeIFrame * 2,
                sizeAll,
                (double)bitmaps / sizeAll,
                (double)(bitmaps - sizeAll) / bitmaps * 100);

            System.Windows.Forms.MessageBox.Show(msgBox, "Compression Info");
        }

        private void writeChannels(BinaryWriter writer, DataBlob ch) {
            Chunker c = new Chunker(8, ch.channelWidth, ch.channelHeight, ch.channelWidth, 1);
            writeChannel(writer, ch.channels[0], c);
            Size s = Subsample.deduceCbCrSize(ch);
            c = new Chunker(8, s.Width, s.Height, s.Width, 1);
            writeChannel(writer, ch.channels[1], c);
            writeChannel(writer, ch.channels[2], c);
        }

        private void writeChannel(BinaryWriter writer, byte[] channel, Chunker c) {
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

        private void writeHeader(BinaryWriter writer) {
            writer.Write((short)inC1.imageWidth);
            writer.Write((short)inC1.imageHeight);
            writer.Write((short)inC1.channelWidth);
            writer.Write((short)inC1.channelHeight);
            writer.Write((byte)inC1.quantizeQuality);
            writer.Write((byte)inC1.samplingMode);
        }

        private bool validateInput() {
            /*
            properties.Add("inChannels1", new Property(true, false));
            properties.Add("inVectors2",  new Property(true, false));
            properties.Add("inChannels2", new Property(true, false));
            properties.Add("inVectors3",  new Property(true, false));
            properties.Add("inChannels3", new Property(true, false));
            */
            Address upC1 = properties["inChannels1"].input;
            if (upC1 == null) return false;
            Address upC2 = properties["inChannels2"].input;
            if (upC2 == null) return false;
            Address upV2 = properties["inVectors2"].input;
            if (upV2 == null) return false;

            inC1 = upC1.node.getData(upC1.port);
            inC2 = upC2.node.getData(upC2.port);
            inV2 = upV2.node.getData(upV2.port);

            if (inC1 == null || inC1.type != DataBlob.Type.Channels || inC1.channels == null)
                return false;
            if (inC2 == null || inC2.type != DataBlob.Type.Channels || inC2.channels == null)
                return false;
            if (inV2 == null || inV2.type != DataBlob.Type.Vectors || inV2.channels == null)
                return false;

            if (inC1.channels[0].Length != inC2.channels[0].Length ||
                inC1.channels[1].Length != inC2.channels[1].Length || 
                inC1.channels[2].Length != inC2.channels[2].Length) {
                return false;
            }

            return true;
        }

    }
}
