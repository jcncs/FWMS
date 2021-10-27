using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class CreateDonationModel
    {
        public List<ViewLocationsModel> LocationList { get; set; }

        public List<ViewFoodDescriptionsModel> FoodDescriptionList { get; set; }

        [Required]
        public string DonationName { get; set; }

        [Required]
        public string Quantity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string CreatedBy { get; set; }

        [Required]
        public string LocationId { get; set; }

        [Required]
        public string FoodId { get; set; }

        public string UserId { get; set; }
    }
}
