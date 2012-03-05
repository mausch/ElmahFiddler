using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Elmah;
using Fiddler;

namespace ElmahFiddler {
    public class ElmahMailSAZModule : IHttpModule {
        private static readonly object sazFilenameKey = new object();
        private static readonly object attachmentKey = new object();
        private ElmahMailSAZConfig config;

        public void Init(HttpApplication context) {
            var mailModule = context.Modules.AsEnumerable().OfType<ErrorMailModule>().FirstOrDefault();
            if (mailModule == null)
                throw new Exception(string.Format("{0} requires {1} to be installed before it", GetType().Name, typeof(ErrorMailModule).Name));
            config = (ElmahMailSAZConfig) ConfigurationManager.GetSection("elmah/errorMailSAZ") ?? new ElmahMailSAZConfig();
            mailModule.Mailing += MailModuleMailing;
            mailModule.Mailed += MailModuleMailed;
        }

        public void MailModuleMailed(object sender, ErrorMailEventArgs args) {
            var context = HttpContext.Current;
            if (context == null)
                return;
            MailModuleMailed(new HttpContextWrapper(context));
        }

        public static void MailModuleMailed(HttpContextBase context) {
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
            var attachment = MailModuleMailing(config, new HttpContextWrapper(context));
            if (attachment != null)
                args.Mail.Attachments.Add(attachment);
        }

        public static Attachment MailModuleMailing(ElmahMailSAZConfig config, HttpContextBase context) {
            if (config.ExcludedUrls.Any(rx => rx.IsMatch(context.Request.RawUrl)))
                return null;
            var saz = SerializeRequestToSAZ(config, context.Request);
            if (saz == null)
                return null;
            var saz2 = saz + ".saz";
            File.Move(saz, saz2);
            var attachment = new Attachment(saz2);
            context.Items[sazFilenameKey] = saz2;
            context.Items[attachmentKey] = attachment;
            return attachment;
        }

        public static string SerializeRequestToSAZ(ElmahMailSAZConfig config, HttpRequestBase request) {
            var filename = Path.GetTempFileName();
            var session = new Session(request.SerializeRequestToBytes(config.RenameHost), null);
            var ok = SAZ.WriteSessionArchive(filename, new[] {session}, config.Password);
            if (!ok) {
                File.Delete(filename);
                return null;
            }
            return filename;
        }

        public void Dispose() {}
    }
}