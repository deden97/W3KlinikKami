using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using System.Data.Entity;
using W3KlinikKami.Messege;
using PagedList;
using PagedList.Mvc;

namespace W3KlinikKami.Controllers
{
    public class ADMController : Controller
    {
        private readonly DbEntities db = new DbEntities();
        private int id { get; set; }
        private string jabatan { get; set; }
        private enum PenangananPasien { DaftarPasienBaru, BerobatPasien, PengambilanObat, DataPasien }

        private bool CekSession()
        {
            this.id = Convert.ToInt32(Session["ID"]);
            this.jabatan = Convert.ToString(Session["JABATAN"]);
            if (this.db.TB_USER.Any(j => j.ID == this.id && j.JABATAN == this.jabatan))
            {
                return nameof(ADMController)
                    .Remove(nameof(ADMController)
                    .IndexOf("Controller")) == this.jabatan;
            }

            return false;
        }

        public ActionResult Index()
        {
            if (this.CekSession())
            {
                return RedirectToAction("DataPasien");
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }
        }

        public ActionResult DaftarPasienBaru()
        {
            if (this.CekSession())
            {
                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.DaftarPasienBaru;

                // data user yg digunakan -> 'Nama', 'Jabatan', 'Foto'
                ViewBag.DT_USER = this.db.TB_USER.Find(this.id);
                return View("Index");
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }
        }

        [HttpPost]
        public ActionResult DaftarPasienBaru([Bind(Exclude = "ID, TERDAFTAR")] TB_PASIEN dt)
        {
            if (!this.CekSession())
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dt.TERDAFTAR = DateTime.Now;
                    this.db.Entry(dt).State = EntityState.Added;
                    this.db.SaveChanges();

                    FlashMessage.SetFlashMessage("Data Pasien Berhasil Disimpan.", FlashMessage.FlashMessageType.Success);
                }
                catch(Exception e)
                {
                    e.ToString();
                }
            }

            // ModePenanganan untuk menentukan 'menu'
            ViewBag.ModePenanganan = PenangananPasien.DaftarPasienBaru.ToString();

            // data user yg digunakan -> 'Nama', 'Jabatan', 'Foto'
            ViewBag.DT_USER = this.db.TB_USER.Find(this.id);
            return View("Index");
        }

        public ActionResult BerobatPasien()
        {
            if (this.CekSession())
            {
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN.ToList();

                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.BerobatPasien;

                // data user yg digunakan -> 'Nama', 'Jabatan', 'Foto'
                ViewBag.DT_USER = this.db.TB_USER.Find(this.id);
                return View("Index");
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }
        }

        public ActionResult PengambilanObat()
        {
            if (this.CekSession())
            {
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN.ToList();

                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.PengambilanObat;

                // data user yg digunakan -> 'Nama', 'Jabatan', 'Foto'
                ViewBag.DT_USER = this.db.TB_USER.Find(this.id);
                return View("Index");
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }
        }

        public ActionResult DataPasien(int? page)
        {
            if (this.CekSession())
            {
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN
                    .OrderByDescending(d => d.TERDAFTAR)
                    .ToPagedList(page ?? 1, 3);

                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.DataPasien;

                // data user yg digunakan -> 'Nama', 'Jabatan', 'Foto'
                ViewBag.DT_USER = this.db.TB_USER.Find(this.id);
                return View("Index");
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }
        }

        [HttpPost]
        public ActionResult DataPasien_Edit([Bind(Exclude = "TERDAFTAR")] TB_PASIEN dt, int? page)
        {
            if (!this.CekSession())
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }

            if (ModelState.IsValid)
            {
                var tes = "";
            }
            else
            {
                TempData["ShowIdEdit"] = dt.ID;
            }

            return RedirectToAction("DataPasien", new { @page = page });
        }

        [HttpPost]
        public ActionResult DataPasien_Delete([Bind(Include = "ID")] TB_PASIEN dt)
        {
            this.db.Entry(dt).State = EntityState.Deleted;
            this.db.SaveChanges();
            FlashMessage.SetFlashMessage("Data Telah Dihapus.", FlashMessage.FlashMessageType.Success);
            return RedirectToAction("DataPasien");
        }
    }
}