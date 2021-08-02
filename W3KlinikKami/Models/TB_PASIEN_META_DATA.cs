using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace W3KlinikKami.Models
{
    [MetadataType(typeof(TB_PASIEN_META_DATA))]
    public partial class TB_PASIEN
    {
    }

    public class TB_PASIEN_META_DATA
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Nama Harus Diisi.")]
        [DisplayName("Nama Lengkap")]
        public string NAMA { get; set; }

        [Required(ErrorMessage = "Jenis Kelamin Harus Diisi.")]
        [DisplayName("Jenis Kelamin")]
        public string JENIS_KELAMIN { get; set; }

        [Required(ErrorMessage = "Tanggal Lahir Harus Diisi.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [DisplayName("Tanggal Lahir")]
        public System.DateTime TANGGAL_LAHIR { get; set; }

        [DisplayName("Golongan Darah")]
        [DisplayFormat(NullDisplayText = "-")]
        public string GOLONGAN_DARAH { get; set; }

        [Required(ErrorMessage = "Nomor HP Harus Diisi.")]
        [DisplayName("Nomor HP")]
        [DisplayFormat(DataFormatString = "+62 {0:0}")]
        public decimal NO_HP { get; set; }

        [Required(ErrorMessage = "Alamat Harus Diisi.")]
        [DisplayName("Alamat")]
        public string ALAMAT { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm:ss}")]
        [DisplayName("Terdaftar")]
        public System.DateTime TERDAFTAR { get; set; }
    }
}