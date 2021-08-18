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
        private bool CekSession()
        {
            this.ID = csmSession.GetIdSession();
            this.JABATAN = csmSession.GetJabatanSession();
            if(this.db.TB_USER.Any(d => d.ID == this.ID && d.JABATAN == this.JABATAN))
            {
                if(nameof(DKRController) == this.JABATAN + "Controller")
                {
                    ViewBag.DT_USER = this.db.TB_USER.Find(this.ID);
                    return true;
                }
                else
                {
                    FlashMessage.SetFlashMessage(
                        "Link Yang Anda Tuju Hanya Dapat Diakses Oleh 'Dokter'.",
                        FlashMessage.FlashMessageType.Warning);
                    return false;
                }
            }
            else
            {
                FlashMessage.SetFlashMessage(
                    "Anda Harus Login Terlebih Dahulu.",
                    FlashMessage.FlashMessageType.Warning);
                return false;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");
            else
                return RedirectToAction("TanganiPasien");
        }

        [HttpGet]
        public ActionResult UpdateAntrianPasien()
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

        /* Begin: Tangani Pasien -------------------------------------------------------------------------------------------------*/
        [HttpGet]
        public ActionResult TanganiPasien(int? idPasien)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                // menentukan menu yg akan ditampilkan pada views/DKR/Index
                ViewBag.Menu = menu.TanganiPasien.ToString();

                // data untuk views/DKR/Index -> html.partial()
                ViewData["Data"] = this.db
                    .TB_KUNJUNGAN_PASIEN
                    .AsEnumerable()
                    .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                        d.PENANGANAN_DOKTER == null &&
                        (d.PENGAMBILAN_OBAT == null || d.PENGAMBILAN_OBAT == false))
                    .OrderBy(d => d.TANGGAL_KUNJUNGAN);

                // jika ada pasien yg dipilih (untuk ditangani oleh dokter) bukan nol / lebih dari nol.
                if (idPasien > 0)
                {
                    bool cekPasienDiAntrian = this.db
                        .TB_KUNJUNGAN_PASIEN
                        .AsEnumerable()
                        .Any(d => d.ID_PASIEN == idPasien &&
                            d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                            (d.PENANGANAN_DOKTER == null || d.PENANGANAN_DOKTER == false));

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
                            "Pasien Tidak Terddaftar Diantrian.",
                            FlashMessage.FlashMessageType.Warning);
                    }
                }
                // jika idPasien yg di pilih selain lebih dari nol. contoh : -1, 0.
                else if(idPasien != null)
                {
                    FlashMessage.SetFlashMessage(
                        "Pasien Tidak Terddaftar.",
                        FlashMessage.FlashMessageType.Error);
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult TanganiPasien()
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
        /* End: Tangani Pasien -------------------------------------------------------------------------------------------------*/

        [HttpGet]
        public ActionResult RiwayatPasien()
        {
            this.CekSession();
            ViewBag.Menu = menu.RiwayatPasien.ToString();
            return View("Index");
        }
    }
}