using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MpegCompressor {
    public class DataBlob {
        public enum Type {
            Image
        };

        public Type type;
        public Image img;
    }
}
