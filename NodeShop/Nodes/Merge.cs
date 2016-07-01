using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeShop.NodeProperties;

namespace NodeShop.Nodes {
    public class Merge : Node {
        public enum Method { Multiply, Add, Subtract, Divide };
        private static string[] options
            = new string[4] {
                "Multiply",
                "Add",
                "Subtract",
                "Divide" };
        private Method method;

        public Merge(): base() { }
        public Merge(NodeView graph) : base(graph) { }
        public Merge(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }


        protected override void init() {
            base.init();
            method = 0;
            rename("Merge");
        }

        protected override void createProperties() {
            base.createProperties();

            Property p = new PropertySelection(options, (int)method, "merge operator");
            p.eValueChanged += e_OpChanged;
            properties["mergeMethod"] = p;

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

        public void setMethod(Method newMethod) {
            method = newMethod;
            properties["mergeMethod"].nValue = (int)newMethod;
            soil();
            
        }

        private void e_OpChanged(object sender, EventArgs e) {
            setMethod((Method)properties["mergeMethod"].nValue);
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

            if (state.type != DataBlob.Type.Channels || state.channels == null) {
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

            merge(state.channels, stateB.channels, state.channels, method);
        }

        public void merge(byte[][] A, byte[][] B, byte[][] outChannels, Method operation) {
            switch(operation) {
                case Method.Multiply:
                    for (int band = 0; band < B.Length; band++) {
                        for (int pixel = 0; pixel < B[band].Length; pixel++) {
                            outChannels[band][pixel] = (byte)(A[band][pixel] * B[band][pixel] / 255);
                        }
                    }
                    break;
                case Method.Divide:
                    for (int band = 0; band < B.Length; band++) {
                        for (int pixel = 0; pixel < B[band].Length; pixel++) {
                            outChannels[band][pixel] = (byte)(Math.Min(255, (double)A[band][pixel] / B[band][pixel] * 255.0));
                        }
                    }
                    break;
                case Method.Add:
                    for (int band = 0; band < B.Length; band++) {
                        for (int pixel = 0; pixel < B[band].Length; pixel++) {
                            outChannels[band][pixel] = (byte)(Math.Min(255, A[band][pixel] + B[band][pixel]));
                        }
                    }
                    break;
                case Method.Subtract:
                    for (int band = 0; band < B.Length; band++) {
                        for (int pixel = 0; pixel < B[band].Length; pixel++) {
                            outChannels[band][pixel] = (byte)(Math.Max(0, A[band][pixel] - B[band][pixel]));
                        }
                    }
                    break;
            }
        }
    }
}
