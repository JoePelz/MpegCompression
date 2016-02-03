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

namespace MpegCompressor {
    public partial class Viewport : Panel {
        private static float ZOOM_FACTOR = 1.2f;
        Matrix xform;
        PointF translate;
        PointF mDown;
        float scale;
        bool bMDown;

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
        }

        protected override void OnPaint(PaintEventArgs pe) {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            g.Transform = xform;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            if (content != null) {
                content.view(pe);
            }
            g.InterpolationMode = InterpolationMode.Default;
            //Draw Origin (0, 0)
            /*
            g.DrawEllipse(Pens.Black, -5, -5, 10, 10);
            g.DrawEllipse(Pens.Black, -2, -2, 4, 4);
            g.DrawLine(Pens.Black, 0, -50, 0, 50);
            g.DrawLine(Pens.Black, -50, 0, 50, 0);
            g.DrawString("(0, 0)", new Font("arial", 10.0f), Brushes.Black, 0, -20);
            */
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
