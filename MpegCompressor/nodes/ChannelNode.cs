using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public abstract class ChannelNode : Node {

        protected override void createProperties() {
            base.createProperties();
            properties.Add("inChannels", new Property(true, false));
            properties.Add("outChannels", new Property(false, true));
        }

        protected override void clean() {
            base.clean();

            //Acquire source
            Address upstream = properties["inChannels"].input;
            if (upstream == null) {
                return;
            }
            state = upstream.node.getData(upstream.port);
            if (state == null) {
                return;
            }

            if (state.type != DataBlob.Type.Channels || state.channels == null) {
                state = null;
                return;
            }

            state = state.clone();

            //create copy of channels for local use.
            byte[][] newChannels = new byte[state.channels.Length][];
            for (int channel = 0; channel < newChannels.Length; channel++) {
                newChannels[channel] = (byte[])state.channels[channel].Clone();
            }
            state.channels = newChannels;
            state.bmp = null;
        }
    }
}
