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
