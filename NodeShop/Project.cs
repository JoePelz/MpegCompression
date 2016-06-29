using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeShop {
    public class Project {
        //project filename
        public string filename;
        //project path
        public string path;
        //do unsaved changes exist?
        public bool dirty;

        //collection of nodes
        private List<Nodes.Node> nodes;
        //selection tracking
        public Nodes.Node selectedNode;
        public List<Nodes.Node> selectedNodes;

        //node connections (stored in node, saved by ID)
        //node settings (stored in node itself)

        /// <summary>
        /// Construct a new blank project.
        /// </summary>
        public Project() {
            filename = "";
            path = "";
            dirty = false;
            nodes = new List<Nodes.Node>();
            selectedNodes = new List<Nodes.Node>();
        }

        /// <summary>
        /// Add a node to the project.  Does not check for duplicates.
        /// </summary>
        /// <param name="node">The node to add the project.</param>
        public void addNode(Nodes.Node node) {
            nodes.Add(node);
        }

        public List<Nodes.Node> getNodes() {
            return nodes;
        }

        public bool isDirty() {
            return dirty;
        }

        public void clean() {
            dirty = false;
        }

        //Read project
        public int readProject(StreamReader stream) {
            List<string[]> connections = new List<string[]>();

            //read file line by line and build nodes.
            Nodes.Node n = readNode(stream, connections);

            while (n != null) {
                nodes.Add(n);
                n = readNode(stream, connections);
            }

            //build connections in nodes
            readConnections(connections);
            return 0;
        }

        private void readConnections(List<string[]> connections) {
            foreach(string[] link in connections) {
                Nodes.Node src = nodes.Find(i => i.getID() == link[0]);
                Nodes.Node dst = nodes.Find(i => i.getID() == link[2]);
                if (src == null || dst == null) {
                    Console.WriteLine(string.Format("Couldn't link nodes {0}.{1} to {2}.{3}", link[0], link[1], link[2], link[3]));
                } else {
                    Nodes.Node.connect(src, link[1], dst, link[3]);
                }
            }
        }

        private Nodes.Node readNode(StreamReader stream, List<string[]> connections) {
            char[] delimiter = { ' ', '=' };
            string nodeType;
            Dictionary<string, string> properties = new Dictionary<string, string>();
            
            //read node name
            string line = stream.ReadLine();
            if (line == null) {
                return null;
            }
            string[] bits = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            if (bits.Length != 2 || bits[1] != "{") {
                Console.WriteLine("Error parsing: \"" + line + "\"");
                return null;
            }
            nodeType = bits[0];
            Console.WriteLine("Processing a " + nodeType);

            line = stream.ReadLine();

            line = line.Trim(' ', '\t');
            int tokenPos = line.IndexOf(" = ");
            
            while (tokenPos != -1) {
                properties[line.Substring(0, tokenPos)] = line.Substring(tokenPos+3);
                line = stream.ReadLine();
                line = line.Trim(' ', '\t');
                tokenPos = line.IndexOf(" = ");
            }

            return Nodes.Node.spawnNode(nodeType, properties, connections);
        }

        //Write project
        public int writeProject(StreamWriter stream) {
            //validate path is writeable (and doesn't already exist?)
            //write out data there in lines
            foreach(Nodes.Node n in nodes) {
                n.writeNode(stream);
            }

            return 0;
        }

        public void newProject() {
            filename = "";
            path = "";
            dirty = false;
            nodes.Clear();
            selectedNodes.Clear();
            selectedNode = null;
        }
    }
}
