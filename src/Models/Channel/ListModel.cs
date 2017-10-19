using System;

namespace ChannelX.Models.Channel
{
    public class ListModel 
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EndTime { get; set; }
        public int Popularity { get; set; }
    }
}