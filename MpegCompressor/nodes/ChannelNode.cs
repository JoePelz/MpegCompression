using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public abstract class ChannelNode : Node {
        
        protected override void createInputs() {
            inputs.Add("inChannels", null);
        }

        protected override void createOutputs() {
            outputs.Add("outChannels", new HashSet<Address>());
        }

        protected override void clean() {
            base.clean();

            //Acquire source
            Address upstream = inputs["inChannels"];
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
        }
    }
}
