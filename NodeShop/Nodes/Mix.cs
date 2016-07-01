using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeShop.NodeProperties;

namespace NodeShop.Nodes {
    public class Mix : Node {
        private float ratio;

        public Mix(): base() { }
        public Mix(NodeView graph) : base(graph) { }
        public Mix(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            setRatio(0.5f);
            rename("Mix");
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertyFloat(ratio, 0.0f, 1.0f, "Mix ratio");
            p.eValueChanged += e_RatioChanged;
            properties["mixRatio"] = p;

            p = new PropertyButton("Swap", "Swap inputs A and B");
            p.eValueChanged += e_SwapInputs;
            properties["btnSwap"] = p;

            properties.Add("inColorA", new PropertyChannels(true, false));
            properties.Add("inColorB", new PropertyChannels(true, false));
            properties.Add("outColor", new PropertyChannels(false, true));
        }

        private void e_SwapInputs(object sender, EventArgs e) {
            Address temp = properties["inColorA"].input;
            properties["inColorA"].input = properties["inColorB"].input;
            properties["inColorB"].input = temp;
            soil();
        }

        public void setRatio(float newRatio) {
            ratio = newRatio;
            properties["mixRatio"].fValue = newRatio;
            soil();

        }

        private void e_RatioChanged(object sender, EventArgs e) {
            setRatio(properties["mixRatio"].fValue);
        }

        protected override void clean() {
            base.clean();

            Address upstreamA = properties["inColorA"].input;
            if (upstreamA == null) {
                return;
            }
            Address upstreamB = properties["inColorB"].input;
            if (upstreamB == null) {
                return;
            }

            state = upstreamA.node.getData(upstreamA.port);
            if (state == null) {
                return;
            }
            DataBlob stateB = upstreamB.node.getData(upstreamB.port);
            if (stateB == null) {
                return;
            }

            state = state.clone();

            if (state.type != DataBlob.Type.Channels || state.channels== null) {
                state = null;
                return;
            }
            if (stateB.type != DataBlob.Type.Channels || stateB.channels == null) {
                stateB = null;
                return;
            }

            //check resolutions match
            if (state.imageWidth != stateB.imageWidth || state.imageHeight != stateB.imageHeight) {
                return;
            }

            byte[][] newChannels = new byte[state.channels.Length][];
            for (int channel = 0; channel < newChannels.Length; channel++) {
                newChannels[channel] = (byte[])state.channels[channel].Clone();
            }
            state.channels = newChannels;
            state.bmp = null;

            mix(state.channels, stateB.channels, state.channels, ratio);
        }

        public void mix(byte[][] A, byte[][] B, byte[][] outChannels, float blend) {
            float xblend = 1.0f - blend;
            for(int band = 0; band < A.Length; band++) {
                for (int pixel = 0; pixel < A[band].Length; pixel++) {
                    outChannels[band][pixel] = (byte)(blend * A[band][pixel] + xblend * B[band][pixel]);
                }
            }
        }
    }
}
