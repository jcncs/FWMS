using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FWMS.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Prompt = "Email")]
        public string Email { get; init; }
        [Required]
        [Display(Prompt = "Username")]
        public string Username { get; set; }
        [Required]
        [DisplayName("Contact number")]
        [Display(Prompt = "Contact number")]
        public string ContractNumber { get; set; }
        [Display(Prompt = "Secondary contact number")]
        [DisplayName("Secondary contact number")]
        public string SecContractNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; }
        [Required]
        public string RoleDown { get; set; }

    }
}
