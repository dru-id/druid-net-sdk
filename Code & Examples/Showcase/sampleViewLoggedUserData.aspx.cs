using System;
using System.IO;
using DruIDSdk;

namespace Showcase
{
    public partial class sampleViewLoggedUserData : System.Web.UI.Page
    {

        Identity identity = new Identity($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Config");
        protected void Page_Load(object sender, EventArgs e)
        {
            identity.synchronizeSessionWithServer();
            if (!identity.isConnected())
            {
                if (!string.IsNullOrEmpty(Request["code"]))
                {
                    identity.authorizeUser(Request["code"]);
                    identity.synchronizeSessionWithServer();
                }
            }

            AccessToken accessToken= identity.druIDThings.getUserToken();
            
            if(accessToken!=null){
                User usrUserLogged = identity.getUserLogged();
                html_pre.InnerHtml = "<strong>Usuario Logado</strong></br>";
                html_pre.InnerHtml += "<p>" + usrUserLogged.getEmail() + "</p>";
                html_pre.InnerHtml += "<p>" + usrUserLogged.raw.ToSafeString() + "</p>";
            }else{
                html_pre.InnerHtml="no hay usuario logado";
            }
        }
    }
}