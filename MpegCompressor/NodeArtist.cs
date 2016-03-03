using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.NodeProperties;

namespace MpegCompressor {
    static class NodeArtist {
        public static Font nodeFont = new Font("Tahoma", 11.0f);
        public static Font nodeExtraFont = new Font("Tahoma", 11.0f, FontStyle.Italic);
        public static Font nodeTitleFont = new Font("Tahoma", 13.0f, FontStyle.Bold);
        public static int ballSize = nodeFont.Height * 2 / 3;
        public static int ballOffset = (nodeFont.Height - ballSize) / 2;
        public static Pen linePen = new Pen(Color.Black, 3);
        public static Pen linkChannels = new Pen(Color.Navy, 3);
        public static Pen linkColors = new Pen(Color.Orange, 3);
        public static Pen linkVectors = new Pen(Color.Violet, 3);
        public static Brush brushChannels = new SolidBrush(Color.Navy);
        public static Brush brushColors = new SolidBrush(Color.Orange);
        public static Brush brushVectors = new SolidBrush(Color.Violet);

        public static Rectangle getRect(Nodes.Node n) {
            int titleWidth = System.Windows.Forms.TextRenderer.MeasureText(n.getName(), nodeTitleFont).Width;
            Rectangle nodeRect = new Rectangle();
            nodeRect.Location = n.getPos();
            nodeRect.Width = Math.Max(100, titleWidth);
            nodeRect.Height = nodeTitleFont.Height;

            //count the lines to cover with text
            if (n.getExtra() != null && n.getExtra() != "") {
                nodeRect.Height += nodeExtraFont.Height;
            }
            foreach (var kvp in n.getProperties()) {
                if (kvp.Value.isInput || kvp.Value.isOutput) {
                    nodeRect.Height += nodeFont.Height;
                }
            }
            return nodeRect;
        }

        public static Rectangle getPaddedRect(Nodes.Node n) {
            Rectangle r = getRect(n);
            r.X -= ballSize / 2;
            r.Y -= ballSize / 2;
            r.Width += ballSize;
            r.Height += ballSize;
            return r;
        }

        public static Point getJointPos(Nodes.Node n, string port, bool input) {
            Rectangle nodeRect = getRect(n);
            Point result = nodeRect.Location;
            result.Y += nodeTitleFont.Height;
            if (n.getExtra() != null) {
                result.Y += nodeExtraFont.Height;
            }

            foreach (var kvp in n.getProperties()) {
                if (kvp.Key == port) {
                    break;
                }
                if (kvp.Value.isInput || kvp.Value.isOutput) {
                    result.Y += nodeFont.Height;
                }
            }
            if (!input) {
                result.X += nodeRect.Width;
            }
            result.Y += ballSize / 2;
            return result;
        }
        
        public static string hitJoint(Nodes.Node n, Rectangle rect, int x, int y, bool inputsOnly) {
            //assume using a padded rect
            //missed the balls.
            if (x > rect.Left + ballSize && x < rect.Right - ballSize)
                return null;

            rect.Y += nodeTitleFont.Height;
            if (n.getExtra() != null) {
                rect.Y += nodeExtraFont.Height;
            }

            if (y < rect.Y)
                return null;

            Point pos;
            foreach (var kvp in n.getProperties()) {
                if ((kvp.Value.isInput && inputsOnly) || (kvp.Value.isOutput && !inputsOnly)) {
                    pos = getJointPos(n, kvp.Key, inputsOnly);
                    pos.X -= x; pos.Y -= y;

                    //intentionally dividing by 2 instead of 4 to expand the 'okay' selection radius.
                    if (pos.X * pos.X + pos.Y * pos.Y < ballSize * ballSize / 2) {
                        return kvp.Key;
                    }
                }
            }

            return null;
        }

        public static void drawGraphNode(Graphics g, Nodes.Node n, bool isSelected) {
            Rectangle nodeRect = getRect(n);
            //draw background
            g.FillRectangle(isSelected ? Brushes.Wheat : Brushes.CadetBlue, nodeRect);
            g.DrawRectangle(Pens.Black, nodeRect);

            drawTitle(g, n, ref nodeRect);

            drawProperties(g, n, ref nodeRect);
        }

