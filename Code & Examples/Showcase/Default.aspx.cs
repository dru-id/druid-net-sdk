using System;
using System.IO;
using DruIDSdk;using System.Diagnostics;

namespace Showcase
{
    public partial class index : System.Web.UI.Page
    {
        Identity identity;
        protected void Page_Load(object sender, EventArgs e)
        {
            string configFolder = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Config";

            identity = new Identity(configFolder);


            string do_logout = string.IsNullOrEmpty(Request["logout"]) ? "" : Request["logout"];
            string do_login_status = string.IsNullOrEmpty(Request["login_status"]) ? "" : Request["login_status"];
            string do_refresh_token = string.IsNullOrEmpty(Request["refresh_token"]) ? "" : Request["refresh_token"];
            string do_get_uid = string.IsNullOrEmpty(Request["get_uid"]) ? "" : Request["get_uid"];

            OAuthConfigParser oauth_config = identity.oauth_config;
            OAuth client = identity.oauth_client;

            identity.synchronizeSessionWithServer();

            // Tener acceso al generador de URLs
            UrlBuilder urlBuilder = new UrlBuilder(identity);

            //Puedo obtener el client token sin estar logado, por ejemplo para obtener el header de coca-cola
            ClientToken client_token = identity.druIDThings.getClientToken();

            Debug.WriteLine("El client_token es: " + client_token.getValue());

            if (!identity.isConnected())
            {
                if (!string.IsNullOrEmpty(Request["code"]))
                {
                    identity.authorizeUser(Request["code"]);
                    identity.synchronizeSessionWithServer();
                }
                else
                {

                    Debug.WriteLine("No tengo code ");
                    lnkLogin.Visible = true;
                    lnkLogin.NavigateUrl = urlBuilder.getUrlLogin();
                    lnkSignup.Visible = true;
                    lnkSignup.NavigateUrl = urlBuilder.getUrlRegister();

                }
            }
            if (identity.isConnected())
            {
                Debug.WriteLine("Usuario conectado");
                AccessToken access_token = identity.druIDThings.getUserToken();
                Debug.WriteLine("el access token es: " + access_token.getValue());

                if (do_logout.Equals("1"))
                {
                    identity.logoutUser();

                    html_info.InnerHtml = " ... <a href=' / '>continuar</a>";
                    lnkLogin.Visible = false;
                    lnkSignup.Visible = false;
                }
                else
                {
                    if (do_login_status.Equals("1"))
                    {
                        Debug.WriteLine("Haciendo login_status");
                        if (identity.isConnected())
                        {
                            html_info.InnerHtml = "<div><strong style='color:green'>Estas conectado.</strong></div>";
                        }
                        else
                        {
                            html_info.InnerHtml = "<div><strong style='color:red'>NO estas conectado.</strong></div>";
                        }
                    }

                    if (do_get_uid.Equals("1"))
                    {
                        Debug.WriteLine("Recuperando el User Logado");
                        User currentUser = identity.getDataUsers("c8c9be7631860f7d4082401b056fd3db98d29546", access_token);

                        html_info.InnerHtml = "Nick: <strong>" + currentUser.getEmail() + "</strong>";
                    }
                    lnkLogin.Visible = false;
                    lnkSignup.Visible = false;
                    lnkEditar.Visible = true;
                    lnkEditar.NavigateUrl = urlBuilder.getUrlEditAccount();
                    lnkRecargar.Visible = true;
                    lnkLogout.Visible = true;
                    lnkLoginStatus.Visible = true;
                    lnkUserLoged.Visible = true;
                    lnkRefreshToken.Visible = true;
                }
            }
            else
            {

                Debug.WriteLine("Usuario NO conectado");

                lnkEditar.Visible = false;
                lnkRecargar.Visible = false;
                lnkLogout.Visible = false;
                lnkLoginStatus.Visible = false;
                lnkUserLoged.Visible = false;
                lnkRefreshToken.Visible = false;
            }
        }
    }
}