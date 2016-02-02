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
        Matrix xform;
        float scale;
        Point mDown;
        bool bMDown;

        private IViewable content;

        public Viewport() {
            InitializeComponent();
            xform = new Matrix();
            DoubleBuffered = true;
            scale = 1;
        }

        public Viewport(IContainer container) {
            container.Add(this);

            InitializeComponent();
            xform = new Matrix();
            DoubleBuffered = true;
            scale = 1;
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

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            mDown.X = e.X;
            mDown.Y = e.Y;
            bMDown = true;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (bMDown) {
                xform.Translate(e.X - mDown.X, e.Y - mDown.Y);
                mDown.X = e.X;
                mDown.Y = e.Y;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);
            bMDown = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            base.OnMouseWheel(e);
            PointF[] center = new PointF[1];
            center[0] = new PointF();
            center[0].X = e.X;
            center[0].Y = e.Y;
            xform.TransformPoints(center);
            xform.Translate(center[0].X, center[0].Y);
            float scroll = e.Delta / 120.0f;
            if (scroll > 0) {
                xform.Scale(1.2f, 1.2f);
                scale *= 1.2f;
            } else if (scroll < 0) {
                xform.Scale(1 / 1.2f, 1 / 1.2f);
                scale /= 1.2f;
            }
            xform.Translate(-center[0].X, -center[0].Y);
            Invalidate();
        }
    }
}
