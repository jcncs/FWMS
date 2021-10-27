using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class UserProfileModel
    {
        [DisplayName("User Id")]
        public string UserId { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        [DisplayName("Home Phone")]
        public string HomePhone { get; set; }
        [DisplayName("Office Phone")]
        public string OfficePhone { get; set; }
        [DisplayName("Email")]
        public string Email { get; set; }
        [DisplayName("Disable")]
        public string IsAccountDisabled { get; set; }
        public string RoleId { get; set; }
        [DisplayName("Role")]
        public string RoleName { get; set; }
    }
}
