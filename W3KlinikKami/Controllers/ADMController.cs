using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using W3KlinikKami.Models;
using System.Data.Entity;
using PagedList;
using PagedList.Mvc;
using W3KlinikKami.Core;

namespace W3KlinikKami.Controllers
{
    public class ADMController : Controller
    {
        private readonly DbEntities db = new DbEntities();
        private int id { get; set; }
        private string jabatan { get; set; }
        public enum PenangananPasien 
        {
            DaftarPasienBaru,
            BerobatPasien,
            PengambilanObat,
            DataPasien
        }

        private bool CekSession()
        {
            this.id = csmSession.GetIdSession();
            this.jabatan = csmSession.GetJabatanSession();
            // jika sudah login
            if (this.db.TB_USER.Any(j => j.ID == this.id && j.JABATAN == this.jabatan))
            {
                // jika kode jabatan pada akun SESUAI dengan nama controller
                if (nameof(ADMController) == this.jabatan + "Controller")
                {
                    // data user yg digunakan -> 'Nama', 'Jabatan', 'Foto'
                    ViewBag.DT_USER = this.db.TB_USER.Find(this.id);
                    return true;
                }
                else // jika kode jabatan pada akun TIDAK SESUAI dengan nama controller
                {
                    FlashMessage.SetFlashMessage(
                        "Link Yang Anda Tuju Hanya Dapat Diakses Oleh 'Admin Pelayanan'",
                        FlashMessage.FlashMessageType.Warning);
                    return false;
                }
            }
            else // jika belum login
            {
                FlashMessage.TemFlashMessageLogin();
                return false;
            }
        }

        public ActionResult Index()
        {
            if (this.CekSession())
                return RedirectToAction("BerobatPasien");
            else
                return RedirectToAction("Index", "Index");
        }

        /* Begin: BerobatPasien -------------------------------------------------------------------- */
        [HttpGet]
        public ActionResult BerobatPasien(int? page, string search)
        {
            if (this.CekSession())
            {
                // jika Id pasien di pilih untuk daftar berobat
                if(int.TryParse(Request.QueryString["pilih_id"], out int id))
                    TempData["ID_PASIEN_TERPILIH"] = this.db.TB_PASIEN.Find(id);

                // get semua data pasien
                int.TryParse(search, out int searchId);
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN
                    .Where(d => d.ID == searchId || d.NAMA.Contains(search) || search == null)
                    .OrderBy(d => d.NAMA)
                    .ToPagedList(page ?? 1, 9);

                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.BerobatPasien;

                return View("Index");
            }
            else
            {
                return RedirectToAction("Index", "Index");
            }
        }

