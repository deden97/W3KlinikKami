//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace W3KlinikKami.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TB_DATA_PENANGANAN_PASIEN
    {
        public int ID { get; set; }
        public Nullable<int> ID_KUNJUNGAN_PASIEN { get; set; }
        public string KELUHAN { get; set; }
        public string PEMERIKSAAN { get; set; }
        public string DIAGNOSA { get; set; }
        public string RESEP_OBAT { get; set; }
        public string KETERANGAN { get; set; }
        public Nullable<int> ID_DOKTER { get; set; }
    
        public virtual TB_KUNJUNGAN_PASIEN TB_KUNJUNGAN_PASIEN { get; set; }
        public virtual TB_USER TB_USER { get; set; }
    }
}