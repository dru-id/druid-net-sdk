using System;
using System.IO;
using DruIDSdk;using System.Diagnostics;

namespace Showcase
{
    public partial class sampleCompleteAccount : System.Web.UI.Page
    {

        Identity identity = new Identity($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Config");

        protected void Page_Load(object sender, EventArgs e)
        {
            identity.synchronizeSessionWithServer();

            if (!identity.isConnected())
            {
                if (!string.IsNullOrEmpty(Request["code"]))
                {
                    Debug.WriteLine("No tengo code");
                }
                else
                {
                    Debug.WriteLine("Tengo código de autorización (code). Puedo Autorizar al usuario y obtener un access_token ");
                    identity.authorizeUser(Request["code"]);
                    identity.synchronizeSessionWithServer();
                }
            }

            if (identity.isConnected())
            {
                bool userCompletePromo1 = this.identity.checkUserComplete(identity.oauth_config.sections["promo1"]);
                bool userCompletePromo2 = this.identity.checkUserComplete(identity.oauth_config.sections["promo2"]);
                bool userCompletePromo3 = this.identity.checkUserComplete(identity.oauth_config.sections["promo3"]);

                if (userCompletePromo1)
                {
                    html_pre.InnerHtml += "<strong>Usuario Completo en la Sección 1</strong> (nombre,apellidos)</br>";
                }
                else
                {
                    html_pre.InnerHtml+="Necesita completar Datos en la Sección 1 (nombre,apellidos) - ";
                    html_pre.InnerHtml += "<a href='/ckactions.aspx?action=completeaccount&scope=" + identity.oauth_config.sections["promo1"] + "'>completar sección 1</a><br></br>";
                }
                
                if (userCompletePromo2)
                {
                    html_pre.InnerHtml += "<strong>Usuario Completo en la Sección 2</strong> (sexo,provincia,poblacion)</br>";
                }
                else
                {
                    html_pre.InnerHtml += "Necesita completar Datos en la Sección 2 - ";
                    html_pre.InnerHtml += "<a href='/ckactions.aspx?action=completeaccount&scope=" + identity.oauth_config.sections["promo2"] + "'>completar sección 2</a><br></br>";
                }
                
                if (userCompletePromo3)
                {
                    html_pre.InnerHtml += "<strong>Usuario Completo en la Sección 3</strong> (todos los datos)</br>";
                }
                else
                {
                    html_pre.InnerHtml += "Necesita completar Datos en la Sección 3 (todos los datos)- ";
                    html_pre.InnerHtml += "<a href='/ckactions.aspx?action=completeaccount&scope=" + identity.oauth_config.sections["promo3"] + "'>completar sección 3</a><br></br>";
                }
            }


        }
    }
}