using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Tienda.Models
{
    public class ImagenLocal
    {
        public int ID { get; set; }

        [Required]
        public string Url { get; set; }

        public int LocalID { get; set; }
    }
}
