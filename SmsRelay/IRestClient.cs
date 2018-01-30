using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace SmsRelay
{
    public interface IRestClient
    {
        Task<MessageResource> SendMessage(string from, string to, string body);
    }

    public class FakeRestClient : IRestClient
    {
        public Task<MessageResource> SendMessage(string @from, string to, string body)
        {
            return Task.FromResult((MessageResource)null);
        }
    }
}