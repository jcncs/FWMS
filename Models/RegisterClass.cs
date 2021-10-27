using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class RegisterClass
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Disable { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string PwdHash { get; set; }

        public string Email { get; set; }
        public string HomePhone { get; set; }

        public string OfficePhone { get; set; }

        public string roleName { get; set; }
    }
}
