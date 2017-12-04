using System;
using ChannelX.Models.Trackers;

namespace ChannelX.Models.Chat
{
    public class TextModel
    {
        public string Content { get; set; }
        public UserDetail User { get; set; }
    }
}