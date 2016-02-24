using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    class WriteMultiChannel : Node {
        private string outPath;

        public WriteMultiChannel(): base() { }
        public WriteMultiChannel(NodeView graph) : base(graph) { }
        public WriteMultiChannel(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            rename("Write Multi");
            setExtra("3x channels");
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new Property(false, false);
            p.createString("", "Image path to save");
            p.eValueChanged += pathChanged;
            properties.Add("path", p);

            p = new Property(false, false);
            p.createButton("Save", "save image to file");
            //p.eValueChanged += save;
            properties.Add("save", p);

            p = new Property(false, false);
            p.createButton("Check", "check file stats");
            p.eValueChanged += check;
            properties.Add("check", p);

            properties.Add("inChannels1", new Property(true, false));
            properties.Add("inVectors2",  new Property(true, false));
            properties.Add("inChannels2", new Property(true, false));
            properties.Add("inVectors3",  new Property(true, false));
            properties.Add("inChannels3", new Property(true, false));
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
                "\nquality: " + state.quantizeQuality + " = " + read_quality +
                "\nsamples: " + state.samplingMode + " = " + (DataBlob.Samples)read_samples
                , "File Information");

        }
    }
}
