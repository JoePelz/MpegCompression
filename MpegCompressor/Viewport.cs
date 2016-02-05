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
    public partial class Viewport : Panel {
        private static float ZOOM_FACTOR = 1.2f;
        private Matrix xform;
        private PointF translate;
        private PointF mDown;
        private float scale;
        private bool bMDown;
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
            DoubleBuffered = true;
            scale = 1;
            xform = new Matrix();
            translate = new Point();
            mDown = new Point();
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
            g.Transform = xform;
            g.InterpolationMode = InterpolationMode.NearestNeighbor; //I wanna see the PIXELS, dammit!

            if (content != null) {
                Bitmap img = content.view();
                if (img != null) {
                    channelFilter(img);
                } else {
                    img = generateImage();
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
            if (content != null) {
                content.eViewChanged -= OnSourceUpdate;
            }
            content = view;
            if (content != null) {
                content.eViewChanged += OnSourceUpdate;
            }
        }

        private void updateTransform() {
            xform.Reset();
            //Note:  .translate() and .scale() _prepend_ a transformation, 
            // so this is like SCALE * TRANSLATE despite being in reverse order.
            xform.Translate(translate.X, translate.Y);
            xform.Scale(scale, scale);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Right) {
                focusView();
                return;
            }

            mDown.X = e.X;
            mDown.Y = e.Y;
            bMDown = true;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (bMDown) {
                translate.X += (e.X - mDown.X);
                translate.Y += (e.Y - mDown.Y);
                mDown.X = e.X;
                mDown.Y = e.Y;
                updateTransform();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            bMDown = false;
        }
        
        protected override void OnMouseWheel(MouseEventArgs e) {
            //e is in screen space.
            base.OnMouseWheel(e);
            
            float scroll = e.Delta / 120.0f;
            if (scroll > 0) {
                translate.X = (int)((translate.X - e.X) * ZOOM_FACTOR) + e.X;
                translate.Y = (int)((translate.Y - e.Y) * ZOOM_FACTOR) + e.Y;
                scale *= ZOOM_FACTOR;
            } else if (scroll < 0) {
                translate.X = (int)((translate.X - e.X) / ZOOM_FACTOR) + e.X;
                translate.Y = (int)((translate.Y - e.Y) / ZOOM_FACTOR) + e.Y;
                scale /= ZOOM_FACTOR;
            }
            
            updateTransform();
        }

        private void OnSourceUpdate(object sender, EventArgs e) {
            Invalidate();
        }

        private void focusView() {
            if (content == null) {
                return;
            }
            int margin = 10;

            Rectangle target = content.getExtents();
            if (target.Width == 0) {
                target.X = -64;
                target.Y = -64;
                target.Width = 128;
                target.Height = 128;
            }
            target.X -= margin;
            target.Y -= margin;
            target.Width += margin*2;
            target.Height += margin*2;



            float scaleW = ((float)Size.Width / (target.Width));
            float scaleH = ((float)Size.Height / (target.Height));
            if (scaleW < scaleH) {
                scale = scaleW;
                translate = new PointF(
                        (-target.X * scale),
                        (-scale * (2 * target.Y + target.Height) + Size.Height) / 2);

            } else {
                scale = scaleH;
                translate = new PointF(
                        (-scale * (2*target.X + target.Width) + Size.Width) / 2,
                        (-target.Y * scale));
            }
            updateTransform();
        }
        
    }
}
