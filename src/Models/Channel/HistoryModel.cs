using System;

namespace ChannelX.Models.Channel
{
    public class HistoryModel 
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }


        public bool IsPrivate { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EndAt { get; set; }
        public string OwnerId { get; set; }
        public string Hash { get; set; } //A short hash value for url

    }
}