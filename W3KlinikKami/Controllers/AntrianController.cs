using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;

namespace W3KlinikKami.Controllers
{
    public class AntrianController : Controller
    {
        private readonly DbEntities db = new DbEntities();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AntrianBerobat()
        {
            
            return View();
        }

        [HttpGet]
        //parameter jika null maka data yg diambil pasien yg belum dipanggil/diperiksa dokter,
        // jika false pasien yg sudah dipanggil/sedang diperiksa oleh dokter,
        // jika true pasien yg sudah selesai diperiksa oleh dokter.
        public JsonResult UpdateDataAntrianBerobat(bool? statusPanggilan)
        {
            var dataAntrian = this.db
                .TB_ANTRIAN_BEROBAT
                .AsEnumerable()
                .Where(d => d.TB_KUNJUNGAN_PASIEN.TANGGAL_KUNJUNGAN.Date == DateTime.Today && d.STATUS_PANGGILAN == statusPanggilan)
                .OrderBy(d => d.NO_ANTRIAN)
                .ToList();

            return Json(dataAntrian.Select(d => new
            {
                d.TB_KUNJUNGAN_PASIEN.ID_PASIEN,
                d.TB_KUNJUNGAN_PASIEN.TB_PASIEN.NAMA,
                d.NO_ANTRIAN
            }),
            JsonRequestBehavior.AllowGet);
        }
    }
}