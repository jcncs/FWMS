using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class ViewLocationsModel
    {
        public string locationId { get; set; }
        public string locationName { get; set; }
        public string address { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }
    }
}
