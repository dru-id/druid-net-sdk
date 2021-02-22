using System;
using System.IO;
using DruIDSdk;
namespace Showcase
{
    public partial class sampleListDataUsers : System.Web.UI.Page
    {
        Identity identity = new Identity($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Config");
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["last_url_oauth"] = Request.Path;
            string oid = "c8c9be7631860f7d4082401b056fd3db98d29546";

            identity.synchronizeSessionWithServer();

            if (!identity.isConnected())
            {
                if (!string.IsNullOrEmpty(Request["code"]))
                {
                    identity.authorizeUser(Request["code"]);
                    identity.synchronizeSessionWithServer();
                }
            }

            User usuarios = identity.getDataUsers(oid, identity.druIDThings.getClientToken());
            HTML_UserId.InnerHtml = oid;
            html_pre.InnerHtml += "Id: " + usuarios.getId();
            html_pre.InnerHtml += "<br>Nick: " + usuarios.getEmail();
            html_pre.InnerHtml += "<br>" + usuarios.raw.ToSafeString();
        }
    }
}