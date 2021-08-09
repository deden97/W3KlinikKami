using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using W3KlinikKami.Core;

namespace W3KlinikKami.Controllers
{
    public class DKRController : Controller
    {
        private readonly DbEntities db = new DbEntities();
        private int ID { get; set; }
        private string JABATAN { get; set; }
        private enum menu { TanganiPasien }
        private bool CekSession()
        {
            this.ID = Convert.ToInt32(Session["ID"]);
            this.JABATAN = Convert.ToString(Session["JABATAN"]);
            if(this.db.TB_USER.Any(d => d.ID == this.ID && d.JABATAN == this.JABATAN))
            {
                if(nameof(DKRController).Remove(nameof(DKRController).IndexOf("Controller")) == this.JABATAN)
                {
                    ViewBag.DT_USER = this.db.TB_USER.Find(this.ID);
                    return true;
                }
                else
                {
                    FlashMessage.SetFlashMessage("Link Yang Anda Tuju Hanya Dapat Diakses Oleh 'Dokter'.", FlashMessage.FlashMessageType.Warning);
                    return false;
                }
            }
            else
            {
                FlashMessage.SetFlashMessage("Anda Harus Login Terlebih Dahulu.", FlashMessage.FlashMessageType.Warning);
                return false;
            }
        }

        public ActionResult Index()
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            return RedirectToAction("TanganiPasien");
        }

        public ActionResult TanganiPasien()
        {
            if (this.CekSession())
            {
                ViewBag.Menu = menu.TanganiPasien.ToString();
                ViewData["Data"] = this.db
                    .TB_KUNJUNGAN_PASIEN
                    .AsEnumerable()
                    .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today)
                    .OrderBy(d => d.TANGGAL_KUNJUNGAN);
                return View("Index");
            }
            else
            {
                return RedirectToAction("Index", "Index");
            }
        }
    }
}