using System;

namespace SmsRelay
{
    public class SmsSendingFailedException : Exception
    {
        public string Error { get; set; }
        public SmsSendingFailedException(string s)
        {
            Error = s;
        }
    }
}