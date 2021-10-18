using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class LoginModel
    {
        [Required]
        public string userName { get; set; }

        [Required]
        public string pwdHash { get; set; }
    }
}
