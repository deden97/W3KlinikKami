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

        public ActionResult DaftarAkunBaru()
        {
            return View();
        }
    }
}