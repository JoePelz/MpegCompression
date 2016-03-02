using MpegCompressor.NodeProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public interface IProperties {
        Dictionary<string, Property> getProperties();
    }
}
