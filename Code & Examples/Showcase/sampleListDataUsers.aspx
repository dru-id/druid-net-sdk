<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="sampleListDataUsers.aspx.cs" Inherits="Showcase.sampleListDataUsers" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="/styles/styles.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
    	<div id="main-body">
			<div id="main-body-inner" class="ie-rel-fix">
				<div class="section">
					<h1>Ejemplo 4: Consulta por ID</h1>
                    <h2>Lista de ejemplos:</h2>
                    <ul>
                    <li><a href="/Default.aspx">Ejemplo 1: Integración con registro</a></li>
                    <li><a href="/sampleListDataUsers.aspx">Ejemplo 4: Consulta de datos de usuario</a></li>
                    <li><a href="/sampleViewLoggedUserData.aspx">Ejemplo 5: Consulta de datos del usuario logado</a></li>
                    <li><a href="/sampleCompleteAccount.aspx">Ejemplo 6: Completar datos del usuario</a></li>
                    <li><a href="/samplePromotionAccount.aspx">Ejemplo 7: Proceso registro en promoción (Sección)</a></li>
                    <li><a href="/sampleListCokeIdDataUserByNick.aspx">Ejemplo 8: Consulta de datos de usuario por nick</a></li>
                    </ul>
                    <p>Consulta Usuario Sección Completa</p>
                    <p>Consulta de datos del usuario <span id="HTML_UserId" runat=server></span> </p>
					<pre id="html_pre" runat=server></pre>
				</div>
			</div>
		</div>
    </div>
    </form>
</body>
</html>
