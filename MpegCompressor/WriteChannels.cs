using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public class WriteChannels : ChannelNode {
        private string outPath;
        private char rleToken;

        public WriteChannels() {
            setPath("C:\\temp\\testfile.dct");
        }
        
        protected override void createOutputs() {
            //went back and forth on this, but decided to remove output
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
            p.createButton("Save", "save image to file");
            p.eValueChanged += save;
            properties.Add("save", p);
        }

        private void save(object sender, EventArgs e) {
            //data to save: image width, image height, quality factor, y channel, Cr channel, Cb channel
            //perform RLE on channels.
            //
            //16 bits for width
            //16 bits for height
            // 8 bits for quality factor
            // 8 bits for channel sampling mode
            //[width x height] bytes for y channel (uncompressed)
            //[(width + 1) / 2 + (height + 1) / 2] bytes for Cr channel
            //[(width + 1) / 2 + (height + 1) / 2] bytes for Cb channel
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
