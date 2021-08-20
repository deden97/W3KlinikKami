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

            var data = this.riwayatPasien.GetDetailPenangananById(idKunjungan);
            return Json(data.Select(d => new
            {
                @ID_PASIEN = d.TB_KUNJUNGAN_PASIEN.TB_PASIEN.ID,
                @NAMA_PASIEN = d.TB_KUNJUNGAN_PASIEN.TB_PASIEN.NAMA,
                @ID_DOKTER = d.TB_USER.ID,
                @NAMA_DOKTER = d.TB_USER.NAMA,
                d.KELUHAN,
                d.PEMERIKSAAN,
                d.DIAGNOSA,
                d.RESEP_OBAT,
                d.KETERANGAN
            }), JsonRequestBehavior.AllowGet);
        }
    }
}