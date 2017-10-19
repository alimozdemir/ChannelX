using System;
using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models.Channel
{
    public class CreateModel
    {
        [Required]
        public string Title { get; set; }
        public bool IsPrivate { get; set; }
        public string Password { get; set; }
        
        [Required]
        public int EndAtHours { get; set; } //for now it is hours

    }
}

