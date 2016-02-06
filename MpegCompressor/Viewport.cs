using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MpegCompressor {
    public partial class Viewport : TransformPanel {
        private char viewChannel;

        private IViewable content;

        public Viewport() {
            InitializeComponent();
            init();
        }

        public Viewport(IContainer container) {
            container.Add(this);

            InitializeComponent();
            init();
        }

        private void init() {
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
        }

        protected override void OnMouseEnter(EventArgs e) {
            this.Focus();
            base.OnMouseEnter(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.R) {
                if (viewChannel == 'r')
                    viewChannel = '\0';
                else
                    viewChannel = 'r';
                Invalidate();
                return true;
            } else if (keyData == Keys.G) {
                if (viewChannel == 'g')
                    viewChannel = '\0';
                else
                    viewChannel = 'g';
                Invalidate();
                return true;
            } else if (keyData == Keys.B) {
                if (viewChannel == 'b')
                    viewChannel = '\0';
                else
                    viewChannel = 'b';
                Invalidate();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void channelFilter(Bitmap bmp) {
            if (viewChannel != 'r' && viewChannel != 'g' && viewChannel != 'b') {
                return;
            }
            BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];
            int pixel;
            int Bpp = 3;

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            if (viewChannel == 'r') {
                for (int y = 0; y < bmpData.Height; y++) {
                    for (int x = 0; x < bmpData.Width; x++) {
                        pixel = y * bmpData.Stride + x * Bpp;
                        rgbValues[pixel] = rgbValues[pixel + 2];
                        rgbValues[pixel + 1] = rgbValues[pixel + 2];
                    }
                }
            } else if (viewChannel == 'g') {
                for (int y = 0; y < bmpData.Height; y++) {
                    for (int x = 0; x < bmpData.Width; x++) {
                        pixel = y * bmpData.Stride + x * Bpp;
                        rgbValues[pixel] = rgbValues[pixel + 1];
                        rgbValues[pixel + 2] = rgbValues[pixel + 1];
                    }
                }
            } else if (viewChannel == 'b') {
                for (int y = 0; y < bmpData.Height; y++) {
                    for (int x = 0; x < bmpData.Width; x++) {
                        pixel = y * bmpData.Stride + x * Bpp;
                        rgbValues[pixel + 1] = rgbValues[pixel];
                        rgbValues[pixel + 2] = rgbValues[pixel];
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);

            Graphics g = pe.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor; //I wanna see the PIXELS, dammit!

            if (content != null) {
                Bitmap img = content.view();
                if (img != null) {
                    img = img.Clone(new Rectangle(0, 0, img.Width, img.Height), img.PixelFormat);
                    channelFilter(img);
                    setFocusRect(img.Width, img.Height);
                } else {
                    img = generateImage();
                    setFocusRect(img.Width, img.Height);
                }
                g.DrawImage(img, -img.Width / 2, -img.Height / 2, img.Width, img.Height);
            }
            
            /*
            //Draw Origin (0, 0)
            g.DrawEllipse(Pens.Black, -5, -5, 10, 10);
            g.DrawEllipse(Pens.Black, -2, -2, 4, 4);
            g.DrawLine(Pens.Black, 0, -50, 0, 50);
            g.DrawLine(Pens.Black, -50, 0, 50, 0);
            g.DrawString("(0, 0)", new Font("arial", 10.0f), Brushes.Black, 0, -20);
            */
        }

        private Bitmap generateImage() {
            //draw a picture!
            //Bitmap render = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Bitmap render = new Bitmap(128, 128, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData bmpData = render.LockBits(
                new Rectangle(0, 0, render.Width, render.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                render.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * render.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);

            //order: B,G,R,A,  B,G,R,A,  ...
            for (int counter = 1; counter < rgbValues.Length; counter++) {
                rgbValues[counter] = (byte)(counter % 256); //whatever
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            render.UnlockBits(bmpData);

            return render;
        }

        public void setSource(IViewable view) {
            //unregister old event handler
            if (content != null) {
                content.eViewChanged -= OnSourceUpdate;
            }
            content = view;
            //register new event handler
            if (content != null) {
                content.eViewChanged += OnSourceUpdate;
            }
        }

        private void OnSourceUpdate(object sender, EventArgs e) {
            Invalidate();
        }
    }
}
