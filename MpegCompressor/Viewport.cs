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
        //private static float ZOOM_FACTOR = 1.2f;
        private static float ZOOM_FACTOR = 2.0f;
        Matrix xform;
        Point translate;
        Point mDown;
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
            if (content != null) {
                content.view(pe);
            }
        }

        public void setSource(IViewable view) {
            content = view;
        }

        private void updateTransform() {
            xform.Reset();
            xform.Translate(translate.X, translate.Y);
            xform.Scale(scale, scale);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            mDown.X = e.X;
            mDown.Y = e.Y;
            bMDown = true;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (bMDown) {
                translate.X += e.X - mDown.X;
                translate.Y += e.Y - mDown.Y;
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
                //translate.X = (int)((translate.X - e.X) * ZOOM_FACTOR) + e.X;
                //translate.Y = (int)((translate.Y - e.Y) * ZOOM_FACTOR) + e.Y;
                scale *= ZOOM_FACTOR;
            } else if (scroll < 0) {
                //translate.X = (int)((translate.X - e.X) / ZOOM_FACTOR) + e.X;
                //translate.Y = (int)((translate.Y - e.Y) / ZOOM_FACTOR) + e.Y;
                scale /= ZOOM_FACTOR;
            }
            updateTransform();
        }
        
    }
}
