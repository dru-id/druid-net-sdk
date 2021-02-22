using System;
using System.IO;
using DruIDSdk;
namespace Showcase
{
    public partial class samplePromotionAccount : System.Web.UI.Page
    {
        Identity identity = new Identity($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Config");
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["last_url_oauth"] = Request.Path;
            identity.synchronizeSessionWithServer();
            if (!identity.isConnected())
            {
                if (!string.IsNullOrEmpty(Request["code"]))
                {
                    identity.authorizeUser(Request["code"]);
                    identity.synchronizeSessionWithServer();
                }
            }
            UrlBuilder urlBuilder = new UrlBuilder(identity);

            bool userCompletePromo1 = this.identity.checkUserComplete(identity.oauth_config.sections["promo1"]);
            bool userCompletePromo2 = this.identity.checkUserComplete(identity.oauth_config.sections["promo2"]);
            bool userCompletePromo3 = this.identity.checkUserComplete(identity.oauth_config.sections["promo3"]);

            html_pre_promo1.InnerHtml = "Sección Promo 1 - (nombre, apellidos)<br/>";
            if (userCompletePromo1)
            {
                html_pre_promo1.InnerHtml += "<strong><a href=\"javascript:alert('apuntado');\">Apuntate en la promo 1</a><strong><br/>";
            }
            else
            {
                html_pre_promo1.InnerHtml += "<strong><a href=\"/ckactions.aspx?action=signuppromotion&scope=" + identity.oauth_config.sections["promo1"] + "\">Apuntate en la promo 1</a></strong>";
            }

            html_pre_promo2.InnerHtml = "Sección Promo 2 - (sexo,provincia,poblacion)<br/>";
            if (userCompletePromo2)
            {
                html_pre_promo2.InnerHtml += "<strong><a href=\"javascript:alert('apuntado');\">Apuntate en la promo 2</a><strong><br/>";
            }
            else
            {
                html_pre_promo2.InnerHtml += "<strong><a href=\"/ckactions.aspx?action=signuppromotion&scope=" + identity.oauth_config.sections["promo2"] + "\">Apuntate en la promo 2</a></strong>";
            }

            html_pre_promo3.InnerHtml = "Sección Promo 3 - (todos los datos)<br/>";
            if (userCompletePromo3)
            {
                html_pre_promo3.InnerHtml += "<strong><a href=\"javascript:alert('apuntado');\">Apuntate en la promo 3</a><strong><br/>";
            }
            else
            {
                html_pre_promo3.InnerHtml += "<strong><a href=\"/ckactions.aspx?action=signuppromotion&scope=" + identity.oauth_config.sections["promo3"] + "\">Apuntate en la promo 3</a></strong>";
            }
        }
    }
}