using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace W3KlinikKami.Models
{
    [MetadataType(typeof(TB_USER_META_DATA))]
    public partial class TB_USER
    {
        
    }

    public class TB_USER_META_DATA
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Nama Tidak Boleh Konsong.")]
        public string NAMA { get; set; }

        [Required(ErrorMessage = "Jenis Kelamin Harus Dipilih.")]
        public string JENIS_KELAMIN { get; set; }

        [Required(ErrorMessage = "Nomor HP Tidak Boleh Konsong.")]
        public decimal NO_HP { get; set; }

        [Required(ErrorMessage = "Alamat Tidak Boleh Konsong.")]
        public string ALAMAT { get; set; }

        public string FOTO { get; set; }

        [Required(ErrorMessage = "Jabatan Harus Dipilih.")]
        public string JABATAN { get; set; }
    }
}