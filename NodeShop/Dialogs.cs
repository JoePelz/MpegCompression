using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NodeShop {
    public static class Dialogs {
        public static string open(string directory = "C:\\temp") {
            string path = "";
            //create "open" dialog box
            OpenFileDialog ofd = new OpenFileDialog();
            //allow choice of single file *.nsh.
            ofd.Filter = "NodeShop Files|*.nsh|All Files|*";
            ofd.InitialDirectory = directory;
            var result = ofd.ShowDialog();

            // Process input if the user clicked OK.
            if (result == DialogResult.OK) {
                path = ofd.FileName;
            }
            return path;
        }

        public static bool discardChanges() {
            var result = MessageBox.Show("Are you sure you want to discard changes?", "Confirmation", MessageBoxButtons.YesNoCancel);
            return result == DialogResult.Yes;
        }

        public static bool overwriteFile(string fileName) {
            var result = MessageBox.Show("File:\n" + fileName + "\n exists. Overwrite?", "Overwrite", MessageBoxButtons.YesNoCancel);
            return result == DialogResult.Yes;
        }
        //Save current project
        public static string save(string directory = "C:\\temp") {
            string path = "";
            //open prompt to path and filename.nsh

            //create "save" dialog box
            SaveFileDialog sfd = new SaveFileDialog();
            //allow choice of single file *.nsh.
            sfd.Filter = "NodeShop Files|*.nsh|All Files|*";
            sfd.InitialDirectory = directory;
            var result = sfd.ShowDialog();

            // Process input if the user clicked OK.
            if (result == DialogResult.OK) {
                path = sfd.FileName;
            }
            return path;
        }
    }
}
