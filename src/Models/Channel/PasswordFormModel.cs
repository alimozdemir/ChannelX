using System;
using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models.Channel
{
    public class PasswordFormModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Password { get; set; }
    }
}