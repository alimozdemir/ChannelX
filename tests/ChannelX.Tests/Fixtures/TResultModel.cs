

namespace ChannelX.Tests
{

    public class TResultModel<T>
    {

        public bool Succeeded { get; set; }
        public bool Prompt { get; set; }
        public string Message { get; set; } = "Data is not valid";
        public T Data { get; set; }

    }
}