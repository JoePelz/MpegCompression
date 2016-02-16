using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class ChannelsToColor : Node {

        public ChannelsToColor() {
            rename("Channels");
            setExtra("to Color");
        }

        protected override void createInputs() {
            inputs.Add("inChannels", null);
        }

        protected override void createOutputs() {
            outputs.Add("outColor", new HashSet<Address>());
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

            state = state.clone();

            if (state.type != DataBlob.Type.Channels || state.channels == null) {
                state = null;
                return;
            }

            state.bmp = Subsample.channelsToBitmap(state.channels, state.samplingMode, state.width, state.height);
            state.type = DataBlob.Type.Image;
        }
    }
}
