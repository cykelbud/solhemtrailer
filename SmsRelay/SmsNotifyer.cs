using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Events;

namespace SmsRelay
{
    public class SmsNotifyer :
        ISubscribeTo<TrailerBookedEvent>,
        ISubscribeTo<TrailerBookingCanceledEvent>
    {
        private readonly IRestClient _restClient;
        private readonly IEventStore _eventStore;
        private const string TwilioPhoneNo = "+46769437558";
        private const string TrailerPhoneNo = "+46760450440";
        private readonly Dictionary<long, int> _storagePositions;

        public SmsNotifyer(IRestClient restClient, IEventStore eventStore)
        {
            _restClient = restClient;
            _eventStore = eventStore;
            _storagePositions = new Dictionary<long, int>();
        }


        public void Handle(TrailerBookedEvent @event)
        {
            var pos = GetNextAvailablePosition();
            var paddedPosition = GetPaddedPosition(pos);
            var formattedStart = new DateTime(@event.Start).ToString("yyMMddHHmm");
            var formattedEnd = new DateTime(@event.End).ToString("yyMMddHHmm");
            var formattedPhone = @event.Phone;
            var bookSms = $"1234A{paddedPosition}#{formattedPhone}#{formattedStart}#{formattedEnd}#";

            var res = _restClient.SendMessage(TwilioPhoneNo, TrailerPhoneNo, bookSms).GetAwaiter().GetResult();

            AddPhoneToRelayPosition(@event.BookingId, pos);
        }


        public void Handle(TrailerBookingCanceledEvent @event)
        {
            var pos = _storagePositions[@event.BookingId];
            var paddedPosition = GetPaddedPosition(pos);
            var cancelBookingSms = $"1234A{paddedPosition}##";
            var res = _restClient.SendMessage(TwilioPhoneNo, TrailerPhoneNo, cancelBookingSms).GetAwaiter().GetResult();

            RemovePhoneFromRelayPosition(@event.BookingId);
        }
        
        private void AddPhoneToRelayPosition(long bookingId, int pos)
        {
            var phoneNumberAuthorizedEvent = new PhoneNumberAuthorizedEvent
            {
                Id = Constants.RelayId,
                BookingId = bookingId,
                RelayPosition = pos
            };

            StorePosition(phoneNumberAuthorizedEvent);
        }

        private void RemovePhoneFromRelayPosition(long bookingId)
        {
            StorePosition(new PhoneNumberRemovedEvent { BookingId = bookingId, Id = Constants.RelayId });
        }

        private void StorePosition(IEvent @event)
        {
            var existingEvents = _eventStore.LoadEventsFor<object>(Constants.RelayId).ToList();
            
            var newEvents = new List<IEvent>
            {
                @event
            };
            _eventStore.SaveEventsFor<object>(Constants.RelayId, existingEvents.Count(), newEvents);
            existingEvents.Add(@event);
            ApplyAllEvents(existingEvents);
        }


        private void ApplyAllEvents(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                if (@event.GetType() == typeof(PhoneNumberAuthorizedEvent))
                {
                    var auth = (PhoneNumberAuthorizedEvent)@event;
                    _storagePositions[auth.BookingId] = auth.RelayPosition;
                }
                if (@event.GetType() == typeof(PhoneNumberRemovedEvent))
                {
                    var auth = (PhoneNumberRemovedEvent)@event;
                    _storagePositions.Remove(auth.BookingId);
                }
            }
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
    }
}