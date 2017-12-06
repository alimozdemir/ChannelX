using System;
using System.Text.RegularExpressions;

namespace ChannelX.Models
{
    public class Helper
    {
        private static object _lock = new object();
        public static string ShortIdentifier()
        {
            string result = string.Empty;

            lock(_lock)
            {
                result = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", ""); 
            }
            
            return result;
        }
    }
}