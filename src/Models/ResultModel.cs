
namespace ChannelX.Models
{
    public class ResultModel
    {
        public bool Succeeded { get; set; }
        public bool Prompt { get; set; }
        public string Message { get; set; } = "Data is not valid";
        public object Data { get; set; }
    }
}