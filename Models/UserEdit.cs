using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Usuario.Models
{
    public class UserEdit
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


        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres de longitud.", MinimumLength = 4)]
        public string Password { get; set; }


        public string Rol { get; } = "P";
    }
}
