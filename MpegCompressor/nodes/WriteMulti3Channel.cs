using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.NodeProperties;

namespace MpegCompressor.Nodes {
    class WriteMulti3Channel : WriteChannels {
        DataBlob inC1, inC2, inC3, inV2, inV3;

        public WriteMulti3Channel(): base() { }
        public WriteMulti3Channel(NodeView graph) : base(graph) { }
        public WriteMulti3Channel(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("Write Multi");
            setExtra("3x channels");
        }

        protected override void createProperties() {
            base.createProperties();
            properties.Add("inVectors2",  new PropertyVectors(true, false));
            properties.Add("inChannels2", new PropertyChannels(true, false));
            properties.Add("inVectors3",  new PropertyVectors(true, false));
            properties.Add("inChannels3", new PropertyChannels(true, false));
        }

        protected override void check(object sender, EventArgs e) {
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

        protected override void save(object sender, EventArgs e) {
            if (!validateInput()) {
                return;
            }

            using (Stream stream = new BufferedStream(new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None))) {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default)) {
                    writeHeader(writer, inC1);
                    writeChannels(writer, inC1);
                }
            }

            long sizeIFrame = (new FileInfo(outPath)).Length;

            using (Stream stream = new BufferedStream(new FileStream(outPath, FileMode.Append, FileAccess.Write, FileShare.None))) {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Default)) {
                    writeChannels(writer, inV2);
                    writeChannels(writer, inC2);
                    writeChannels(writer, inV3);
                    writeChannels(writer, inC3);
                }
            }


            long sizeAll = new FileInfo(outPath).Length;
            int bitmaps = inC1.imageHeight * inC1.imageWidth * 9;

            String msgBox = String.Format(
                "3 Frames as bitmaps: {0} Bytes\n"
                + "3 Frames as 3 jpegs: {1} Bytes\n"
                + "3 Frames as mpeg: {2} Bytes\n"
                + "Bitmap to mpeg: {3:0.00} : 1\n"
                + "mpeg is {4:0.00}% smaller than bitmaps.",
                bitmaps,
                sizeIFrame * 3,
                sizeAll,
                (double)bitmaps / sizeAll,
                (double)(bitmaps - sizeAll) / bitmaps * 100);

            System.Windows.Forms.MessageBox.Show(msgBox, "Compression Info");
        }
        
        private bool validateInput() {
            Address upC1 = properties["inChannels"].input;
            if (upC1 == null) return false;
            Address upC2 = properties["inChannels2"].input;
            if (upC2 == null) return false;
            Address upC3 = properties["inChannels3"].input;
            if (upC3 == null) return false;
            Address upV2 = properties["inVectors2"].input;
            if (upV2 == null) return false;
            Address upV3 = properties["inVectors3"].input;
            if (upV3 == null) return false;

            inC1 = upC1.node.getData(upC1.port);
            inC2 = upC2.node.getData(upC2.port);
            inC3 = upC3.node.getData(upC3.port);
            inV2 = upV2.node.getData(upV2.port);
            inV3 = upV3.node.getData(upV3.port);

            if (inC1 == null || inC1.type != DataBlob.Type.Channels || inC1.channels == null)
                return false;
            if (inC2 == null || inC2.type != DataBlob.Type.Channels || inC2.channels == null)
                return false;
            if (inC3 == null || inC3.type != DataBlob.Type.Channels || inC3.channels == null)
                return false;
            if (inV2 == null || inV2.type != DataBlob.Type.Vectors || inV2.channels == null)
                return false;
            if (inV3 == null || inV3.type != DataBlob.Type.Vectors || inV3.channels == null)
                return false;

            if (inC1.channels[0].Length != inC2.channels[0].Length || inC1.channels[0].Length != inC3.channels[0].Length ||
                inC1.channels[1].Length != inC2.channels[1].Length || inC1.channels[1].Length != inC3.channels[1].Length ||
                inC1.channels[2].Length != inC2.channels[2].Length || inC1.channels[2].Length != inC3.channels[2].Length) {
                return false;
            }
            if (inV2.channels[0].Length != inV3.channels[0].Length ||
                inV2.channels[1].Length != inV3.channels[1].Length ||
                inV2.channels[2].Length != inV3.channels[2].Length) {
                return false;
            }

            return true;
        }

    }
}
