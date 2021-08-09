using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using W3KlinikKami.Models;
using W3KlinikKami.Core;

namespace W3KlinikKami.Report
{
    public partial class DataPasien : System.Web.UI.Page
    {
        private bool CekSession()
        {
            int id = Convert.ToInt32(Session["ID"]);
            string jabatan = Convert.ToString(Session["JABATAN"]);
            using(Models.DbEntities db = new Models.DbEntities())
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
                    if(Int32.TryParse(Request.QueryString["Id_Pasien"], out int id))
                    {
                        var dt = new List<TB_PASIEN> { new DbEntities().TB_PASIEN.Find(id) };
                        this.ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Report/RDLC/KartuPasien.rdlc");
                        this.ReportViewer1.LocalReport.DataSources.Clear();
                        this.ReportViewer1.LocalReport.DataSources
                            .Add(new Microsoft.Reporting.WebForms.ReportDataSource("DataSet1", dt));
                    }
                    else
                    {
                        FlashMessage.SetFlashMessage("Pilih Data Pasien Dengan Benar.", FlashMessage.FlashMessageType.Warning);
                        Response.Redirect("~/ADM/TanganiPasien?Tangani=DataPasien");
                    }
                }
                else
                {
                    FlashMessage.SetFlashMessage("Hanya Bisa Diakses Oleh Admin Pelayanan.", FlashMessage.FlashMessageType.Warning);
                    Response.Redirect("~/");
                }
            }
        }
    }
}