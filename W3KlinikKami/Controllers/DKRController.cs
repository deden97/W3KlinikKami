using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using W3KlinikKami.Core;
using System.Data.Entity;

namespace W3KlinikKami.Controllers
{
    public class DKRController : Controller
    {
        private readonly DbEntities db = new DbEntities();
        private int ID { get; set; }
        private string JABATAN { get; set; }
        public enum menu { TanganiPasien }
        private bool CekSession()
        {
            this.ID = csmSession.GetIdSession();
            this.JABATAN = csmSession.GetJabatanSession();
            if(this.db.TB_USER.Any(d => d.ID == this.ID && d.JABATAN == this.JABATAN))
            {
                if(nameof(DKRController) == this.JABATAN + "Controller")
                {
                    ViewBag.DT_USER = this.db.TB_USER.Find(this.ID);
                    return true;
                }
                else
                {
                    FlashMessage.SetFlashMessage("Link Yang Anda Tuju Hanya Dapat Diakses Oleh 'Dokter'.", FlashMessage.FlashMessageType.Warning);
                    return false;
                }
            }
            else
            {
                FlashMessage.SetFlashMessage("Anda Harus Login Terlebih Dahulu.", FlashMessage.FlashMessageType.Warning);
                return false;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");
            else
                return RedirectToAction("TanganiPasien");
        }

        public ActionResult TanganiPasien(int? idPasien)
        {
            if (this.CekSession())
            {
                ViewBag.Menu = menu.TanganiPasien.ToString();
                ViewData["Data"] = this.db
                    .TB_KUNJUNGAN_PASIEN
                    .AsEnumerable()
                    .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                        d.PENANGANAN_DOKTER == null &&
                        (d.PENGAMBILAN_OBAT == null || d.PENGAMBILAN_OBAT == false))
                    .OrderBy(d => d.TANGGAL_KUNJUNGAN);
                if (idPasien > 0)
                {
                    ViewBag.idTerpilih = idPasien;
                }
                return View("Index");
            }
            else
            {
                return RedirectToAction("Index", "Index");
            }
        }
        // [Bind(Include = "TANGGAL_KUNJUNGAN, KELUHAN, PEMERIKSAAN, DIAGNOSA, RESEP_OBAT, KETERANGAN")]
        [HttpPost]
        public ActionResult TanganiPasien()
        {
            TB_DATA_PENANGANAN_PASIEN dt = new TB_DATA_PENANGANAN_PASIEN();
            UpdateModel(dt, null, null, new string[] { "ID, TB_KUNJUNGAN_PASIEN" });
            dt.ID_DOKTER = csmSession.GetIdSession();
            this.db.Entry(dt).State = EntityState.Added;
            this.db.SaveChanges();

            TB_KUNJUNGAN_PASIEN tB_KUNJUNGAN_PASIEN = this.db.TB_KUNJUNGAN_PASIEN.Find(dt.ID_KUNJUNGAN_PASIEN);
            tB_KUNJUNGAN_PASIEN.ID = (int)dt.ID_KUNJUNGAN_PASIEN;
            tB_KUNJUNGAN_PASIEN.PENANGANAN_DOKTER = true;
            this.db.Entry(tB_KUNJUNGAN_PASIEN).State = EntityState.Modified;
            this.db.SaveChanges();

            return RedirectToAction("TanganiPasien");
        }
    }
}