        [HttpPost]
        public ActionResult BerobatPasien([Bind(Include = "ID, NAMA")] TB_PASIEN dt, int? page, string search)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                if (dt.ID > 0)
                {
                    /* syarat pasien:
                     * - jika pasien belum mendaftar berobat pada hari ini,
                     * - jika pasien sudah mendaftar hari ini, dan ingin mendaftar lagi maka syaratnya:
                     *      @ pasien sudah ditangani oleh dokter,
                     *      @ dan pasien sudah mengambil obat.
                     * - jika 'cekAntrianPasien' = FALSE maka pasien BOLEH dicatat di antrian.
                     */
                    bool cekAntrianPasien = this.db
                        .TB_KUNJUNGAN_PASIEN
                        .AsEnumerable()
                        .Any(d => d.ID_PASIEN == dt.ID &&
                            d.TANGGAL_KUNJUNGAN.Date == DateTime.Today &&
                            (d.PENANGANAN_DOKTER == null ||
                             d.PENGAMBILAN_OBAT == null ||
                             d.PENGAMBILAN_OBAT == false));

                    if (!cekAntrianPasien)
                    {
                        // simpan ke db -> TB_KUNJUNGAN_PASIEN
                        TB_KUNJUNGAN_PASIEN tB_KUNJUNGAN_PASIEN = new TB_KUNJUNGAN_PASIEN()
                        {
                            TANGGAL_KUNJUNGAN = DateTime.Now,
                            ID_PASIEN = dt.ID
                        };
                        this.db.Entry(tB_KUNJUNGAN_PASIEN).State = EntityState.Added;
                        this.db.SaveChanges();

                        // simpan ke db -> TB_ANTRIAN_BEROBAT
                        List<TB_KUNJUNGAN_PASIEN> pasienHariIni = this.db.TB_KUNJUNGAN_PASIEN
                            .AsEnumerable()
                            .Where(d => d.TANGGAL_KUNJUNGAN.Date == DateTime.Today)
                            .OrderByDescending(d => d.TANGGAL_KUNJUNGAN)
                            .ToList();

                        if (pasienHariIni.Count() > 0)
                        {
                            int idKunjungan = pasienHariIni.First().ID;
                            if (!this.db.TB_ANTRIAN_BEROBAT.Any(d => d.ID_KUNJUNGAN_PASIEN == idKunjungan))
                            {
                                TB_ANTRIAN_BEROBAT antrian = new TB_ANTRIAN_BEROBAT
                                {
                                    ID_KUNJUNGAN_PASIEN = idKunjungan,
                                    NO_ANTRIAN = pasienHariIni.Count()
                                };

                                this.db.TB_ANTRIAN_BEROBAT.Add(antrian);
                                this.db.SaveChanges();
                            }
                        }

                        FlashMessage.SetFlashMessage(
                            $"Pasien Atas Nama '{dt.NAMA}' Dengan ID '{dt.ID}' Berhasil Dicatat Di Antrian",
                            FlashMessage.FlashMessageType.Success);
                    }
                    else // jika pasien terpilih SUDAH TERDAFTAR di antrian
                    {
                        FlashMessage.SetFlashMessage(
                            $"Pasien Atas Nama '{dt.NAMA}' Dengan ID '{dt.ID}' Sudah Terdaftar DiAntrian",
                            FlashMessage.FlashMessageType.Warning);
                    }
                }
                else
                {
                    FlashMessage.SetFlashMessage(
                        "Pilih Pasien Terlebih Dahulu.",
                        FlashMessage.FlashMessageType.Warning);
                }
            }
            catch(Exception e)
            {
                e.ToString();
            }

