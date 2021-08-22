using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using W3KlinikKami.Core;

namespace W3KlinikKami.Controllers
{
    public class RiwayatPasienController : Controller
    {
        private readonly RiwayatPasien riwayatPasien = new RiwayatPasien();

        private bool CekSession()
        {
            int id = csmSession.GetIdSession();
            string jabatan = csmSession.GetJabatanSession();
            return new DbEntities().TB_USER.Any(d => d.ID == id && d.JABATAN == jabatan);
        }

        [HttpGet]
        public JsonResult DetailPenanganan(int idKunjungan)
        {
            if (!this.CekSession())
                return Json(new { @dataPasien = "null" }, JsonRequestBehavior.AllowGet);

            object data = this.riwayatPasien.GetDetailPenangananById(idKunjungan);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}