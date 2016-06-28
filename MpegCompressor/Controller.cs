using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MpegCompressor.Nodes;
using System.IO;

namespace MpegCompressor {
    class Controller {
        private PropertyPage viewProps;
        private Viewport viewLeft;
        private Viewport viewRight;
        private NodeView viewNodes;
        private Project project;

        public Controller(PropertyPage props, NodeView nodes, Viewport left, Viewport right) {
            project = new Project();
            viewProps = props;
            viewLeft  = left;
            viewRight = right;
            viewNodes = nodes;
            viewNodes.eSelectionChanged += OnSelectionChange;

            viewNodes.setProject(project);

            buildGraph();
            viewLeft.focusView();
            viewRight.focusView();
            viewNodes.focusView();
        }
        
        public void mergeTest() {
            Node nR1 = new ReadImage();
            Node nR2 = new ReadImage();
            Node nM = new Merge();

            nR1.setPos(0, -50);
            nR2.setPos(0, 50);
            nM.setPos(180, 0);

            (nR1 as ReadImage).setPath("C:\\temp\\uv.jpg");
            (nR2 as ReadImage).setPath("C:\\temp\\lena.tif");

            Node.connect(nR1, "outColor", nM, "inColorA");
            Node.connect(nR2, "outColor", nM, "inColorB");

            viewNodes.addNode(nR1);
            viewNodes.addNode(nR2);
            viewNodes.addNode(nM);
        }
        
        public void buildGraph() {            
            mergeTest();
        }
        
        //Open existing project from file
        public void open(string path = "") {
            //Are you sure you want to discard changes in the current project?
            if (project.isDirty() && Dialogs.discardChanges() == false) {
                return;
            }

            //out with the old...
            viewLeft.setSource(null);
            viewRight.setSource(null);
            string oldPath = project.path;
            project.newProject();

            if (path.Length == 0) {
                if (oldPath.Length != 0)
                    path = Dialogs.open(oldPath);
                else
                    path = Dialogs.open();
            }
            if (path.Length != 0) {
                // Check file is readable and
                // Open the selected file to read.
                Stream fileStream = null;
                try {
                    fileStream = File.OpenRead(path);
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(fileStream)) {
                        //call readProject to read in that file.
                        project.readProject(reader);
                        int fnameIndex = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                        project.path = path.Substring(0, fnameIndex + 1);
                        project.filename = path.Substring(fnameIndex + 1);
                    }
                } catch (Exception e) {
                    MessageBox.Show("Error: " + e.Message);
                } finally {
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
        }

        //Save current project
        public void save(string path = "") {
            if (path.Length == 0) {
                if (project.path.Length != 0)
                    path = Dialogs.save(project.path);
                else
                    path = Dialogs.save();
            }
            if (path.Length != 0) {
                Stream fileStream = File.OpenWrite(path);

                using (StreamWriter writer = new StreamWriter(fileStream)) {
                    //call readProject to read in that file.
                    project.writeProject(writer);
                    int fnameIndex = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                    project.path = path.Substring(0, fnameIndex + 1);
                    project.filename = path.Substring(fnameIndex + 1);
                }
                fileStream.Close();
            }
        }

        //New project
        public void newProject() {
            if (Dialogs.discardChanges()) {
                viewLeft.setSource(null);
                viewRight.setSource(null);
                project.newProject();
                viewRight.Invalidate();
                viewLeft.Invalidate();
                viewNodes.Invalidate();
            }
        }
        public void OnSelectionChange(object sender, EventArgs e) {
            viewProps.clearProperties();
            Node selection = viewNodes.getSelection();
            if (selection != null) {
                viewProps.showProperties(selection);
            }
        }

        public bool HotKeys(Keys keys) {
            Node n;
            bool handled = true;  //has this function handled the hotkey?

            //The hotkeys for viewing a node are here instead of in the NodeView because
            //the controller has necessary access to both the NodeView and the Viewport
            switch (keys) {
                case Keys.D1:
                    if (viewNodes.Focused) {
                        n = viewNodes.getSelection();
                        if (n != null) {
                            //load left view with selected node
                            viewLeft.setSource(n);
                            viewLeft.Invalidate();
                        }
                    } else {
                        handled = false;
                    }
                    break;
                case Keys.D2:
                    if (viewNodes.Focused) {
                        n = viewNodes.getSelection();
                        if (n != null) {
                            //load right view with selected node
                            viewRight.setSource(n);
                            viewRight.Invalidate();
                        }
                    } else {
                        handled = false;
                    }
                    break;
                case Keys.S | Keys.Control | Keys.Shift:
                    save("");
                    break;
                case Keys.S | Keys.Control:
                    save(project.path + project.filename);
                    break;
                case Keys.O | Keys.Control:
                    open();
                    break;
                case Keys.N | Keys.Control:
                    newProject();
                    break;
                default:
                    handled = false;
                    break;
            }
            return handled;
        }
    }
}
