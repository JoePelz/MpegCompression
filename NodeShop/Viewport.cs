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

namespace NodeShop {
    public partial class Viewport : TransformPanel {
        private char viewChannel;
        private bool viewExtra;

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
            viewExtra = true;
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
            } else if (keyData == Keys.O) {
                viewExtra = !viewExtra;
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

        public static Bitmap channelsToBitmap(DataBlob data) {
            if (data == null || data.channels == null) {
                return null;
            }

            Bitmap bmp = new Bitmap(data.imageWidth, data.imageHeight, PixelFormat.Format24bppRgb);

            BitmapData bmpData = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height),
                                ImageLockMode.ReadWrite,
                                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;


            //copy bytes
            int nBytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[nBytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, nBytes);
            int pixel;
            int iY, iCrb;
            int channelYStride = data.channelWidth;
            //TODO: don't assume padded. :P
            int channelCRBStride = Nodes.Subsample.getCbCrStride(data);

            for (int y = 0; y < data.imageHeight; y++) {
                pixel = y * bmpData.Stride;
                iY = y * channelYStride;
                iCrb = (data.samplingMode == DataBlob.Samples.s420 ? y / 2 : y) * channelCRBStride;
                for (int x = 0; x < data.imageWidth; x++) {
                    rgbValues[pixel + 2] = data.channels[0][iY];
                    rgbValues[pixel + 1] = data.channels[1][iCrb];
                    rgbValues[pixel] = data.channels[2][iCrb];
                    pixel += 3;
                    iY++;

                    if (data.samplingMode == DataBlob.Samples.s420 || data.samplingMode == DataBlob.Samples.s422) {
                        if (x % 2 == 1) {
                            iCrb++;
                        }
                    } else if (data.samplingMode == DataBlob.Samples.s444) {
                        iCrb++;
                    } else if (data.samplingMode == DataBlob.Samples.s411) {
                        if (x % 4 == 3) {
                            iCrb++;
                        }
                    }

                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, nBytes);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            Bitmap img = null;
            Graphics g = pe.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality; //stop cutting corners! :P
            g.InterpolationMode = InterpolationMode.NearestNeighbor; //I wanna see the PIXELS, dammit!

            if (content != null) {
                img = content.view();
                if (img != null) {
                    img = img.Clone(new Rectangle(0, 0, img.Width, img.Height), img.PixelFormat);
                    channelFilter(img);
                } else {
                    img = generateImage();
                }
                setFocusRect(0, -img.Height, img.Width, img.Height);
                g.DrawImage(img, 0, -img.Height, img.Width, img.Height);
                g.ScaleTransform(1, -1);
                if (viewExtra) {
                    content.viewExtra(g);
                }
            }

            if (img != null) {
                g.ResetTransform();
                g.DrawString(content + ": " + img.Width + "x" + img.Height, new Font("arial", 10.0f), Brushes.Black, 0, 0);
            }
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
