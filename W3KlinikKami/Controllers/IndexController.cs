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

        private bool CekSession()
        {
            int id = Convert.ToInt32(Session["ID"]);
            string jabatan = Convert.ToString(Session["JABATAN"]);
            return this.db.TB_USER.Any(j => j.ID == id && j.JABATAN == jabatan);
        }

        public ActionResult Login()
        {
            if (this.CekSession()) return RedirectToAction("Index", "Home");
            else return View();
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
            if (this.CekSession())
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["JABATAN"] = new SelectList(this.db.TB_JABATAN, "KODE_JABATAN", "JABATAN");
                return View();
            }
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

        public ActionResult EditData()
        {
            if (this.CekSession())
            {
                ViewBag.EditMode = "Edit Data";
                int id = Convert.ToInt32(Session["ID"]);
                return View("EditData", this.db.TB_USER.Find(id));
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //[HttpPost]
        //public ActionResult EditData(TB_USER dt)
        //{
        //    return View();
        //}

        public ActionResult EditUsername()
        {
            if (this.CekSession())
            {
                ViewBag.EditMode = "Edit Username";
                return View("EditData");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //[HttpPost]
        //public ActionResult EditUsername(TB_AKUN dt)
        //{
        //    return View();
        //}

        public ActionResult EditPassword()
        {
            if (this.CekSession())
            {
                ViewBag.EditMode = "Edit Password";
                return View("EditData");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //[HttpPost]
        //public ActionResult EditPassword(TB_AKUN dt)
        //{
        //    return View();
        //}

        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Login");
        }
    }
}