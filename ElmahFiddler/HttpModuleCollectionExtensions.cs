using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElmahFiddler {
    public static class HttpModuleCollectionExtensions {
        public static IEnumerable<IHttpModule> AsEnumerable(this HttpModuleCollection modules) {
            return modules.Cast<string>().Select(x => modules[x]);
        }
    }
}