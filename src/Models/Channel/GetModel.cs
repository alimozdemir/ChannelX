using System;


namespace ChannelX.Models.Channel
{
    public class GetModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}