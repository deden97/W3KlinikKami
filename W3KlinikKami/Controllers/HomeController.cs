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
        private int id { get; set; }
        private string jabatan { get; set; }

        private bool CekSession()
        {
            this.id = Convert.ToInt32(Session["ID"]);
            this.jabatan = Convert.ToString(Session["JABATAN"]);
            return this.db.TB_USER.Any(j => j.ID == this.id && j.JABATAN == this.jabatan);
        }

        public ActionResult Index()
        {
            if (this.CekSession())
                return RedirectToAction("Index", this.jabatan);
            else
                return RedirectToAction("Login", "Index");
        }
    }
}