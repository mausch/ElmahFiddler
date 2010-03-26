using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace ElmahFiddler {
    public class ElmahMailSAZSectionHandler : IConfigurationSectionHandler {
        public object Create(object parent, object configContext, XmlNode section) {
            var moduleConfig = new ElmahMailSAZConfig();

            var excludes = GetChild(section, "exclude");
            if (excludes != null) {
                foreach (XmlNode n in excludes.ChildNodes)
                    moduleConfig.ExcludedUrls.Add(new Regex(n.InnerText, RegexOptions.Compiled | RegexOptions.IgnoreCase));
            }

            var password = GetChild(section, "password");
            if (password != null)
                moduleConfig.Password = password.InnerText;

            var keepLastNRequests = GetChild(section, "keepLastNRequests");
            if (keepLastNRequests != null)
                moduleConfig.KeepLastNRequests = int.Parse(keepLastNRequests.InnerText);

            return moduleConfig;
        }

        private XmlNode GetChild(XmlNode node, string name) {
            return node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == name);
        }
    }
}