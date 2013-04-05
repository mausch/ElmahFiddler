using System;
using Fiddler;

namespace ElmahFiddler {
    public static class SessionExtensions {
        public static string Protocol(this Session session) {
            if (session.isFTP)
                return Uri.UriSchemeFtp;
            if (session.isHTTPS)
                return Uri.UriSchemeHttps;
            return Uri.UriSchemeHttp;
        }
    }
}