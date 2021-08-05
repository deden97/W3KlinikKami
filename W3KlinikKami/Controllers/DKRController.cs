using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace W3KlinikKami.Controllers
{
    public class DKRController : Controller
    {
        // GET: DKR
        public ActionResult Index()
        {
            ViewBag.DT_USER = new Models.DbEntities().TB_USER.Single(d => d.JABATAN == "DKR");
            return View();
        }
    }
}