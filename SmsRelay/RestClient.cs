using System.Threading.Tasks;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SmsRelay
{
    public class RestClient : IRestClient
    {
        private readonly ITwilioRestClient _client;

        public RestClient()
        {
            _client = new TwilioRestClient(
                "AC43333d97ac2cdb701817be8887932d09",/* Credentials.TwilioAccountSid,*/
                "2e707ec2b39420561e83975f1a616a5d" /*Credentials.TwilioAuthToken*/
            );
        }

        public RestClient(ITwilioRestClient client)
        {
            _client = client;
        }

        public async Task<MessageResource> SendMessage(string from, string to, string body)
        {
            var toPhoneNumber = new PhoneNumber(to);
            var res = await MessageResource.CreateAsync(
                toPhoneNumber,
                @from: new PhoneNumber(from),
                body: body,
                client: _client);

            if (res.ErrorCode == null)
            {
                return res;
            }
            throw new SmsSendingFailedException($"{res.ErrorCode}, {res.ErrorMessage}");
        }
    }
}