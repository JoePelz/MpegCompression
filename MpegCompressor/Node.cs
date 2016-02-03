using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpegCompressor {
    //TODO: have node not extend panel, and be drawn purely by NodeView.
    public class Node : Panel, IViewable, IProperties {
        private bool isSelected;
        protected Label name;
        protected Dictionary<string, Property> properties;

        public Node() {
            SuspendLayout();
            name = new Label();
            name.Text = "NoOp";
            name.AutoSize = true;
            name.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(name);

            AutoSize = false;
            Size = new System.Drawing.Size(100, 100);
            BackColor = System.Drawing.Color.CadetBlue;
            Margin = new Padding(5);
            ResumeLayout();

            properties = new Dictionary<string, Property>();
            createProperties();
        }

        protected virtual void createProperties() {
            Property p = new Property();
            p.createString("NoOp", "Name of the control");
            p.eValueChanged += (s, e) => name.Text = (s as Property).getString();
            properties.Add("name", p);
        }

        public virtual void view(PaintEventArgs pe) {
            //if there is input, 
            //   delegate to upstream
            //else
            //   draw nothing
            Graphics g = pe.Graphics;
            g.DrawLine(SystemPens.ControlDarkDark, -5, -15, 5, 15);
            g.DrawLine(SystemPens.ControlDarkDark, 5, -15, -5, 15);
        }

        public Dictionary<string, Property> getProperties() {
            return properties;
        }

        private void updateNodeDisplay() {
            SuspendLayout();
            BackColor = isSelected ? System.Drawing.Color.Wheat : System.Drawing.Color.CadetBlue;
            ResumeLayout();
        }

        public void setSelected(bool value) {
            isSelected = value;
            updateNodeDisplay();
        }
        public bool getSelected() {
            return isSelected;
        }
    }
}
