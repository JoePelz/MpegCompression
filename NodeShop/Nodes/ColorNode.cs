using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeShop.NodeProperties;
using System.Drawing.Imaging;

namespace NodeShop.Nodes {
    public abstract class ColorNode : ChannelNode {
        protected const int r = 2;
        protected const int g = 1;
        protected const int b = 0;

        public ColorNode(): base() { }
        public ColorNode(NodeView graph) : base(graph) { }
        public ColorNode(NodeView graph, int posX, int posY) : base(graph, posX, posY) { }
        
        protected override void clean() {
            base.clean();

            processChannels();
        }

        protected void processChannels() {
            //make copy of relevant channels
            //run processing to write to output using input
            //copy output back into source channels

            byte[][] workingCopy = new byte[3][];
            workingCopy[0] = new byte[state.channels[0].Length];
            workingCopy[1] = new byte[state.channels[1].Length];
            workingCopy[2] = new byte[state.channels[2].Length];

            processChannels(state.channels, workingCopy, state.channelWidth, state.channelHeight);

            state.channels[0] = workingCopy[0];
            state.channels[1] = workingCopy[1];
            state.channels[2] = workingCopy[2];

            //set state.bmp?   Viewport.channelsToBitmap(...)
        }

        protected virtual void processChannels(byte[][] inValues, byte[][] outValues, int w, int h) {
            for (int i = 0; i < outValues.Length; i++) {
                Array.Copy(inValues[i], outValues[i], outValues[i].Length);
            }
        }
    }
}
