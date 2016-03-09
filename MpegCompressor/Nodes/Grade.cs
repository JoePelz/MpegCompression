using MpegCompressor.NodeProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class Grade : ColorNode {
        public Grade(): base() { }
        public Grade(NodeView graph) : base(graph) { }
        public Grade(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }

        protected override void init() {
            base.init();
            rename("Grade");
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertyFloat(0, -100, 100, "Black point");
            p.eValueChanged += (s, e) => { soil(); };
            properties["black"] = p;

            p = new PropertyFloat(1, -100, 100, "White point");
            p.eValueChanged += (s, e) => { soil(); };
            properties["white"] = p;

            p = new PropertyFloat(1, -100, 100, "Multiply");
            p.eValueChanged += (s, e) => { soil(); };
            properties["mult"] = p;

            p = new PropertyFloat(0, -100, 100, "Offset");
            p.eValueChanged += (s, e) => { soil(); };
            properties["add"] = p;
        }

        protected override void processPixels(byte[] inValues, byte[] outValues, int w, int h, int xstep, int ystep) {
            float black = properties["black"].fValue;
            float white = properties["white"].fValue;
            float mult = properties["mult"].fValue;
            float add = properties["add"].fValue;
            float val;
            int pixel;
            for (int band = 0; band < xstep; band++) {
                for (int y = 0; y < h; y++) {
                    for (int x = 0; x < w; x++) {
                        pixel = y * ystep + x * xstep;
                        val = inValues[pixel + band];

                        val /= 255;
                        val -= black;
                        val /= (white - black);
                        
                        val *= mult;
                        val += add;

                        val = val > 1 ? 1 : val < 0 ? 0 : val;
                        outValues[pixel + band] = (byte)(val * 255);
                    }
                }
            }
        }
    }
}
