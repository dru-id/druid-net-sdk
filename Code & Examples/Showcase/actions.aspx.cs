using System;
using System.IO;
using DruIDSdk;

namespace Showcase
{
    public partial class ckactions : System.Web.UI.Page
    {
        Identity identity = new Identity($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Config");

        protected void Page_Load(object sender, EventArgs e)
        {

            UrlBuilder urlBuilder = new UrlBuilder(identity);
            string urlTo="", scope = "";

            if (!string.IsNullOrEmpty(Request["action"]))
            {
                switch (Request["action"].Trim())
                {
                    case "login":
                        urlTo = urlBuilder.getUrlLogin();
                        break;
                    case "register":
                        scope = "";
                        if (!string.IsNullOrEmpty(Request["scope"].Trim()))
                        {
                            scope = Request["scope"].Trim();
                        }
                        urlTo = urlBuilder.getUrlRegister(scope);
                        break;
                    case "editaccount":
                        urlTo = urlBuilder.getUrlEditAccount();
                        break;
                    case "logout":
                        identity.synchronizeSessionWithServer();
                        identity.logoutUser();
                        urlTo = Session["last_url_oauth"] + "?" + Request.QueryString.ToString();
                        break;
                    case "completeaccount":
                        if (!string.IsNullOrEmpty(Request["scope"]))
                        {
                            urlTo = urlBuilder.getUrlCompleteAccount(Request["scope"]);
                            //urlTo = HttpUtility.UrlEncode(urlTo);
                        }
                        break;
                    case "signuppromotion":
                        identity.synchronizeSessionWithServer();
                        if (!string.IsNullOrEmpty(Request["scope"]))
                        {
                            scope = "";
                            if (!string.IsNullOrEmpty(Request["scope"]))
                            {
                                Session["scope"] = Request["scope"];
                            }
                        }
                        urlTo = urlBuilder.buildSignupPromotionUrl(Request["scope"]);
                        break;
                }
            }
            if (!string.IsNullOrEmpty(urlTo))
            {
                Response.Redirect(urlTo, false);
            }
        }
    }
}