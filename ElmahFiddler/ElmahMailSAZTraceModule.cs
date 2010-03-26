using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.SessionState;
using Elmah;
using Fiddler;

namespace ElmahFiddler {
    public class ElmahMailSAZTraceModule : IHttpModule {
        private static readonly object sazFilenameKey = new object();
        private static readonly object attachmentKey = new object();
        private static readonly string sessionKey = "ElmahMailSAZTraceModule" + Guid.NewGuid();
        private ElmahMailSAZConfig config;

        public void Init(HttpApplication context) {
            var modules = from string n in context.Modules select context.Modules[n];
            var mailModule = modules.FirstOrDefault(m => m is ErrorMailModule) as ErrorMailModule;
            if (mailModule == null)
                return;
            ((SessionStateModule) context.Modules["Session"]).Start += SessionStart;
            context.PreRequestHandlerExecute += PreRequestHandlerExecute;
            config = (ElmahMailSAZConfig) ConfigurationManager.GetSection("elmah/errorMailSAZ") ?? new ElmahMailSAZConfig();
            mailModule.Mailing += MailModuleMailing;
            mailModule.Mailed += MailModuleMailed;
        }

        private void PreRequestHandlerExecute(object sender, EventArgs e) {
            var app = (HttpApplication)sender;
            if (config.ExcludedUrls.Any(rx => rx.IsMatch(app.Request.RawUrl)))
                return;
            if (!(app.Context.CurrentHandler is IRequiresSessionState))
                return;
            var req = app.Request.SerializeRequestToBytes();
            ((List<byte[]>)app.Session[sessionKey]).Add(req);
        }

        private void SessionStart(object sender, EventArgs e) {
            HttpContext.Current.Session[sessionKey] = new List<byte[]>();
        }

        public void MailModuleMailed(object sender, ErrorMailEventArgs args) {
            var saz = HttpContext.Current.Items[sazFilenameKey] as string;
            if (saz == null)
                return;
            var attachment = HttpContext.Current.Items[attachmentKey] as Attachment;
            attachment.Dispose();
            File.Delete(saz);
        }

        public void MailModuleMailing(object sender, ErrorMailEventArgs args) {
            var saz = SerializeRequestToSAZ();
            if (saz == null)
                return;
            var saz2 = saz + ".saz";
            File.Move(saz, saz2);
            var attachment = new Attachment(saz2);
            HttpContext.Current.Items[sazFilenameKey] = saz2;
            HttpContext.Current.Items[attachmentKey] = attachment;
            args.Mail.Attachments.Add(attachment);
        }

        public string SerializeRequestToSAZ() {
            var filename = Path.GetTempFileName();
            var sessions = ((IEnumerable<byte[]>) HttpContext.Current.Session[sessionKey]).Select(b => new Session(b, null)).ToArray();
            var ok = SAZ.WriteSessionArchive(filename, sessions, config.Password);
            return ok ? filename : null;
        }

        public void Dispose() {}
    }
}