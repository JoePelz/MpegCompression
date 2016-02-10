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

        public override DataBlob getData(string port) {
            base.getData(port);
            DataBlob d = new DataBlob();
            d.type = DataBlob.Type.Image;
            d.img = bmp;
            return d;
        }

        protected override void clean() {
            base.clean();

            bmp = null;

            //Acquire source
            Address upstream = inputs["inChannels"];
            if (upstream == null) {
                return;
            }
            DataBlob dataIn = upstream.node.getData(upstream.port);
            if (dataIn == null) {
                return;
            }

            if (dataIn.type == DataBlob.Type.Channels) {
                if (dataIn.channels == null)
                    return;
                //copy channels to local arrays
                bmp = Subsample.channelsToBitmap(dataIn.channels, dataIn.samplingMode, dataIn.width, dataIn.height);
            } else {
                return;
            }
            
            
        }
    }
}
