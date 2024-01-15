using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Tienda.Models
{
    public class LocalLoad
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del local es requerido.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La dirección es requerida.")]
        public string Direccion { get; set; }

        public String Imagen { get; set; }
    }
}
