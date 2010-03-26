using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ElmahFiddler {
    /// <summary>
    /// Optional SAZ module configuration
    /// </summary>
    public class ElmahMailSAZConfig {
        /// <summary>
        /// Password for SAZ file
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// List of URLs to exclude from SAZ
        /// </summary>
        public IList<Regex> ExcludedUrls { get; private set; }

        /// <summary>
        /// Keep only the last N requests before the error
        /// Only used by <see cref="ElmahMailSAZTraceModule"/>
        /// </summary>
        public int? KeepLastNRequests { get; set; }

        /// <summary>
        /// Rename host in SAZ requests
        /// </summary>
        public string RenameHost { get; set; }

        public ElmahMailSAZConfig() {
            ExcludedUrls = new List<Regex>();
        }
    }
}