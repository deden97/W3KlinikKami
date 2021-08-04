<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="KartuPasien.aspx.cs" Inherits="W3KlinikKami.Report.DataPasien" %>

<%@ Register Assembly="System.Web.Mvc" Namespace="System.Web.Mvc" TagPrefix="cc1" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Kartu Pasien</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>
