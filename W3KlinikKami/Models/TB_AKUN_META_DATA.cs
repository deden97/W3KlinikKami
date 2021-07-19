using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace W3KlinikKami.Models
{
    [MetadataType(typeof(TB_AKUN_META_DATA))]
    public partial class TB_AKUN
    {
       
    }

    public class TB_AKUN_META_DATA
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "\"Username\" Tidak Boleh Kosong!")]
        public string USERNAME { get; set; }
        [Required(ErrorMessage = "\"Password\" Tidak Boleh Kosong!")]
        public string PASSWORD_AKUN { get; set; }
    }
}