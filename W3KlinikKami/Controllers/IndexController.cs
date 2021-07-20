using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;

namespace W3KlinikKami.Controllers
{
    public class IndexController : Controller
    {
        private readonly DbEntities db = new DbEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(TB_AKUN login)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (this.db.TB_AKUN.Any(l => l.USERNAME == login.USERNAME)) // jika username terdaftar pada Db
                    {
                        if (this.db.TB_AKUN.Any(l => l.USERNAME == login.USERNAME && l.PASSWORD_AKUN == login.PASSWORD_AKUN)) // jika password sesuai dengan username
                        {
                            Session["ID"] = this.db.TB_AKUN.Single(l => l.USERNAME == login.USERNAME && l.PASSWORD_AKUN == login.PASSWORD_AKUN).ID;
                            Session["JABATAN"] = this.db.TB_USER.Find(Session["ID"]).JABATAN;
                            return RedirectToAction("Index", "Home");
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
            return View();
        }

        public ActionResult DaftarAkunBaru()
        {
            ViewData["JABATAN"] = new SelectList(this.db.TB_JABATAN, "KODE_JABATAN", "JABATAN");
            return View();
        }

        [HttpPost]
        public ActionResult DaftarAkunBaru(TB_USER dt)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(!this.db.TB_AKUN.Any(d => d.USERNAME == dt.TB_AKUN.USERNAME)) // jika username belum terdaftar
                    {
                        dt.FOTO = "~/image/profile_user.png";
                        this.db.TB_USER.Add(dt);
                        this.db.SaveChanges();
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
    }
}