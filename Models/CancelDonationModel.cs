using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class CancelDonationModel
    {
        [Required]
        public string DonationId { get; set; }
    }
}
