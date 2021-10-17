using System;
using System.Collections.Generic;

namespace FWMS.Models
{
    public class ViewCollectionsModel
    {
        public string collectionId { get; set; }
        public string collectionName { get; set; }
        public DateTime? collectionDate { get; set; }
        public string donationId { get; set; }
    }
}
