using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
    public class TransformPanel : Panel {
        private static float ZOOM_FACTOR = 1.2f;
        private Matrix xform;
        private PointF translate;
        private PointF mDown;
        private float scale;
        private bool bMDown;
        private Rectangle contentRect;

        public TransformPanel() {
            init();
        }

        public TransformPanel(IContainer container) {
            container.Add(this);
            init();
        }

        public void setFocusRect(int width, int height) {
            contentRect.X = -width / 2;
            contentRect.Y = -height / 2;
            contentRect.Width = width;
            contentRect.Height = height;
        }

        private void init() {
            DoubleBuffered = true;
            scale = 1;
            xform = new Matrix();
            translate = new Point();
            mDown = new Point();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            e.Graphics.Transform = xform;
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

        protected virtual void focusView() {
            int margin = 10;

            Rectangle target = contentRect;
            if (target.Width == 0) {
                target.X = -64;
                target.Y = -64;
                target.Width = 128;
                target.Height = 128;
            }
            target.X -= margin;
            target.Y -= margin;
            target.Width += margin * 2;
            target.Height += margin * 2;
            
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
                        (-scale * (2 * target.X + target.Width) + Size.Width) / 2,
                        (-target.Y * scale));
            }
            updateTransform();
        }
    }
}
