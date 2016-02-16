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

        public Bitmap bmp;

        public byte[][] channels;
        public int width;
        public int height;
        //public int realWidth;
        //public int realHeight;
        public Subsample.Samples samplingMode;

        public DataBlob clone() {
            DataBlob d = new DataBlob();
            d.type = type;
            d.bmp = bmp;
            d.channels = channels;
            d.width = width;
            d.height = height;
            //d.realHeight = realHeight;
            //d.realWidth = realWidth;
            d.samplingMode = samplingMode;
            return d;
        }
    }
}
