using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace MpegCompressor {
    public class ReadImage : Node {
        private string filepath;

        public ReadImage() {
            setPath("C:\\temp\\uv.jpg");
        }

        protected override void createProperties() {
            Property p = properties["name"];
            p.setString("ReadImage");

            //create filepath property
            p = new Property();
            p.createString("", "Image file to open");
            p.eValueChanged += OnPathChange;
            properties.Add("path", p);
        }

        protected override void createInputs() {
            //no inputs for ReadImage. Only string property.
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
        }

        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Image;
            d.img = bmp;
            return d;
        }

        public void setPath(string path) {
            filepath = path;
            properties["path"].setString(path);

            int lastSlash = path.LastIndexOf('\\') + 1;
            lastSlash = lastSlash == -1 ? 0 : lastSlash;

            setExtra(path.Substring(lastSlash));
            soil();
        }

        private void OnPathChange(object sender, EventArgs e) {
            setPath(properties["path"].getString());
        }

        protected override void clean() {
            base.clean();
            //load image from path, if it exists
            try {
                bmp = new Bitmap(filepath);
                bmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            } catch (Exception) {
                //silently fail. 
                bmp = null;
            }
        }
    }
}
