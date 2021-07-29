using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace W3KlinikKami.Models
{
    [MetadataType(typeof(TB_USER_META_DATA))]
    public partial class TB_USER
    {
        public HttpPostedFileBase FOTO_TERPILIH { get; set; }
    }

    public class TB_USER_META_DATA
    {
        public int ID { get; set; }

        [MaxLength(50, ErrorMessage = "Maksimal 50 Karakter, Nama Bisa Disingkat.")]
        [Required(ErrorMessage = "Nama Tidak Boleh Kosong.")]
        [DisplayName("Nama Lengkap")]
        public string NAMA { get; set; }

        [Required(ErrorMessage = "Jenis Kelamin Harus Dipilih.")]
        [DisplayName("Jenis Kelamin")]
        public string JENIS_KELAMIN { get; set; }

        //[MaxLength(18, ErrorMessage = "Maksimal 18 Karakter, Masukan Nomor HP Yang Benar.")]
        [Required(ErrorMessage = "Nomor HP Tidak Boleh Kosong.")]
        [DisplayName("Nomor HP")]
        public decimal NO_HP { get; set; }

        [Required(ErrorMessage = "Alamat Tidak Boleh Kosong.")]
        [DisplayName("Alamat")]
        public string ALAMAT { get; set; }

        [DisplayName("Foto Profil")]
        public string FOTO { get; set; }

        [Required(ErrorMessage = "Jabatan Harus Dipilih.")]
        [DisplayName("Jabatan")]
        public string JABATAN { get; set; }

        [Required()]
        [DisplayName("Terdaftar")]
        [DisplayFormat(DataFormatString = "{dd-MM-yyyy hh:mm:ss}")]
        public System.DateTime TERDAFTAR { get; set; }
    }
}