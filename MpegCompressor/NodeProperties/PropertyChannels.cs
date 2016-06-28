using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.NodeProperties {
    public class PropertyChannels : Property {
        public PropertyChannels(bool input, bool output) : base(input, output, Type.CHANNELS) {
            sLabel = "Channels";
        }

        public override string ToString() {
            if (isInput) {
                return String.Format("{0}.{1}", input.node.getID(), input.port);
            } else {
                return "...";
            }
        }

        public override void FromString(string data) {
            throw new NotImplementedException();
        }
    }
}
