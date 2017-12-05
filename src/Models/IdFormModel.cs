using System;
using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models 
{
    public class IdFormModel 
    {
        [Required]
        public int Id { get; set; }
    }

    public class IdStringFormModel 
    {
        [Required]
        public string Id { get; set; }
    }
}