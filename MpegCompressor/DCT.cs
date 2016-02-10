using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    class DCT : Node {
        private int chunkSize;

        protected override void createInputs() {
            inputs.Add("inChannels", null);
        }

        protected override void createOutputs() {
            outputs.Add("outChannels", new HashSet<Address>());
        }

        protected override void createProperties() {
            Property p = new Property();
            p.createInt(4, 2, 16, "Chunk size to use");
            p.eValueChanged += P_eValueChanged;
            properties["chunkSize"] = p;
            
        }
        
        public void setChunkSize(int size) {
            chunkSize = Math.Min(Math.Max(2, size), 16);
            properties["chunkSize"].setInt(chunkSize);
            setExtra("Chunk Size: " + chunkSize);
            soil();
        }

        private void P_eValueChanged(object sender, EventArgs e) {
            setChunkSize(properties["chunkSize"].getInt());
        }
    }
}
