using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using W3KlinikKami.Core;
using System.Data.Entity;

namespace W3KlinikKami.Controllers
{
    public class DKRController : Controller
    {
        private readonly DbEntities db = new DbEntities();
        private int ID { get; set; }
        private string JABATAN { get; set; }
        public enum menu { TanganiPasien, RiwayatPasien }

        #region Cek Session
        private bool CekSession()
        {
            this.ID = csmSession.GetIdSession();
            this.JABATAN = csmSession.GetJabatanSession();
            if (this.db.TB_USER.Any(d => d.ID == this.ID && d.JABATAN == this.JABATAN))
            {
                if(nameof(DKRController) == this.JABATAN + "Controller")
                {
                    ViewBag.DT_USER = this.db.TB_USER.Find(this.ID);
                    return true;
                }
                else
                {
                    FlashMessage.SetFlashMessage(
                        "Url Yang Anda Tuju Hanya Dapat Diakses Oleh 'Dokter'.",
                        FlashMessage.FlashMessageType.Warning);
                }
            }
            else
            {
                FlashMessage.TemFlashMessageLogin();
            }

            return false;
        }

        #endregion Cek Session

        #region Index
        [HttpGet]
        public ActionResult Index()
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");
            else
                return RedirectToAction("TanganiPasien");
        }
        #endregion Index

        #region JSon
        [HttpGet]
        public JsonResult UpdateAntrianPasien()
        {
            var dtAntrian = this.db
                .TB_KUNJUNGAN_PASIEN
                .AsEnumerable()
                .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today && d.PENANGANAN_DOKTER == null)
                .OrderBy(d => d.TANGGAL_KUNJUNGAN);

