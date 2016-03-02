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
    }
}
