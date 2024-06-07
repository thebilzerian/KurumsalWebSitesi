using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace KurumsalWeb.Models.Model
{
    [Table("Slider")]
    public class Slider
    {
        [Key]
        public int SliderId { get; set; }
        [DisplayName("Slider Başlık"), StringLength(30, ErrorMessage = "30 karakter olmalıdır.")]
        public string Baslik { get; set; }
        [DisplayName("Slider Açıklama"), StringLength(150, ErrorMessage = "150 karakter olmalıdır.")]
        public string Aciklama { get; set; }
        [DisplayName("Slider Resim"), StringLength(500)]
        public string ResimURL { get; set; }
    }
}