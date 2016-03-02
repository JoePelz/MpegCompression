using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.NodeProperties {
    class PropertyColor : Property {
        public PropertyColor(bool input, bool output) : base(input, output, Type.COLOR) {
            sLabel = "Color";
        }
    }
}
