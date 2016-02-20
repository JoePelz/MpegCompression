using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MpegCompressor {
    public interface IViewable {

        event EventHandler eViewChanged;
        Bitmap view();

        void viewExtra(Graphics g);
    }
}
