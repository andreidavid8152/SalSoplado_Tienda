using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Usuario.Models
{
    public class UserRegistration
    {
        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [MinLength(5, ErrorMessage = "El campo Nombre debe tener al menos 5 caracteres.")]
        public string Nombre { get; set; }


        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [EmailAddress(ErrorMessage = "Por favor, ingresa una dirección de correo electrónico válida.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "El campo Username es obligatorio.")]
        [MinLength(4, ErrorMessage = "El campo Username debe tener al menos 4 caracteres.")]
        public string Username { get; set; }


        [Required(ErrorMessage = "El campo Password es obligatorio.")]
        [MinLength(4, ErrorMessage = "El campo Password debe tener al menos 4 caracteres.")]
        public string Password { get; set; }


        public string Rol { get; } = "P";
    }
}
