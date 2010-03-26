using System;
using System.Web.UI;

namespace ElmahFiddlerApp {
    public class Default : Page {
        protected void Page_Load(object sender, EventArgs e) {
            if (Request.Form.Count > 0 || Request.QueryString.Count > 0)
                throw new Exception();
        }
    }
}