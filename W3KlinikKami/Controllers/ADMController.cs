using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;

namespace W3KlinikKami.Controllers
{
    public class ADMController : Controller
    {
        private readonly DbEntities db = new DbEntities();
        private int id { get; set; }
        private string jabatan { get; set; }
        private enum PenangananPasien { DaftarPasienBaru, BerobatPasien, PengambilanObat }

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
                ViewBag.ModePenanganan = PenangananPasien.DaftarPasienBaru.ToString();
                return View(this.db.TB_USER.Find(this.id));
            }
            else
            {
                return RedirectToAction("Login", "Index");
            }
        }

        public ActionResult TanganiPasien()
        {
            if (this.CekSession())
            {
                string tanganiPasien = Request.QueryString["tangani"];
                if (tanganiPasien != PenangananPasien.DaftarPasienBaru.ToString() &&
                    tanganiPasien != PenangananPasien.BerobatPasien.ToString() &&
                    tanganiPasien != PenangananPasien.PengambilanObat.ToString())
                {
                    return new HttpNotFoundResult();
                }
                ViewBag.ModePenanganan = tanganiPasien;
                return View("Index", this.db.TB_USER.Find(this.id));
            }
            else
            {
                return RedirectToAction("Index", "Index");
            }
        }
    }
}