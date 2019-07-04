using System;

namespace Common
{
    public class LocalException:Exception
    {
        public string ResultMessage { get; set; }
        public LocalException ()
        {}
        
        public LocalException(string message, string resultMessage, params object[] data)
            : base(message)
        {
            ResultMessage = resultMessage;
            if (data.Length % 2 == 0)
                for (int i = 0; i < data.Length; i += 2)
                    Data.Add(data[i], data[i + 1] ?? "");
        }
    }
}
