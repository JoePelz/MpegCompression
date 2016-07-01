using NodeShop.NodeProperties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeShop.Nodes {
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
        
        protected override void processChannels(byte[][] inValues, byte[][] outValues, int w, int h) {
            float black = properties["black"].fValue;
            float white = properties["white"].fValue;
            float mult = properties["mult"].fValue;
            float add = properties["add"].fValue;
            float val;
            for (int band = 0; band < 3; band++) {
                for (int pixel = 0; pixel < inValues[band].Length; pixel++) {
                    val = inValues[band][pixel];

                    val /= 255;
                    val -= black;
                    val /= (white - black);
                        
                    val *= mult;
                    val += add;

                    val = val > 1 ? 1 : val < 0 ? 0 : val; //clamp 0..1
                    outValues[band][pixel] = (byte)(val * 255);
                }
            }
        }
    }
}
