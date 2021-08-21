using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace W3KlinikKami.Models
{
    [MetadataType(typeof(TB_KUNJUNGAN_PASIEN_META_DATA))]
    public partial class TB_KUNJUNGAN_PASIEN
    {
    }

    public class TB_KUNJUNGAN_PASIEN_META_DATA
    {
        [DisplayName("Tanggal Kunjungan")]
        public DateTime TANGGAL_KUNJUNGAN { get; set; }

        [DisplayName("ID Pasien")]
        public int ID_PASIEN { get; set; }
    }
}