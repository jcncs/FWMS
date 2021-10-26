using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class Roles
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdateDate { get; set; }

    }
}
