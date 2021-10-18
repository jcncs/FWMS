using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class ViewProfileModel
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string homePhone { get; set; }
        public string officePhone { get; set; }
        public string email { get; set; }
        public string isAccountDisabled { get; set; }
        public string roleId { get; set; }
        public string roleName { get; set; }
    }
}
