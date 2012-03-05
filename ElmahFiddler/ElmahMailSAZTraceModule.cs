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

        private static SessionStateModule GetSessionStateModule(HttpApplication app) {
            var m = app.Modules["Session"];
            if (m == null)
                return null;
            if (!(m is SessionStateModule))
                return null;
            return (SessionStateModule) m;
        }

        public void Init(HttpApplication context) {
            var mailModule = context.Modules.AsEnumerable().OfType<ErrorMailModule>().FirstOrDefault();
            if (mailModule == null)
                throw new Exception(string.Format("{0} requires {1} to be installed before it", GetType().Name, typeof(ErrorMailModule).Name));
            var sessionStateModule = GetSessionStateModule(context);
            if (sessionStateModule == null)
                throw new Exception(string.Format("{0} requires ASP.NET session", GetType().Name));
            sessionStateModule.Start += SessionStart;
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
            var req = SerializeRequest(config, new HttpContextWrapper(app.Context));
            if (req == null)
                return;
            AddRequestToSession(req, config, new HttpSessionStateWrapper(app.Session));
        }

        public static void AddRequestToSession(byte[] req, ElmahMailSAZConfig config, HttpSessionStateBase session) {
            var reqs = ((List<byte[]>)session[sessionKey]);
            reqs.Add(req);
            if (config.KeepLastNRequests.HasValue && reqs.Count > config.KeepLastNRequests)
                reqs.RemoveAt(0);
        }

        public static byte[] SerializeRequest(ElmahMailSAZConfig config, HttpContextBase context) {
            if (config == null)
                throw new ArgumentNullException("config");
            if (context == null)
                throw new ArgumentNullException("context");
            var request = context.Request;
            if (config.ExcludedUrls.Any(rx => rx.IsMatch(request.RawUrl)))
                return null;
            if (!(context.CurrentHandler is IRequiresSessionState))
                return null;
            return request.SerializeRequestToBytes(config.RenameHost);
        }

        private static void SessionStart(object sender, EventArgs e) {
            HttpContext.Current.Session[sessionKey] = new List<byte[]>();
        }

        public void MailModuleMailed(object sender, ErrorMailEventArgs args) {
            var context = HttpContext.Current;
            if (context == null)
                return;
            var saz = context.Items[sazFilenameKey] as string;
            if (saz == null)
                return;
            var attachment = context.Items[attachmentKey] as Attachment;
            if (attachment != null)
                attachment.Dispose();
            File.Delete(saz);
        }

        public void MailModuleMailing(object sender, ErrorMailEventArgs args) {
            var context = HttpContext.Current;
            if (context == null)
                return;
            var attachment = MailModuleMailing(new HttpContextWrapper(context), config);
            if (attachment != null)
                args.Mail.Attachments.Add(attachment);
        }

        public static Attachment MailModuleMailing(HttpContextBase context, ElmahMailSAZConfig config) {
            var saz = SerializeRequestToSAZ(context.Session, config);
            if (saz == null)
                return null;
            var saz2 = saz + ".saz";
            File.Move(saz, saz2);
            var attachment = new Attachment(saz2);
            context.Items[sazFilenameKey] = saz2;
            context.Items[attachmentKey] = attachment;
            return attachment;
        }

        public static string SerializeRequestToSAZ(HttpSessionStateBase session, ElmahMailSAZConfig config) {
            var filename = Path.GetTempFileName();
            var sessions = ((IEnumerable<byte[]>)HttpContext.Current.Session[sessionKey]).Select(b => new Session(b, null)).ToArray();
            var ok = SAZ.WriteSessionArchive(filename, sessions, config.Password);
            if (!ok) {
                File.Delete(filename);
                return null;
            }
            return filename;            
        }

        public void Dispose() {}
    }
}