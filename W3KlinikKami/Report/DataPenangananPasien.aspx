<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DataPenangananPasien.aspx.cs" Inherits="W3KlinikKami.Report.DataPenangananPasien" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Data Penanganan Pasien</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <strong>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%" Height="699px"></rsweb:ReportViewer>
            </strong>
        </div>
    </form>
</body>
</html>
