

using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models.Account 
{
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string FirstAndLastName { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        [Compare("Password", ErrorMessage = "Confirm Password must match.")]
        public string ConfirmPassword{ get; set; }
        
    }
}