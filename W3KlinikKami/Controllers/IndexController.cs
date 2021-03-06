using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using System.Text;
using System.Data.Entity;
using W3KlinikKami.Core;

namespace W3KlinikKami.Controllers
{
    public class IndexController : Controller
    {
        private readonly DbEntities db = new DbEntities();

        // cek session -> (ID, JABATAN(kode jabatan))
        private bool CekSession()
        {
            int id = csmSession.GetIdSession();
            string jabatan = csmSession.GetJabatanSession();
            if (this.db.TB_USER.Any(j => j.ID == id && j.JABATAN == jabatan))
            {
                // data user yg digunakan -> 'Nama', 'Jabatan', 'Foto'
                ViewBag.DT_USER = this.db.TB_USER.Single(d => d.ID == id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActionResult Index() => RedirectToAction("Login");

        /* Begin: Login -------------------------------------------------------------------- */
        // Login Get
        public ActionResult Login()
        {
            if (this.CekSession())
                return RedirectToAction("Index", csmSession.GetJabatanSession());
            else
                return View();
        }

        // Login Post -> verifikasi username dan password
        [HttpPost]
        public ActionResult Login([Bind(Exclude = "ID, PASSWORD_BARU")] TB_AKUN login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // jika username terdaftar pada Db
                    if (this.db.TB_AKUN.Any(l => l.USERNAME == login.USERNAME))
                    {
                        // jika password sesuai dengan username
                        if (this.db.TB_AKUN.Any(l => l.USERNAME == login.USERNAME && l.PASSWORD_AKUN == login.PASSWORD_AKUN))
                        {
                            var dt = this.db.TB_AKUN.Single(l => l.USERNAME == login.USERNAME && l.PASSWORD_AKUN == login.PASSWORD_AKUN);
                            csmSession.Set(dt.ID, dt.TB_USER.JABATAN);
                            return RedirectToAction("Index", csmSession.GetJabatanSession());
                        }
                        else // jika password tidak sesuai dengan username
                        {
                            ModelState.AddModelError("PASSWORD_AKUN", "Password Tidak Sesuai");
                        }
                    }
                    else // jika username tidak terdaftar pada Db
                    {
                        ModelState.AddModelError("USERNAME", "Username Tidak Terdaftar");
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }
            return View("Login");
        }
        /* End: Login -------------------------------------------------------------------- */

        /* Begin: DaftarAkunBaru -------------------------------------------------------------------- */
        // DaftarAkunBaru Get
        public ActionResult DaftarAkunBaru()
        {
            if (this.CekSession())
            {
                return RedirectToAction("Index", "Index");
            }
            else
            {
                ViewData["JABATAN"] = new SelectList(this.db.TB_JABATAN, "KODE_JABATAN", "JABATAN");
                return View();
            }
        }

        // DaftarAkunBaru Post menerima dan menyimpan data baru.
        [HttpPost]
        public ActionResult DaftarAkunBaru([Bind(Exclude = "ID, FOTO")] TB_USER dt)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(!this.db.TB_AKUN.Any(d => d.USERNAME == dt.TB_AKUN.USERNAME)) // jika username belum terdaftar
                    {
                        dt.FOTO = "~/image/profile_user.png";
                        dt.TERDAFTAR = DateTime.Now;
                        this.db.TB_USER.Add(dt);
                        this.db.SaveChanges();
                        FlashMessage.SetFlashMessage(
                            "Daftar Akun Berhasil, Silahkan Untuk Login",
                            FlashMessage.FlashMessageType.Success);
                        return RedirectToAction("Login");
                    }
                    else // jika username sudah terdaftar
                    {
                        ModelState.AddModelError("TB_AKUN.USERNAME", "Username Sudah Digunakan, Gunakan Username Lain.");
                    }
                }
                catch(Exception e)
                {
                    e.ToString();
                }
            }

            ViewData["JABATAN"] = new SelectList(this.db.TB_JABATAN, "KODE_JABATAN", "JABATAN");
            return View();
        }
        /* End: DaftarAkunBaru -------------------------------------------------------------------- */

        /* Begin: EditData -------------------------------------------------------------------- */
        // EditData get
        public ActionResult EditData()
        {
            if (this.CekSession())
            {
                ViewBag.EditMode = "Edit Data";
                ViewBag.JABATAN = new SelectList(this.db.TB_JABATAN, "KODE_JABATAN", "JABATAN", csmSession.GetJabatanSession());
                return View("EditData", this.db.TB_USER.Find(csmSession.GetIdSession()));
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Login");
            }
        }

        // EditData post -> menerima data user yg telah diedit dan menyimpan perubahan data.
        [HttpPost]
        public ActionResult EditData([Bind(Include = "NAMA, NO_HP, ALAMAT, FOTO, FOTO_TERPILIH")] TB_USER dt)
        {
            // cek session -> ID, jika belum login diarahkan ke form login
            if (!this.CekSession())
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Login");
            }

