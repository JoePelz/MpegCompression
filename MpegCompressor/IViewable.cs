using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public interface IViewable {
        void view(PaintEventArgs pe);
    }
}
