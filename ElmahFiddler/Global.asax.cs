using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Elmah;
using Fiddler;

namespace ElmahFiddler {
    public class Global : HttpApplication {
        protected void Application_Start(object sender, EventArgs e) {}

        protected void Session_Start(object sender, EventArgs e) {}

        protected void Application_BeginRequest(object sender, EventArgs e) {}

        protected void Application_AuthenticateRequest(object sender, EventArgs e) {}

        protected void Application_Error(object sender, EventArgs e) {}

        protected void Session_End(object sender, EventArgs e) {}

        protected void Application_End(object sender, EventArgs e) {}
    }
}