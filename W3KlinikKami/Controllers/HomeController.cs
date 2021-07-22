using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;

namespace W3KlinikKami.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbEntities db = new DbEntities();

        private bool CekSession()
        {
            int id = Convert.ToInt32(Session["ID"]);
            string jabatan = Convert.ToString(Session["JABATAN"]);
            return this.db.TB_USER.Any(j => j.ID == id && j.JABATAN == jabatan);
        }

        // GET: Home
        public ActionResult Index()
        {
            //return View();
            if (this.CekSession())
            {
                int id = Convert.ToInt32(Session["ID"]);
                TB_USER dtUser = this.db.TB_USER.Find(id);
                return View(dtUser);
            }
            else return RedirectToAction("Login", "Index");
        }
    }
}