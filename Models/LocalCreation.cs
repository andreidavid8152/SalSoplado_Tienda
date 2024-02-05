using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Tienda.Models
{
    public class LocalCreation
    {

        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [MinLength(5, ErrorMessage = "El campo Nombre debe tener al menos 5 caracteres.")]
        public string Nombre { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

        public string Telefono { get; set; }

        [Required(ErrorMessage = "El campo Direccion es obligatorio.")]
        public string Direccion { get; set; }

        public string Logo { get; set; }

    }
}
