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
        //private const string TrailerPhoneNo = "+46760450440";
        private const string TrailerPhoneNo = "+46701485178";

        // [bookingid, position]
        private readonly Dictionary<long, PhoneNumberAuthorizedEvent> _storagePositions;

        public SmsNotifyer(IRestClient restClient, IEventStore eventStore)
        {
            _restClient = restClient;
            _eventStore = eventStore;
            _storagePositions = new Dictionary<long, PhoneNumberAuthorizedEvent>();
        }

        // Handle events 

        public void Handle(TrailerBookedEvent @event)
        {
            var pos = GetNextAvailablePosition();
            var paddedPosition = GetPaddedPosition(pos);
            var formattedStart = new DateTime(@event.Start).ToString("yyMMddHHmm");
            var formattedEnd = new DateTime(@event.End).ToString("yyMMddHHmm");
            var formattedPhone = @event.Phone;
            var bookSms = $"1234A{paddedPosition}#{formattedPhone}#{formattedStart}#{formattedEnd}#";

            var res = _restClient.SendMessage(TwilioPhoneNo, TrailerPhoneNo, bookSms).GetAwaiter().GetResult();

            AddPhoneToRelayPosition(@event.BookingId, pos, @event.End);
        }


        public void Handle(TrailerBookingCanceledEvent @event)
        {
            var pos = _storagePositions[@event.BookingId];
            var paddedPosition = GetPaddedPosition(pos.RelayPosition);
            var cancelBookingSms = $"1234A{paddedPosition}##";
            var res = _restClient.SendMessage(TwilioPhoneNo, TrailerPhoneNo, cancelBookingSms).GetAwaiter().GetResult();

            RemovePhoneFromRelayPosition(@event.BookingId);
        }
        

        // private functions

        private void AddPhoneToRelayPosition(long bookingId, int pos, long end)
        {
            var phoneNumberAuthorizedEvent = new PhoneNumberAuthorizedEvent
            {
                Id = Constants.RelayId,
                BookingId = bookingId,
                RelayPosition = pos,
                EndTime = end
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
            var eventsLoaded = existingEvents.Count();

            var newEvents = new List<IEvent>
            {
                @event
            };
            _eventStore.SaveEventsFor<object>(Constants.RelayId, eventsLoaded, newEvents);
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
                    _storagePositions[auth.BookingId] = auth;
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
            var now = DateTime.Now;
            var currentPos = _storagePositions.Where(d => d.Value.EndTime < now.Ticks).ToDictionary(t=>t.Key, t=> t.Value.RelayPosition);

            for (int pos = 1; pos < 200; pos++)
            {
                if (!currentPos.ContainsValue(pos))
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