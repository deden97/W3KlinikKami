using Microsoft.Reporting.WebForms;
using System;
using System.Linq;
using System.Web.UI;
using W3KlinikKami.Core;
using W3KlinikKami.Models;

namespace W3KlinikKami.Report
{
    public partial class DataPenangananPasien : Page
    {
        private bool CekSession()
        {
            int id = csmSession.GetIdSession();
            string jabatan = csmSession.GetJabatanSession();
            return new DbEntities().TB_USER.Any(d => d.ID == id && d.JABATAN == jabatan);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (CekSession())
                {
                    if (int.TryParse(Request.QueryString["idKunjunganPasien"], out int idKunjunganPasien))
                    {
                        var reportView = this.ReportViewer1.LocalReport;
                        object data = new RiwayatPasien().GetDetailPenangananById(idKunjunganPasien);

                        reportView.ReportPath = Server.MapPath("~/Report/RDLC/DataPenangananPasien.rdlc");
                        reportView.DataSources.Clear();
                        reportView.DataSources.Add(new ReportDataSource("DataSet1", data));
                    }
                }
                else
                {
                    FlashMessage.SetFlashMessage("Anda Harus Login Terlebih Dahulu.", FlashMessage.FlashMessageType.Warning);
                    Response.Redirect("~/");
                }
            }
        }
    }
}