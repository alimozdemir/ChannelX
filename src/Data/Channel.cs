using System;
using System.Collections.Generic;

namespace ChannelX.Data
{
    public class Channel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsPrivate { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EndAt { get; set; }
        public string OwnerId { get; set; }
        public string Hash { get; set; } //A short hash value for url
        public ApplicationUser Owner { get; set; }
        public List<ChannelUser> Users { get; set; }

    }
}