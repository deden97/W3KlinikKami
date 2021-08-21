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

        // ambil data pasien yg masih aktif
        private List<TB_PASIEN> GetDataPasienAktif(out int totalData)
        {
            // ambil semua data pasien
            var pasienAktif = this.db.TB_PASIEN.ToList();

            // data pasien yg telah dihapus
            var pasienTerhapus = this.db.TB_PASIEN_TERHAPUS.ToList();

            // jika ada data pasien terhapus, maka hapus data pasien yg telah dihapus pada variabel 'pasienAktif'
            if (pasienTerhapus.Count > 0)
                pasienTerhapus.ForEach(h => pasienAktif.Remove(pasienAktif.Single(a => a.ID == h.ID_PASIEN)));

            // hitung total data pasien
            totalData = pasienAktif.Count;

            // mengembalikan data pasien aktif
            return pasienAktif;
        }

        public enum PenangananPasien 
        {
            DaftarPasienBaru,
            BerobatPasien,
            PengambilanObat,
            DataPasien,
            RiwayatPasien
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
                // jika kode jabatan pada akun TIDAK SESUAI dengan nama controller
                else
                {
                    FlashMessage.SetFlashMessage(
                        "Link Yang Anda Tuju Hanya Dapat Diakses Oleh 'Admin Pelayanan'",
                        FlashMessage.FlashMessageType.Warning);
                    return false;
                }
            }
            // jika belum login
            else
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
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                // jika Id pasien di pilih untuk daftar berobat
                if (int.TryParse(Request.QueryString["pilih_id"], out int id))
                    TempData["ID_PASIEN_TERPILIH"] = this.db.TB_PASIEN.Find(id);

                // get semua data pasien
                if (string.IsNullOrEmpty(search))
                {
                    ViewData["DT_PASIEN"] = this.GetDataPasienAktif(out int tot)
                   .OrderBy(d => d.NAMA)
                   .ToPagedList(page ?? 1, 9);
                }
                else
                {
                    int.TryParse(search, out int searchId);
                    ViewData["DT_PASIEN"] = this.GetDataPasienAktif(out int tot)
                        .Where(d => d.ID == searchId || d.NAMA.Contains(search))
                        .OrderBy(d => d.NAMA)
                        .ToPagedList(page ?? 1, 9);
                }
            }
            catch(Exception e)
            {
                e.ToString();
            }

            // ModePenanganan untuk menentukan 'menu'
            ViewBag.ModePenanganan = PenangananPasien.BerobatPasien;

            // Judul Halaman
            ViewBag.TitlePage = "Daftar Berobat";
            return View("Index");
        }

        [HttpPost]
        public ActionResult BerobatPasien([Bind(Include = "ID, NAMA")] TB_PASIEN dt, int? page, string search)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                // jika id pasien lebih dari nol/sesuai dengan yg ada di Db
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
                    // jika pasien terpilih SUDAH TERDAFTAR di antrian
                    else
                    {
                        FlashMessage.SetFlashMessage(
                            $"Pasien Atas Nama '{dt.NAMA}' Dengan ID '{dt.ID}' Sudah Terdaftar DiAntrian",
                            FlashMessage.FlashMessageType.Warning);
                    }
                }
                // jika id pasien tidak sesuai dengan Db.
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
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                int totDataPasien;

                // jika keyword search kosong
                if (string.IsNullOrEmpty(search))
                {
                    ViewData["DT_PASIEN"] = this.GetDataPasienAktif(out totDataPasien)
                        .OrderByDescending(d => d.TERDAFTAR)
                        .ToPagedList(page ?? 1, pageSize ?? 8);
                }
                // jika keyword search tidak kosong
                else
                {
                    int.TryParse(search, out int idPasien);
                    ViewData["DT_PASIEN"] = this.GetDataPasienAktif(out totDataPasien)
                        .Where(d => d.ID == idPasien || d.NAMA.Contains(search))
                        .OrderByDescending(d => d.TERDAFTAR)
                        .ToPagedList(page ?? 1, pageSize ?? 8);
                }

                // jumlah total data pasien
                ViewBag.TotDataPasien = totDataPasien;

                // ModePenanganan untuk menentukan 'menu'
                ViewBag.ModePenanganan = PenangananPasien.DataPasien;

                // Judul Halaman
                ViewBag.TitlePage = "Data Pasien";
            }
            catch(Exception e)
            {
                e.ToString();
            }
               
            return View("Index");
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
        public ActionResult DataPasien_Delete([Bind(Include = "ID, NAMA")] TB_PASIEN dt)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                TB_PASIEN_TERHAPUS hapusPasien = new TB_PASIEN_TERHAPUS { ID_PASIEN = dt.ID };
                this.db.TB_PASIEN_TERHAPUS.Add(hapusPasien);
                this.db.SaveChanges();
                FlashMessage.SetFlashMessage(
                    $"Data Pasien Atas Nama: '{dt.NAMA}' Dengan ID: '{dt.ID}' Telah Dihapus.",
                    FlashMessage.FlashMessageType.Success);
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
                ViewBag.TitlePage = "Daftar Pasien Baru";
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

            ViewBag.TitlePage = "Daftar Pasien Baru";
            if (ModelState.IsValid)
            {
                try
                {
                    dt.TERDAFTAR = DateTime.Now;
                    this.db.Entry(dt).State = EntityState.Added;
                    this.db.SaveChanges();
                    FlashMessage.SetFlashMessage(
                        $"Data Pasien Berhasil Disimpan Atas Nama: '{dt.NAMA}'.",
                        FlashMessage.FlashMessageType.Success);
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

        [HttpGet]
        public ActionResult RiwayatPasien(string search, int? page, int? pageSize)
        {
            if (!this.CekSession())
                return RedirectToAction("Index", "Index");

            try
            {
                RiwayatPasien riwayatPasien = new RiwayatPasien();
                ViewData["DT_PASIEN"] = riwayatPasien.GetData(search, page ?? 1, pageSize ?? 8);
            }
            catch(Exception e)
            {
                e.ToString();
            }

            ViewBag.ModePenanganan = PenangananPasien.RiwayatPasien;
            ViewBag.TitlePage = "Riwayat Pasien";
            return View("Index");
        }
    }
}