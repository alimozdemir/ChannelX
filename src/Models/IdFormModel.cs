using System;
using System.ComponentModel.DataAnnotations;

namespace ChannelX.Models 
{
    public class IdFormModel 
    {
        [Required]
        public int Id { get; set; }
    }
}