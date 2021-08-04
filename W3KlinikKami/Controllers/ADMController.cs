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

        /* Begin: DataPasien -------------------------------------------------------------------- */
        public ActionResult DataPasien(int? page, string search, int? pageSize)
        {
            if (this.CekSession())
            {
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN
                    .Where(d => d.NAMA.Contains(search) || search == null)
                    .OrderByDescending(d => d.TERDAFTAR)
                    .ToPagedList(page ?? 1, pageSize ?? 5);

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
        public ActionResult DataPasien_Edit([Bind(Exclude = "TERDAFTAR")] TB_PASIEN dt, int? page, string search, int? pageSize)
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
                    TB_PASIEN dtUpdate = this.db.TB_PASIEN.Find(dt.ID);
                    dtUpdate.NAMA = dt.NAMA;
                    dtUpdate.JENIS_KELAMIN = dt.JENIS_KELAMIN;
                    dtUpdate.TANGGAL_LAHIR = dt.TANGGAL_LAHIR;
                    dtUpdate.GOLONGAN_DARAH = dt.GOLONGAN_DARAH;
                    dtUpdate.NO_HP = dt.NO_HP;
                    dtUpdate.ALAMAT = dt.ALAMAT;
                    dtUpdate.TERDAFTAR = this.db.TB_PASIEN.Find(dt.ID).TERDAFTAR;

                    this.db.Entry(dtUpdate).State = EntityState.Modified;
                    this.db.SaveChanges();
                    FlashMessage.SetFlashMessage(
                        $"Data Pasien Atas Nama: '{dt.NAMA}' Dengan ID: '{dt.ID}' Telah Diubah.",
                        FlashMessage.FlashMessageType.Success);
                }
                catch(Exception e)
                {
                    e.ToString();
                }
            }

            return RedirectToAction("DataPasien", new { @page = page, @search = search, @pageSize = pageSize });
        }

        [HttpPost]
        public ActionResult DataPasien_Delete([Bind(Include = "ID")] TB_PASIEN dt)
        {
            if (!this.CekSession())
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Index", "Index");
            }

            try
            {
                this.db.Entry(dt).State = EntityState.Deleted;
                this.db.SaveChanges();
                FlashMessage.SetFlashMessage("Data Telah Dihapus.", FlashMessage.FlashMessageType.Success);
            }
            catch(Exception e)
            {
                e.ToString();
            }

            return RedirectToAction("DataPasien");
        }
        /* End: DataPasien -------------------------------------------------------------------- */

        /* Begin: BerobatPasien -------------------------------------------------------------------- */
        [HttpGet]
        public ActionResult BerobatPasien(int? page, string search)
        {
            if (this.CekSession())
            {
                // jika Id pasien di pilih untuk daftar berobat
                if(int.TryParse(Request.QueryString["pilih_id"], out int id))
                {
                    TempData["ID_PASIEN_TERPILIH"] = this.db.TB_PASIEN.Find(id);
                }

                // get semua data pasien
                int.TryParse(search, out int searchId);
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN
                    .Where(d => d.ID == searchId || d.NAMA.Contains(search) || search == null)
                    .OrderBy(d => d.NAMA)
                    .ToPagedList(page ?? 1, 8);

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
        /* End: BerobatPasien -------------------------------------------------------------------- */

        /* Begin: PengambilanObat -------------------------------------------------------------------- */
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
        /* End: PengambilanObat -------------------------------------------------------------------- */

        /* Begin: DaftarPasienBaru -------------------------------------------------------------------- */
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
                catch (Exception e)
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
        /* End: DaftarPasienBaru -------------------------------------------------------------------- */
    }
}