using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace FWMS.Models
{
    public class ForgetPasswordModel
    {
        [Required]
        public string email { get; set; }
    }
}