using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Tienda.Models
{
    public class ProductoDetalleEdit
    {
        public int ID { get; set; }

        [Required]
        public int LocalID { get; set; }

        [Required]
        [StringLength(maximumLength: 255, ErrorMessage = "El nombre es demasiado largo")]
        public string Nombre { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Required]
        public DateTime FechaVencimiento { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "El precio original debe ser mayor a 0")]
        public decimal PrecioOriginal { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "El precio con oferta debe ser mayor a 0")]
        public decimal PrecioOferta { get; set; }

        [Required]
        public string Categoria { get; set; }

        [Required]
        public List<string> ImagenesUrls { get; set; }

    }
}
