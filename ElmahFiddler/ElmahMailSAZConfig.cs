using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ElmahFiddler {
    public class ElmahMailSAZConfig {
        public string Password { get; set; }
        public IList<Regex> ExcludedUrls { get; private set; }
        public int? KeepLastNRequests { get; set; }
        public ElmahMailSAZConfig() {
            ExcludedUrls = new List<Regex>();
        }
    }
}