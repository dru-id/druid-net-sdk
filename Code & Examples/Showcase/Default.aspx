<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Showcase.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>.Net DruID SDK Demo</title>
    <meta charset="UTF-8">
   
    <link href="/styles/styles.css" rel="stylesheet"    
        type="text/css" />
    <script src="http://register.sandbox.cocacola.es/login/sso"></script>
    
</head>
<body>
		<!--- Content -->

    <form id="form1" runat="server">
		<div id="main-body">
			<div id="main-body-inner" class="ie-rel-fix">
				<div class="section">
					<h1>Ejemplo 1: Integración con registro</h1>
                    <h2>Lista de ejemplos:</h2>
                    <ul>
                    <li><a href="/Default.aspx">Ejemplo 1: Integración con registro</a></li>
                    <li><a href="/sampleListDataUsers.aspx">Ejemplo 4: Consulta de datos de usuario</a></li>
                    <li><a href="/sampleViewLoggedUserData.aspx">Ejemplo 5: Consulta de datos del usuario logado</a></li>
                    <li><a href="/sampleCompleteAccount.aspx">Ejemplo 6: Completar datos del usuario</a></li>
                    <li><a href="/samplePromotionAccount.aspx">Ejemplo 7: Proceso registro en promoción (Sección)</a></li>
                    </ul>
                    <div>
                        <asp:HyperLink ID="lnkLogin" runat="server">login</asp:HyperLink>
                        |<asp:HyperLink ID="lnkSignup" runat="server">sign up</asp:HyperLink>
                        |<asp:HyperLink ID="lnkEditar" runat="server" Visible="False">editar</asp:HyperLink>
                        |<asp:HyperLink ID="lnkRecargar" runat="server" Visible="False" NavigateUrl="/">recargar</asp:HyperLink>
                        |<asp:HyperLink ID="lnkLogout" runat="server" Visible="False" 
                            NavigateUrl="/?logout=1">logout</asp:HyperLink>
                        |<asp:HyperLink ID="lnkLoginStatus" runat="server" Visible="False" 
                            NavigateUrl="/?login_status=1">login-status</asp:HyperLink>
                        |<asp:HyperLink ID="lnkUserLoged" runat="server" Visible="False" 
                            NavigateUrl="/?get_uid=1">user-logged</asp:HyperLink>
                        |<asp:HyperLink ID="lnkRefreshToken" runat="server" Visible="False" 
                            NavigateUrl="/?refresh_token=1">refresh_token</asp:HyperLink>
                    </div>
                </div>
			</div>
		</div>
    </form>
        <div id="html_info" runat=server>
        </div>
</body>
</html>
