using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace W3KlinikKami.Models
{
    [MetadataType(typeof(TB_DATA_PENANGANAN_PASIEN_META_DATA))]
    public partial class TB_DATA_PENANGANAN_PASIEN
    {
    }

    public class TB_DATA_PENANGANAN_PASIEN_META_DATA
    {
        public int ID { get; set; }
        public Nullable<int> ID_KUNJUNGAN_PASIEN { get; set; }
        [Required(ErrorMessage = "Harus Diisi.")]
        public string KELUHAN { get; set; }
        public string PEMERIKSAAN { get; set; }
        public string DIAGNOSA { get; set; }
        public string RESEP_OBAT { get; set; }
        public string KETERANGAN { get; set; }
        public Nullable<int> ID_DOKTER { get; set; }
    }
}