            return RedirectToAction("BerobatPasien", new { page, search });
        }
        /* End: BerobatPasien -------------------------------------------------------------------- */

        /* Begin: PengambilanObat -------------------------------------------------------------------- */
        [HttpGet]
        public ActionResult PengambilanObat()
        {
            if (this.CekSession())
            {
                // Ambil semua data pasien
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN.ToList();

                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.PengambilanObat;

                return View("Index");
            }
            else
            {
                return RedirectToAction("Index", "Index");
            }
        }
        /* End: PengambilanObat -------------------------------------------------------------------- */

        /* Begin: DataPasien -------------------------------------------------------------------- */
        public ActionResult DataPasien(int? page, string search, int? pageSize)
        {
            if (this.CekSession())
            {
                // jumlah total data pasien
                ViewBag.TotDataPasien = this.db.TB_PASIEN.Count();

                // data yg akan ditampilkan pada table sesuai dengan jumlah row, dan search
                int.TryParse(search, out int idPasien);
                ViewData["DT_PASIEN"] = this.db.TB_PASIEN
                    .Where(d => d.ID == idPasien || d.NAMA.Contains(search) || search == null)
                    .OrderByDescending(d => d.TERDAFTAR)
                    .ToPagedList(page ?? 1, pageSize ?? 8);

                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.DataPasien;

                return View("Index");
            }
            else
            {
                return RedirectToAction("Index", "Index");
            }
        }

        [HttpPost]
        public ActionResult DataPasien_Edit([Bind(Exclude = "TERDAFTAR")] TB_PASIEN dt, int? page, string search, int? pageSize)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            if (ModelState.IsValid)
            {
                try
                {
                    TB_PASIEN dtUpdate = this.db.TB_PASIEN.Find(dt.ID);
                    dtUpdate.NAMA = dt.NAMA;
                    dtUpdate.JENIS_KELAMIN = dt.JENIS_KELAMIN;
                    dtUpdate.TANGGAL_LAHIR = dt.TANGGAL_LAHIR;
                    dtUpdate.GOLONGAN_DARAH = dt.GOLONGAN_DARAH;
                    dtUpdate.NO_HP = dt.NO_HP;
                    dtUpdate.ALAMAT = dt.ALAMAT;
                    dtUpdate.TERDAFTAR = this.db.TB_PASIEN.Find(dt.ID).TERDAFTAR;

                    this.db.Entry(dtUpdate).State = EntityState.Modified;
                    this.db.SaveChanges();
                    FlashMessage.SetFlashMessage(
                        $"Data Pasien Atas Nama: '{dt.NAMA}' Dengan ID: '{dt.ID}' Telah Diubah.",
                        FlashMessage.FlashMessageType.Success);
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }

            // ModelState.IsValid -> true / false tetap return ke -> "DataPasien"
            return RedirectToAction("DataPasien", new
            {
                @page = page,
                @search = search,
                @pageSize = pageSize
            });
        }

        [HttpPost]
        public ActionResult DataPasien_Delete([Bind(Include = "ID")] TB_PASIEN dt)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                var tB_KUNJUNGAN_PASIEN = this.db.TB_KUNJUNGAN_PASIEN;
                tB_KUNJUNGAN_PASIEN
                    .Where(d => d.ID_PASIEN == dt.ID)
                    .ToList()
                    .ForEach(d => tB_KUNJUNGAN_PASIEN.Remove(d));
                //this.db.Entry(tB_KUNJUNGAN_PASIEN).State = EntityState.Deleted;
                //dt.TB_KUNJUNGAN_PASIEN.Remove(tB_KUNJUNGAN_PASIEN.whe);
                //this.db.Entry(dt).State = EntityState.Deleted;
                this.db.SaveChanges();
                FlashMessage.SetFlashMessage("Data Telah Dihapus.", FlashMessage.FlashMessageType.Success);
            }
            catch (Exception e)
            {
                e.ToString();
                FlashMessage.SetFlashMessage(e.ToString(), FlashMessage.FlashMessageType.Success);
            }

            return RedirectToAction("DataPasien");
        }
        /* End: DataPasien -------------------------------------------------------------------- */

        /* Begin: DaftarPasienBaru -------------------------------------------------------------------- */
        [HttpGet]
        public ActionResult DaftarPasienBaru()
        {
            if (this.CekSession())
            {
                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.DaftarPasienBaru;

                return View("Index");
            }
            else
            {
                return RedirectToAction("Index", "Index");
            }
        }

        [HttpPost]
        public ActionResult DaftarPasienBaru([Bind(Exclude = "ID, TERDAFTAR")] TB_PASIEN dt)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            if (ModelState.IsValid)
            {
                try
                {
                    dt.TERDAFTAR = DateTime.Now;
                    this.db.Entry(dt).State = EntityState.Added;
                    this.db.SaveChanges();
                    FlashMessage.SetFlashMessage($"Data Pasien Berhasil Disimpan Atas Nama: '{dt.NAMA}'.", FlashMessage.FlashMessageType.Success);
                    // ModePenanganan untuk menentukan 'menu'
                    ViewBag.ModePenanganan = PenangananPasien.DaftarPasienBaru.ToString();
                    return RedirectToAction("DataPasien", new { @search = dt.NAMA });
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }

            // ModePenanganan untuk menentukan 'menu'
            ViewBag.ModePenanganan = PenangananPasien.DaftarPasienBaru.ToString();

            // ModelState.IsValid -> true / false tetap return ke -> "Index"
            return View("Index");
        }
        /* End: DaftarPasienBaru -------------------------------------------------------------------- */
    }
}