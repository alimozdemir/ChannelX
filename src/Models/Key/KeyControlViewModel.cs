using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models.Account
{
    public class KeyControlViewModel
    {
        [Required]
        public string Key { get; set; }

    }
}