            return Json(dtAntrian.Select(d => new
            {
                d.ID_PASIEN,
                d.TB_PASIEN.NAMA,
                d.TB_ANTRIAN_BEROBAT.NO_ANTRIAN
            }),
            JsonRequestBehavior.AllowGet);
        }
        #endregion JSon

        #region Tangani Pasien
        public ActionResult pilihPasien(FormCollection data)
        {
            TempData["id"] = data["id-pasien"];
            return RedirectToAction("TanganiPasien");
        }
        
        [HttpGet, ActionName("TanganiPasien")]
        public ActionResult TanganiPasienGet()
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                // data untuk views/DKR/Index -> html.partial()
                ViewData["Data"] = this.db
                    .TB_KUNJUNGAN_PASIEN
                    .AsEnumerable()
                    .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                        d.PENANGANAN_DOKTER == null && d.PENGAMBILAN_OBAT == null)
                    .OrderBy(d => d.TANGGAL_KUNJUNGAN);

                int? idPasien = null;
                if (TempData["id"] != null)
                    idPasien = Convert.ToInt32(TempData["id"]);

                // jika ada pasien yg dipilih (untuk ditangani oleh dokter) bukan nol / lebih dari nol.
                if (idPasien > 0)
                {
                    bool cekPasienDiAntrian = this.db
                        .TB_KUNJUNGAN_PASIEN
                        .AsEnumerable()
                        .Any(d => d.ID_PASIEN == idPasien &&
                            d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                            d.PENANGANAN_DOKTER == null);

                    // cek pasien di antrian, jika sudah terdaftar di antrian.
                    if (cekPasienDiAntrian)
                    {
                        ViewBag.idTerpilih = idPasien;

                        // ambil 'id kunjungan pasien' yg dipilih dokter
                        var dtKunjunganPasien = (IOrderedEnumerable<TB_KUNJUNGAN_PASIEN>)ViewData["Data"];
                        var idKunjunganPasien = dtKunjunganPasien
                            .Where(d => d.ID_PASIEN == idPasien)
                            .OrderByDescending(d => d.TANGGAL_KUNJUNGAN)
                            .Select(d => d.ID)
                            .ToList()
                            .First();

                        // ubah 'status panggilan' di Db menjadi false
                        TB_ANTRIAN_BEROBAT panggilAntrian = this.db.TB_ANTRIAN_BEROBAT.Find(idKunjunganPasien);
                        panggilAntrian.ID_KUNJUNGAN_PASIEN = idKunjunganPasien;
                        panggilAntrian.STATUS_PANGGILAN = false;
                        this.db.Entry(panggilAntrian).State = EntityState.Modified;
                        this.db.SaveChanges();
                    }
                    else
                    {
                        FlashMessage.SetFlashMessage(
                            "Pasien Tidak Terdaftar Diantrian.",
                            FlashMessage.FlashMessageType.Warning);
                    }
                }
                // jika idPasien yg di pilih selain lebih dari nol. contoh : -1, 0.
                else if(idPasien != null)
                {
                    FlashMessage.SetFlashMessage(
                        "Pasien Tidak Terdaftar.",
                        FlashMessage.FlashMessageType.Error);
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            // menentukan menu yg akan ditampilkan pada views/DKR/Index
            ViewBag.Menu = menu.TanganiPasien.ToString();

            // judul halaman
            ViewBag.TitlePage = "Tangani Pasien";

            return View("Index");
        }

        [HttpPost, ActionName("TanganiPasien")]
        public ActionResult TanganiPasienPost()
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                // simpan data pemeriksaan pasien ke 'TB_DATA_PENANGANAN_PASIEN'
                TB_DATA_PENANGANAN_PASIEN dt = new TB_DATA_PENANGANAN_PASIEN();
                UpdateModel(dt, null, null, new string[] { "ID, TB_KUNJUNGAN_PASIEN" });
                dt.ID_DOKTER = csmSession.GetIdSession();
                this.db.Entry(dt).State = EntityState.Added;

                // mengubah 'status panggilan' menjadi true / 1 sebagai tanda bahwa salah satu pasien telah di panggil
                var tesIDKunj = dt.ID_KUNJUNGAN_PASIEN;
                TB_ANTRIAN_BEROBAT panggilAntrian = this.db.TB_ANTRIAN_BEROBAT.Find(tesIDKunj);
                panggilAntrian.ID_KUNJUNGAN_PASIEN = (int)tesIDKunj;
                panggilAntrian.STATUS_PANGGILAN = true;
                this.db.Entry(panggilAntrian).State = EntityState.Modified;

                // ubah status pemeriksaan pasien menjadi true/1 pada 'TB_KUNJUNGAN_PASIEN'
                TB_KUNJUNGAN_PASIEN tB_KUNJUNGAN_PASIEN = this.db.TB_KUNJUNGAN_PASIEN.Find(dt.ID_KUNJUNGAN_PASIEN);
                tB_KUNJUNGAN_PASIEN.ID = (int)dt.ID_KUNJUNGAN_PASIEN;
                tB_KUNJUNGAN_PASIEN.PENANGANAN_DOKTER = true;
                this.db.Entry(tB_KUNJUNGAN_PASIEN).State = EntityState.Modified;
                this.db.SaveChanges();

                // get data pasien yg telah ditanganani untuk ditampilkan di flashmessage dibawah
                TB_PASIEN tB_PASIEN = this.db.TB_PASIEN.Single(d => d.ID == tB_KUNJUNGAN_PASIEN.ID_PASIEN);
                FlashMessage.SetFlashMessage(
                    $"Data Pemeriksaan Atas Nama: '{tB_PASIEN.NAMA}' dengan ID: '{tB_PASIEN.ID}' Telah Disimpan.",
                    FlashMessage.FlashMessageType.Success);
            }
            catch(Exception e)
            {
                e.ToString();
            }

            return RedirectToAction("TanganiPasien");
        }
        #endregion Tangani Pasien

        #region Riwayat Pasien
        [HttpGet]
        public ActionResult RiwayatPasien(string search, int? page, int? pageSize)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                RiwayatPasien riwayatPasien = new RiwayatPasien();
                ViewData["Data"] = riwayatPasien.GetData(search, page ?? 1, pageSize ?? 8);
            }
            catch(Exception e)
            {
                e.ToString();
            }

            // menentukan menu yg akan ditampilkan pada views/DKR/Index
            ViewBag.Menu = menu.RiwayatPasien.ToString();

            // judul halaman
            ViewBag.TitlePage = "Riwayat Pasien";

            return View("Index");
        }
        #endregion Riwayat Pasien
    }
}