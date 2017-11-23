

using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models.Account 
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
       
    }
}