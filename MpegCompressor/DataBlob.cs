using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MpegCompressor {
    public class DataBlob {
        public enum Type {
            Image,
            Channels
        };

        public Type type;

        public Bitmap img;

        public byte[][] channels;
        public int width;
        public int height;
        public Subsample.Samples samplingMode;
    }
}
