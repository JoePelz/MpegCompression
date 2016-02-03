using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    public interface IViewable {

        event EventHandler eViewChanged;

        void view(PaintEventArgs pe);

        System.Drawing.Rectangle getExtents();
    }
}
