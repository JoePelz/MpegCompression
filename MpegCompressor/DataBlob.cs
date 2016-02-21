using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MpegCompressor {
    public class DataBlob {
        public enum Type { Image, Channels, Vectors };
        public enum Samples { s444, s422, s411, s420 };


        public Type type;

        public Bitmap bmp;

        public byte[][] channels;
        public int imageWidth;
        public int imageHeight;
        public int channelWidth;
        public int channelHeight;
        public int quantizeQuality;
        public Samples samplingMode;

        public DataBlob clone() {
            DataBlob d = new DataBlob();
            d.type = type;
            d.bmp = bmp;
            d.channels = channels;
            d.imageWidth = imageWidth;
            d.imageHeight = imageHeight;
            d.channelHeight = channelHeight;
            d.channelWidth = channelWidth;
            d.quantizeQuality = quantizeQuality;
            d.samplingMode = samplingMode;
            return d;
        }
    }
}
