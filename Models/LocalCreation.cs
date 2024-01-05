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


        [Required(ErrorMessage = "El campo Descripcion es obligatorio.")]
        [MinLength(10, ErrorMessage = "El campo Descripcion debe tener al menos 10 caracteres.")]
        public string Descripcion { get; set; }


        [Required(ErrorMessage = "El campo Direccion es obligatorio.")]
        public string Direccion { get; set; }


        //public List<Imagen> Imagenes { get; set; }
    }
}
