using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels
{
    public class RegisterViewModel
    {

        [Required]
        [StringLength(55)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(55, MinimumLength = 5)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(55, MinimumLength = 5)]
        public string ConfirmPassword { get; set; } = string.Empty;

    }
}