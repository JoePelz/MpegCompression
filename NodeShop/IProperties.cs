using NodeShop.NodeProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeShop {
    public interface IProperties {
        Dictionary<string, Property> getProperties();
    }
}
