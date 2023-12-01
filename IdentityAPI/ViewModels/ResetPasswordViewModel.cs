using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApp.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(55, MinimumLength = 5)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(55, MinimumLength = 5)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}