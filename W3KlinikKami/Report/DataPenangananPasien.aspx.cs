using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace W3KlinikKami.Report
{
    public partial class DataPenangananPasien : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int.TryParse(Request.QueryString["idKunjunganPasien"], out int idKunjunganPasien);
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Report/RDLC/DataPenangananPasien.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.DataSources
                    .Add(new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", new W3KlinikKami.Models.RiwayatPasien().GetDetailPenangananById(idKunjunganPasien)));
            }
        }
    }
}