        public static void drawLinks(Graphics g, Nodes.Node n) {
            foreach (var kvp in n.getProperties()) {
                if (kvp.Value.isInput && kvp.Value.input != null) {
                    if (kvp.Value.getType() == NodeProperties.Type.CHANNELS) {
                        g.DrawLine(linkChannels, NodeArtist.getJointPos(kvp.Value.input.node, kvp.Value.input.port, false), NodeArtist.getJointPos(n, kvp.Key, true));
                    } else if (kvp.Value.getType() == NodeProperties.Type.COLOR) {
                        g.DrawLine(linkColors, NodeArtist.getJointPos(kvp.Value.input.node, kvp.Value.input.port, false), NodeArtist.getJointPos(n, kvp.Key, true));
                    } else if (kvp.Value.getType() == NodeProperties.Type.VECTORS) {
                        g.DrawLine(linkVectors, NodeArtist.getJointPos(kvp.Value.input.node, kvp.Value.input.port, false), NodeArtist.getJointPos(n, kvp.Key, true));
                    } else {
                        g.DrawLine(linePen, NodeArtist.getJointPos(kvp.Value.input.node, kvp.Value.input.port, false), NodeArtist.getJointPos(n, kvp.Key, true));
                    }
                }
            }
        }

        private static void drawTitle(Graphics g, Nodes.Node n, ref Rectangle nodeRect) {
            //draw title
            g.DrawString(n.getName(), nodeTitleFont, Brushes.Black, nodeRect.X + (nodeRect.Width - System.Windows.Forms.TextRenderer.MeasureText(n.getName(), nodeTitleFont).Width) / 2, nodeRect.Y);
            nodeRect.Y += nodeFont.Height;
            g.DrawLine(Pens.Black, nodeRect.Left, nodeRect.Y, nodeRect.Right, nodeRect.Y);
            
            //draw extra
            if (n.getExtra() != null && n.getExtra() != "") {
                g.DrawString(n.getExtra(), nodeExtraFont, Brushes.Black, nodeRect.Location);
                nodeRect.Y += nodeFont.Height;
                g.DrawLine(Pens.Black, nodeRect.Left, nodeRect.Y, nodeRect.Right, nodeRect.Y);
            }
        }

        private static void drawProperties(Graphics g, Nodes.Node n, ref Rectangle nodeRect) {
            //draw properties
            foreach (var kvp in n.getProperties()) {
                //draw bubbles
                if (kvp.Value.isInput) {
                    g.DrawString(kvp.Key, nodeFont, Brushes.Black, nodeRect.Left + ballSize / 2, nodeRect.Y);
                    if (kvp.Value.input != null) {
                        switch(kvp.Value.getType()) {
                            case NodeProperties.Type.CHANNELS:
                                g.FillEllipse(brushChannels, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.COLOR:
                                g.FillEllipse(brushColors, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.VECTORS:
                                g.FillEllipse(brushVectors, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            default:
                                g.FillEllipse(Brushes.Black, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                        }
                    } else {
                        switch (kvp.Value.getType()) {
                            case NodeProperties.Type.CHANNELS:
                                g.DrawEllipse(linkChannels, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.COLOR:
                                g.DrawEllipse(linkColors, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.VECTORS:
                                g.DrawEllipse(linkVectors, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            default:
                                g.DrawEllipse(Pens.Black, nodeRect.Left - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                        }
                    }
                    nodeRect.Y += nodeFont.Height;
                } else if (kvp.Value.isOutput) {
                    g.DrawString(kvp.Key, nodeFont, Brushes.Black, nodeRect.Left + (nodeRect.Width - g.MeasureString(kvp.Key, nodeFont).Width), nodeRect.Y);
                    if (kvp.Value.output.Any()) {
                        switch (kvp.Value.getType()) {
                            case NodeProperties.Type.CHANNELS:
                                g.FillEllipse(brushChannels, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.COLOR:
                                g.FillEllipse(brushColors, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.VECTORS:
                                g.FillEllipse(brushVectors, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            default:
                                g.FillEllipse(Brushes.Black, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                        }
                    } else {
                        switch (kvp.Value.getType()) {
                            case NodeProperties.Type.CHANNELS:
                                g.DrawEllipse(linkChannels, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.COLOR:
                                g.DrawEllipse(linkColors, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            case NodeProperties.Type.VECTORS:
                                g.DrawEllipse(linkVectors, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                            default:
                                g.DrawEllipse(Pens.Black, nodeRect.Right - ballSize / 2, nodeRect.Y + ballOffset, ballSize, ballSize);
                                break;
                        }
                    }
                    nodeRect.Y += nodeFont.Height;
                }
            }
        }
    }
}
