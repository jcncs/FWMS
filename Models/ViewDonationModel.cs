using System;
using System.Collections.Generic;

namespace FWMS.Models
{
    public class ViewDonationModel
    {
        public string donationId { get; set; }
        public string donationName { get; set; }
        public string category { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public DateTime expiryDate { get; set; }
        public string locationName { get; set; }
        public DateTime createdDate { get; set; }
        public string createdBy { get; set; }
        public DateTime updatedDate { get; set; }
        public string updatedBy { get; set; }
        public string userId { get; set; }
        public string reservedBy { get; set; }
        public DateTime? reservedDate { get; set; }
        public string collectionId { get; set; }
        public string foodEntryId { get; set; }
    }
}
