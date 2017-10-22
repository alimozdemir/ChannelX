using System;

namespace ChannelX.Models.Chat
{
    public class UserModel
    {
        public UserModel(string name, bool owner)
        {
            Owner = owner;
            Name = name;
        }
        public bool Owner { get; }
        public string Name { get; }
    }
}