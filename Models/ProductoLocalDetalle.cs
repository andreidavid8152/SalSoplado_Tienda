using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Tienda.Models
{
    public class ProductoLocalDetalle
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioOriginal { get; set; }
        public decimal PrecioDescuento { get; set; }
        public string Categoria { get; set; }
        public string Imagen { get; set; }
    }
}
