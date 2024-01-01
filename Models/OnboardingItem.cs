using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalSoplado_Usuario.Models
{
    public class OnboardingItem
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public bool IsLast { get; set; }
    }
}