            try
            {
                TB_USER updateDt = new TB_USER();
                updateDt = this.db.TB_USER.Find(csmSession.GetIdSession());
                updateDt.NAMA = dt.NAMA;
                updateDt.NO_HP = dt.NO_HP;
                updateDt.ALAMAT = dt.ALAMAT;

                if (dt.FOTO_TERPILIH != null)
                {
                    StringBuilder namaFoto = new StringBuilder();

                    // tambah tanggal dan waktu hari ini
                    namaFoto.Append($"{DateTime.Now.ToString("dd-MM-yyyy H-m-s")}_");

                    // tambah nama file dengan ekstensi contoh: '<n_img>.jpg'
                    namaFoto.Append(Path.GetFileName(dt.FOTO_TERPILIH.FileName));

                    // simpan direktori foto ke 'dt.FOTO' untuk di simpan di Db
                    updateDt.FOTO = $"~/Image/{namaFoto.ToString()}";

                    // masukan path direktori secara lengkap contoh: D:/Folder/img.jpg
                    namaFoto.Replace(namaFoto.ToString(), Path.Combine(Server.MapPath("~/Image/"), namaFoto.ToString()));

                    // simpan foto di direktori lokal atau pada web server 'W3KlinikKami/Image'
                    dt.FOTO_TERPILIH.SaveAs(namaFoto.ToString());
                }

                this.db.Entry(updateDt).State = EntityState.Modified;
                this.db.SaveChanges();

                FlashMessage.SetFlashMessage("Perubahan Data Telah Disimpan", FlashMessage.FlashMessageType.Success);
            }
            catch (Exception e)
            {
                e.ToString();
            }

            ViewBag.EditMode = "Edit Data";
            return View(this.db.TB_USER.Find(csmSession.GetIdSession()));
        }
        /* End: EditData -------------------------------------------------------------------- */

        /* Begin: EditUsername -------------------------------------------------------------------- */
        // EditUsername get
        public ActionResult EditUsername()
        {
            if (this.CekSession())
            {
                ViewBag.EditMode = "Edit Username";
                return View("EditData");
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Login");
            }
        }

        // EditUsername post -> menerima username baru dan menyimpan perubahan username.
        [HttpPost]
        public ActionResult EditUsername([Bind(Exclude = "ID, TB_USER")] TB_AKUN dt)
        {
            // cek session -> ID, jika belum login diarahkan ke form login
            if (!this.CekSession())
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    dt.ID = csmSession.GetIdSession();
                    if(this.db.TB_AKUN.Any(i => i.ID == dt.ID && i.PASSWORD_AKUN == dt.PASSWORD_AKUN))
                    {
                        this.db.Entry(dt).State = EntityState.Modified;
                        this.db.SaveChanges();

                        FlashMessage.SetFlashMessage("Username Berhasil Di-Edit", FlashMessage.FlashMessageType.Success);
                    }
                    else
                    {
                        ModelState.AddModelError("PASSWORD_AKUN", "Password Salah, Pastikan Password Bener.");
                    }
                }
                catch(Exception e)
                {
                    e.ToString();
                }
            }

            ViewBag.EditMode = "Edit Username";
            return View("EditData");
        }
        /* End: EditUsername -------------------------------------------------------------------- */

        /* Begin: EditPassword -------------------------------------------------------------------- */
        // EditPassword get
        public ActionResult EditPassword()
        {
            if (this.CekSession())
            {
                ViewBag.EditMode = "Edit Password";
                return View("EditData");
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Login");
            }
        }

        // EditPassword post -> menerima password baru dan menyimpan perubahan password.
        [HttpPost]
        public ActionResult EditPassword([Bind(Exclude = "ID, USERNAME, TB_USER")] TB_AKUN dt)
        {
            // cek session -> ID, jika belum login diarahkan ke form login
            if(!this.CekSession())
            {
                FlashMessage.TemFlashMessageLogin();
                return RedirectToAction("Login");
            }

            try
            {
                if (!String.IsNullOrEmpty(dt.PASSWORD_BARU))
                {
                    dt.ID = csmSession.GetIdSession();
                    if (this.db.TB_AKUN.Any(d => d.ID == dt.ID && d.PASSWORD_AKUN == dt.PASSWORD_AKUN))
                    {
                        TB_AKUN updateDt = this.db.TB_AKUN.Find(dt.ID);
                        updateDt.PASSWORD_AKUN = dt.PASSWORD_BARU;
                        this.db.Entry(updateDt).State = EntityState.Modified;
                        this.db.SaveChanges();

                        FlashMessage.SetFlashMessage("Password Berhasil Di-Edit", FlashMessage.FlashMessageType.Success);
                    }
                    else
                    {
                        ModelState.AddModelError("PASSWORD_AKUN", "Password Salah, Pastikan Password Bener.");
                    }
                }
                else
                {
                    ModelState.AddModelError("PASSWORD_BARU", "Password Baru Tidak Boleh Kosong.");
                }
            }
            catch(Exception e)
            {
                e.ToString();
            }

            return View("EditData");
        }
        /* End: EditPassword -------------------------------------------------------------------- */

        /* Begin: Logout -------------------------------------------------------------------- */
        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Login");
        }
        /* End: Logout -------------------------------------------------------------------- */
    }
}