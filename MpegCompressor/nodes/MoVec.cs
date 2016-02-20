using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor.Nodes {
    public class MoVec : Node {
        private const int searchRange = 15;
        
        public MoVec() {
            rename("Motion Vectors");
        }

        protected override void createProperties() {
            base.createProperties();

            properties.Add("inChannelNow", new Property(true, false));
            properties.Add("inChannelPast", new Property(true, false));
            properties.Add("outVectors", new Property(false, true));
            properties.Add("outChannels", new Property(false, true));
        }

        protected override void clean() {
            base.clean();

            Address upstreamNow = properties["inChannelNow"].input;
            if (upstreamNow == null) {
                return;
            }
            Address upstreamPast = properties["inChannelPast"].input;
            if (upstreamPast == null) {
                return;
            }

            state = upstreamNow.node.getData(upstreamNow.port);
            if (state == null) {
                return;
            }
            DataBlob statePast = upstreamPast.node.getData(upstreamPast.port);
            if (statePast == null) {
                return;
            }

            state = state.clone();

            if (state.type != DataBlob.Type.Channels || state.channels == null) {
                state = null;
                return;
            }
            if (statePast.type != DataBlob.Type.Channels || statePast.channels == null) {
                return;
            }

            //check resolutions match
            if (state.channels[0].Length != statePast.channels[0].Length
                || state.channels[1].Length != statePast.channels[1].Length
                || state.channels[2].Length != statePast.channels[2].Length) {
                return;
            }
            //no need to duplicate channels for local use--they will only be read, not written.


            calcMoVec(statePast.channels);

            //get rid of the old channel pointers--carrying them around could be dangerous.
            state.channels = null;
        }

        private void calcMoVec(byte[][] channels) {
            //for each channel
            //chunk state.channels into 8x8 blocks
            //compare each block with blocks surrounding them in the arg channels 
            //over x = [-7,7] (range 15 values)
            //and  y = [-7,7] (range 15 values)

            //for each chunk,
            // record the offset to it's origin match
        }

        private int SAD(byte[] a, int startA, byte[] b, int startB, int stride) {
            int sad = 0, ia, ib;
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    ia = y * stride + x;
                    ib = ia + startB;
                    ia = ia + startA;
                    sad += a[ia] > b[ib] ? a[ia] - b[ib] : b[ib] - a[ia];
                }
            }
            return sad;
        }
    }
}
