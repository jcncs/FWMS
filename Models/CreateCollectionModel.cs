using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class CreateCollectionModel
    {
        [Required]
        [DisplayName("Donation Id")]
        public string donationId { get; set; }

        [Required]
        [DisplayName("Collection Name")]
        public string CollectionName { get; set; }

        [DisplayName("Reserve By")]
        public string ReservedBy { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        [DisplayName("Collection Date")]
        public DateTime? CollectionDate { get; set; }
        public List<ViewDonationsModel> DonationList { get; set; }
    }
}
