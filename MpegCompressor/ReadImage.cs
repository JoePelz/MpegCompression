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
        private Image img;

        public ReadImage() {
            setPath("C:\\temp\\uv.jpg");
        }

        protected override void createProperties() {
            base.createProperties();
            //change name to read
            Property p = properties["name"];
            p.setString("Read");

            //create filepath property
            p = new Property();
            p.createString("", "Image file to open");
            p.eValueChanged += OnPathChange;
            properties.Add("path", p);
        }

        private void setPath(string path) {
            filepath = path;
            properties["path"].setString(path);
            //load image from path, if it exists
            try {
                img = Image.FromFile(filepath);
            } catch (Exception x) {
                string temp = x.Message;
                //silently fail.
                img = null;
            }
            soil();
        }

        private void OnPathChange(object sender, EventArgs e) {
            setPath(properties["path"].getString());
        }

        public override void view(PaintEventArgs pe) {
            if (img == null) {
                return;
            }
            Graphics g = pe.Graphics;
            //g.DrawImageUnscaled(img, -img.Width, 0);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.DrawImage(img, -img.Width / 2, -img.Height / 2, img.Width, img.Height);
        }

        public override Rectangle getExtents() {
            return new Rectangle(-img.Width / 2, -img.Height / 2, img.Width, img.Height);
        }
    }
}
