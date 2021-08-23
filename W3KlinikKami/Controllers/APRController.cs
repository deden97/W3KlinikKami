using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using W3KlinikKami.Core;

namespace W3KlinikKami.Controllers
{
    public class APRController : Controller
    {
        private readonly DbEntities db = new DbEntities();

        private int id { get; set; }

        private string jabatan { get; set; }

        public enum menu { RacikObat, RiwayatPasien }

        private bool CekSession()
        {
            this.id = csmSession.GetIdSession();
            this.jabatan = csmSession.GetJabatanSession();
            if (this.db.TB_USER.Any(d => d.ID == this.id && d.JABATAN == this.jabatan))
            {
                if(nameof(APRController) == this.jabatan + "Controller")
                {
                    ViewBag.DT_USER = this.db.TB_USER.Find(this.id);
                    return true;
                }
                else
                {
                    FlashMessage.SetFlashMessage(
                        "Url Yang Anda Tuju Hanya Dapat Diakses Oleh 'Apoteker'",
                        FlashMessage.FlashMessageType.Warning);
                }
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
            }

            return false;
        }

        public ActionResult Index()
        {
            if (this.CekSession())
                return RedirectToAction("RiwayatPasien");
            else
                return RedirectToAction("Index", "Index");
        }

        public ActionResult RacikObat()
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                ViewData["Data"] = this.db
                    .TB_KUNJUNGAN_PASIEN
                    .AsEnumerable()
                    .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                        d.PENANGANAN_DOKTER == true &&
                        d.PENGAMBILAN_OBAT == null);
            }
            catch(Exception e)
            {
                e.ToString();
            }

            ViewBag.TitlePage = "Racik Obat";
            ViewBag.Menu = menu.RacikObat.ToString();
            return View("Index");
        }

        public ActionResult RiwayatPasien(string search, int? page, int? pageSize)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                RiwayatPasien riwayatPasien = new RiwayatPasien();
                ViewData["Data"] = riwayatPasien.GetData(search, page ?? 1, pageSize ?? 8);
            }
            catch(Exception e)
            {
                e.ToString();
            }

            ViewBag.TitlePage = "Riwayat Pasien";
            ViewBag.Menu = menu.RiwayatPasien.ToString();
            return View("Index");
        }
    }
}