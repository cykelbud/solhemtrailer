using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Events;
using Twilio.Clients;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace ReadModels
{
    public class SmsNotifyer:
        ISubscribeTo<TrailerBookedEvent>,
        ISubscribeTo<TrailerBookingCanceledEvent>
    {
        private readonly IRestClient _restClient;
        private readonly IMessageDispatcher _dispatcher;
        private const string TwilioPhoneNo = "+46769437558";
        private const string TrailerPhoneNo = "+46760450440";
        private readonly Dictionary<string, int> _storagePositions;

        public SmsNotifyer(IRestClient restClient, IMessageDispatcher dispatcher)
        {
            _restClient = restClient;
            _dispatcher = dispatcher;
            _storagePositions = new Dictionary<string, int>();
        }

        public void AddStoragePosition(string phone, int pos)
        {
            _storagePositions.Add(phone, pos);
        }

        public void RemovePhoneFromPosition(string phone)
        {
            _storagePositions.Remove(phone);
        }

        private int GetNextAvailablePosition()
        {
            for (int pos = 1; pos < 200; pos++)
            {
                if (!_storagePositions.ContainsValue(pos))
                {
                    return pos;
                }
            }
            throw new Exception("Positions full");
        }

        string GetPaddedPosition(int pos)
        {
            return pos.ToString().PadLeft(3, '0');
        }

        public void Handle(TrailerBookedEvent @event)
        {
            // skicka sms bokning
            var pos = GetNextAvailablePosition();
            var paddedPosition = GetPaddedPosition(pos);
            var formattedStart = new DateTime(@event.Start).ToString("yyMMddHHmm");
            var formattedEnd = new DateTime(@event.End).ToString("yyMMddHHmm");
            var bookSms = $"1234A{paddedPosition}#{@event.Phone}#{formattedStart}#{formattedEnd}#";

            var res = _restClient.SendMessage(TwilioPhoneNo, TrailerPhoneNo, bookSms).GetAwaiter().GetResult();

            AddStoragePosition(@event.Phone, pos);
        }

        public void Handle(TrailerBookingCanceledEvent @event)
        {
            // skicka sms avbokning
            var phone = @event.Phone;
            var pos = _storagePositions[phone];
            var paddedPosition = GetPaddedPosition(pos);
            var cancelBookingSms = $"1234A{paddedPosition}##";
            var res = _restClient.SendMessage(TwilioPhoneNo, TrailerPhoneNo, cancelBookingSms).GetAwaiter().GetResult();
            RemovePhoneFromPosition(@event.Phone);
        }
    }

    public class StorePositionForBookingCommand : ICommand
    {
        public Guid Id { get; set; }
        public int Position { get; set; }
        public string Phone { get; set; }
        public long BookingId { get; set; }
    }


    public interface IRestClient
    {
        Task<MessageResource> SendMessage(string from, string to, string body);
    }

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
                from: new PhoneNumber(from),
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

