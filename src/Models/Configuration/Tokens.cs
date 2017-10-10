
namespace ChannelX.Models.Configuration 
{
    public class Tokens 
    {
        public string Key { get; set; }
        public string Issuer { get; set; } 
        public string Audience { get; set; }
        public int Expires { get; set; }
    }

}