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
        private Bitmap img;

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
            d.img = img;
            return d;
        }

        private void setPath(string path) {
            filepath = path;
            properties["path"].setString(path);
            soil();
        }

        private void OnPathChange(object sender, EventArgs e) {
            setPath(properties["path"].getString());
        }

        protected override void clean() {
            base.clean();
            //load image from path, if it exists
            try {
                img = new Bitmap(filepath);
            } catch (Exception) {
                //silently fail. 
                img = null;
            }
        }

        public override Bitmap view() {
            base.view();
            if (img == null) {
                return null;
            }
            return img.Clone(new Rectangle(0, 0, img.Width, img.Height), img.PixelFormat);
        }

        public override Rectangle getExtents() {
            return new Rectangle(-img.Width / 2, -img.Height / 2, img.Width, img.Height);
        }
    }
}
