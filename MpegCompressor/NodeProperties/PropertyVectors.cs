using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.NodeProperties {
    class PropertyVectors : Property {
        public PropertyVectors(bool input, bool output) : base(input, output, Type.VECTORS) {
            sLabel = "Channels";
        }
    }
}
