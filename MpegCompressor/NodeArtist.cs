using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpegCompressor {
    static class NodeArtist {
        public static Font nodeFont = new Font("Tahoma", 11.0f);
        public static Font nodeExtraFont = new Font("Tahoma", 11.0f, FontStyle.Italic);
        public static Font nodeTitleFont = new Font("Tahoma", 13.0f, FontStyle.Bold);
        public static int ballSize = nodeFont.Height / 2;
        public static int ballOffset = (nodeFont.Height - ballSize) / 2;
        
        private static Rectangle getGraphRect(Nodes.Node n) {
            int titleWidth = System.Windows.Forms.TextRenderer.MeasureText(n.getName(), nodeTitleFont).Width;
            Rectangle nodeRect = new Rectangle();
            nodeRect.Width = Math.Max(100, titleWidth);
            nodeRect.Height = nodeTitleFont.Height;

            //count the lines to cover with text
            if (n.getExtra() != null) {
                nodeRect.Height += nodeExtraFont.Height;
            }
            foreach (var kvp in n.getProperties()) {
                if (kvp.Value.getType() == Property.Type.NONE) {
                    nodeRect.Height += nodeFont.Height;
                }
            }
            return nodeRect;
        }

        public static Point getJointPos(Nodes.Node n, string port, bool input) {
            Rectangle nodeRect = getGraphRect(n);
            Point result = new Point(nodeRect.X, nodeRect.Y);
            result.Y += nodeTitleFont.Height;
            if (n.getExtra() != null) {
                result.Y += nodeExtraFont.Height;
            }

            foreach (var kvp in n.getProperties()) {
                if (kvp.Key == port) {
                    break;
                }
                if (kvp.Value.getType() == Property.Type.NONE) {
                    result.Y += nodeFont.Height;
                }
            }
            if (!input) {
                result.X += nodeRect.Width;
            }
            result.Y += nodeFont.Height / 2 - ballSize / 2;
            return result;
        }
    }
}
