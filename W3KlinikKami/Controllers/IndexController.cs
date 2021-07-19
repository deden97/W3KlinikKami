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
                // verifikasi 'Username' & 'Password'
                if (this.db.TB_AKUN.Any(l => l.USERNAME == login.USERNAME && l.PASSWORD_AKUN == login.PASSWORD_AKUN))
                {
                    Session["ID"] = this.db.TB_AKUN.Single(l => l.USERNAME == login.USERNAME && l.PASSWORD_AKUN == login.PASSWORD_AKUN).ID;
                    Session["JABATAN"] = this.db.TB_USER.Find(Session["ID"]).JABATAN;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    string msg = "tes";
                }
            }
            return View();
        }

        [HttpGet]
        public JsonResult GetJabatan()
        {
            try
            {
                this.db.Configuration.ProxyCreationEnabled = false;
                return Json(this.db.TB_JABATAN.ToList(), JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.db.Configuration.ProxyCreationEnabled = true;
            }
        }

        [HttpPost]
        public ActionResult DaftarAkunBaru(TB_USER dt)
        {
            dt.FOTO = "~/image/profile_user.png";
            this.db.TB_USER.Add(dt);
            this.db.SaveChanges();
            return RedirectToAction("Login");
        }
    }
}