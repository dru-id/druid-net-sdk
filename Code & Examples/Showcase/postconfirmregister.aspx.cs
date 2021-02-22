using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Showcase
{
    public partial class postconfirmregister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url="/";
            if (!string.IsNullOrEmpty(Session["last_url_oauth"].ToString()))
            {
                url = Session["last_url_oauth"].ToString();
            }
            System.Web.HttpContext.Current.Response.Redirect(url, false);

        }
       
    }
    
}