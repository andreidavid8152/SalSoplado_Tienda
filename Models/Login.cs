using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Usuario.Models
{
    public class Login
    {
        [Required(ErrorMessage = "El username es obligatorio.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El password es obligatorio.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
