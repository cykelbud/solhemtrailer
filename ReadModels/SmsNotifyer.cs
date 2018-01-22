using System;
using System.Collections.Generic;
using System.Text;
using Core;
using Events;

namespace ReadModels
{
    public class SmsNotifyer:
        ISubscribeTo<TrailerBookedEvent>,
        ISubscribeTo<TrailerBookingCanceledEvent>
    {
        public void Handle(TrailerBookedEvent @event)
        {
            // skicka sms bokning
        }

        public void Handle(TrailerBookingCanceledEvent @event)
        {
            // skicka sms avbokning
        }
    }
}
