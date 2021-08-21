using PagedList;
using System.Linq;

namespace W3KlinikKami.Models
{
    public class RiwayatPasien
    {
        private readonly DbEntities db = new DbEntities();

        // ambil semua data pada 'TB_KUNJUNGAN_PASIEN' yang sesuai dengan 'ID_PASIEN' yg berada pada tabel 'TB_KUNJUNGAN_PASIEN'
        public IPagedList<TB_KUNJUNGAN_PASIEN> GetByIdPasien(string search, int page, int pageSize)
        {
            int.TryParse(search, out int idPasien);
            var data = this.db
                .TB_KUNJUNGAN_PASIEN
                .OrderByDescending(d => d.TANGGAL_KUNJUNGAN)
                .Where(d => (d.ID_PASIEN == idPasien || d.TB_PASIEN.NAMA.Contains(search)) &&
                    d.PENANGANAN_DOKTER == true &&
                    d.PENGAMBILAN_OBAT == null) // && d.PENGAMBILAN_OBAT == true
                .ToPagedList(page, pageSize);
            return data;
        }

        // ambil semua data pada 'TB_KUNJUNGAN_PASIEN'
        public IPagedList<TB_KUNJUNGAN_PASIEN> GetAll(int page, int pageSize)
        {
            var data = this.db
                .TB_KUNJUNGAN_PASIEN
                .OrderByDescending(d => d.TANGGAL_KUNJUNGAN)
                .Where(d => d.PENANGANAN_DOKTER == true && d.PENGAMBILAN_OBAT == null) // && d.PENGAMBILAN_OBAT == true
                .ToPagedList(page, pageSize);
            return data;
        }

        // ambil data berdasarkan 'search'
        public IPagedList<TB_KUNJUNGAN_PASIEN> GetData(string search, int page, int pageSize)
        {
            return string.IsNullOrEmpty(search) ?
                this.GetAll(page, pageSize) :
                this.GetByIdPasien(search, page, pageSize);
        }

        // ambil data pada tabel 'TB_DATA_PENANGANAN_PASIEN' sesuai dengan 'ID_KUNJUNGAN_PASIEN'
        public IQueryable<TB_DATA_PENANGANAN_PASIEN> GetDetailPenangananById(int idPemeriksaan)
        {
            var data = this.db
                .TB_DATA_PENANGANAN_PASIEN
                .Where(d => d.ID_KUNJUNGAN_PASIEN == idPemeriksaan);
            return data;
        }
    }
}