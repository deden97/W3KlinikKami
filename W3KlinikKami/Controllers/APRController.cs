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
                return RedirectToAction("RacikObat");
            else
                return RedirectToAction("Index", "Index");
        }

        public JsonResult UpdateData()
        {
            var dt = this.db
                    .TB_KUNJUNGAN_PASIEN
                    .AsEnumerable()
                    .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                        d.PENANGANAN_DOKTER == true &&
                        d.PENGAMBILAN_OBAT == null);
            return Json(dt.Select(d => new
            {
                d.ID,
                d.ID_PASIEN,
                d.TB_PASIEN.NAMA
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RacikObat(FormCollection formData)
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

                if(formData.Count == 1)
                    ViewBag.IdPasienTerpilih = Convert.ToInt32(formData["ID_KUNJUNGAN_PASIEN"]);
            }
            catch(Exception e)
            {
                e.ToString();
            }

            ViewBag.TitlePage = "Racik Obat";
            ViewBag.Menu = menu.RacikObat.ToString();
            return View("Index");
        }

        public ActionResult SimpanObat(FormCollection formData)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            if(formData["ID_KUNJUNGAN_PASIEN_S"] != null)
            {
                var ID_KUNJUNGAN_PASIEN = Convert.ToInt32(formData["ID_KUNJUNGAN_PASIEN_S"]);

                //1. tambah ke antrian
                var noAntrian = this.db
                    .TB_ANTRIAN_PENGAMBILAN_OBAT
                    .AsEnumerable()
                    .Where(d => d.TB_KUNJUNGAN_PASIEN.TANGGAL_KUNJUNGAN.Date == DateTime.Today)
                    .Count() + 1;

                var dt = new TB_ANTRIAN_PENGAMBILAN_OBAT
                {
                    ID_KUNJUNGAN_PASIEN = ID_KUNJUNGAN_PASIEN,
                    NO_ANTRIAN = noAntrian
                };
                this.db.TB_ANTRIAN_PENGAMBILAN_OBAT.Add(dt);

                //2. pada tb kunjungan pengambilan obat jadi false
                var kunj = this.db
                    .TB_KUNJUNGAN_PASIEN
                    .Find(ID_KUNJUNGAN_PASIEN);
                kunj.PENGAMBILAN_OBAT = false;
                this.db.Entry(kunj).State = System.Data.Entity.EntityState.Modified;

                this.db.SaveChanges();
            }

            return RedirectToAction("RacikObat");
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