using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public abstract class ChannelNode : Node {
        protected byte[][] channels;
        protected int width, height;
        protected Subsample.Samples samples;
        
        protected override void createInputs() {
            inputs.Add("inChannels", null);
        }

        protected override void createOutputs() {
            outputs.Add("outChannels", new HashSet<Address>());
        }

        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Channels;
            d.channels = channels;
            d.width = width;
            d.height = height;
            d.samplingMode = samples;
            return d;
        }

        protected override void clean() {
            base.clean();

            bmp = null;
            channels = null;

            //Acquire source
            Address upstream = inputs["inChannels"];
            if (upstream == null) {
                return;
            }
            DataBlob dataIn = upstream.node.getData(upstream.port);
            if (dataIn == null) {
                return;
            }

            //Acquire data from source
            if (dataIn.type == DataBlob.Type.Channels) {
                if (dataIn.channels == null)
                    return;
                //copy channels to local arrays
                channels = new byte[dataIn.channels.Length][];
                samples = dataIn.samplingMode;
                width = dataIn.width;
                height = dataIn.height;
                for (int channel = 0; channel < channels.Length; channel++) {
                    channels[channel] = (byte[])dataIn.channels[channel].Clone();
                }
            } else {
                return;
            }
        }
    }
}
