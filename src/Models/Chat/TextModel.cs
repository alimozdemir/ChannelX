using System;
using ChannelX.Models.Trackers;
using Newtonsoft.Json;

namespace ChannelX.Models.Chat
{
    public class TextModel
    {
        public string Content { get; set; }
        public UserDetail User { get; set; }
        public DateTime SentTime {get; set;}
        
        public override string ToString()
        {
                return JsonConvert.SerializeObject(this);
        }
    }
}