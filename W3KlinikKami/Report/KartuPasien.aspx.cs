using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Microsoft.Reporting.WebForms;
using W3KlinikKami.Core;
using W3KlinikKami.Models;

namespace W3KlinikKami.Report
{
    public partial class DataPasien : Page
    {
        private bool CekSession()
        {
            int id = Convert.ToInt32(Session["ID"]);
            string jabatan = Convert.ToString(Session["JABATAN"]);
            using (DbEntities db = new DbEntities())
            {
                if (db.TB_USER.Any(j => j.ID == id && j.JABATAN == jabatan))
                {
                    return nameof(Controllers.ADMController)
                        .Remove(nameof(Controllers.ADMController)
                        .IndexOf("Controller")) == jabatan;
                }

                return false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (this.CekSession())
                {
                    if (Int32.TryParse(Request.QueryString["Id_Pasien"], out int id))
                    {
                        var viewReport = this.ReportViewer1.LocalReport;
                        var data = new List<TB_PASIEN> { new DbEntities().TB_PASIEN.Find(id) };

                        viewReport.ReportPath = Server.MapPath("~/Report/RDLC/KartuPasien.rdlc");
                        viewReport.DataSources.Clear();
                        viewReport.DataSources.Add(new ReportDataSource("DataSet1", data));
                    }
                    else
                    {
                        FlashMessage.SetFlashMessage(
                            "Pilih Data Pasien Dengan Benar.",
                            FlashMessage.FlashMessageType.Warning);
                        Response.Redirect("~/ADM/TanganiPasien?Tangani=DataPasien");
                    }
                }
                else
                {
                    FlashMessage.SetFlashMessage(
                        "Hanya Bisa Diakses Oleh Admin Pelayanan.",
                        FlashMessage.FlashMessageType.Warning);
                    Response.Redirect("~/");
                }
            }
        }
    }
}